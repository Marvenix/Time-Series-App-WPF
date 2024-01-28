using BiosigLibWin64;
using ScottPlot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Markup;
using Time_Series_App_WPF.Enums;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Files
{
    public class FileService : IFileService
    {
        private readonly List<SignalChartData> _channelsData;

        public List<SignalChartData> ChannelsData { get { return _channelsData; } }

        public FileService()
        {
            _channelsData = new List<SignalChartData>();
        }

        private int AssignValuesToChartData(double[] data, int channelLength, int startArrayIndex)
        {
            var doubleDataIndex = 0;
            for (int i = 0; i < _channelsData.Count; i++)
            {
                for (int j = startArrayIndex; j < startArrayIndex + channelLength; j++)
                {
                    _channelsData[i].Values[j] = (float)data[doubleDataIndex];
                    doubleDataIndex++;
                }
            }
            startArrayIndex += channelLength;

            return startArrayIndex;
        }

        private void ReadFile(PowerOfTwo powerOfTwo, HDRTYPE header, int samplesPerRecord, int numberOfRecords, int numberOfChannels)
        {
            var allSamples = numberOfRecords * samplesPerRecord * numberOfChannels;

            var samplesPerRecordAllChannels = samplesPerRecord * numberOfChannels;

            var arrayPool = ArrayPool<double>.Shared;

            if (allSamples/(int)powerOfTwo <= 1)
            {
                double[] data = arrayPool.Rent(allSamples);
                Biosig.sread(data, 0, (uint)numberOfRecords, header);
                AssignValuesToChartData(data, numberOfRecords * samplesPerRecord, 0);
                arrayPool.Return(data);
            }
            else
            {
                uint recordsPerRead = (uint)((int)powerOfTwo / samplesPerRecordAllChannels);
                uint lastRecords = (uint)(numberOfRecords % recordsPerRead);
                var loopIterations = (numberOfRecords / recordsPerRead) + 1;
                var startArrayIndex = 0;

                for (int i = 0; i < loopIterations; i++)
                {
                    uint start = (uint)(i * recordsPerRead);

                    if (i == loopIterations - 1)
                    {
                        recordsPerRead = lastRecords;
                    } 

                    double[] data = arrayPool.Rent((int)powerOfTwo);
                    Biosig.sread(data, start, recordsPerRead, header);
                    header.AS.first += recordsPerRead;
                    startArrayIndex = AssignValuesToChartData(data, (int)(recordsPerRead * samplesPerRecord), startArrayIndex);
                    arrayPool.Return(data);
                }
            }
        }

        private void PrepareChartDataObjects(HDRTYPE header, int numberOfChannels, int samplesPerChannel)
        {
            _channelsData.Clear();

            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                var channelInfo = Biosig.biosig_get_channel(header, channel);

                var channelData = new SignalChartData()
                {
                    Values = new float[samplesPerChannel]
                };

                channelData.Label = channelInfo.Label;
                channelData.YUnit = Biosig.PhysDim3(channelInfo.PhysDimCode);
                channelData.Frequency = (float)header.SampleRate;
                channelData.XAxisMin = 0;
                channelData.XAxisMax = samplesPerChannel/channelData.Frequency;
                channelData.YAxisMin = channelData.Values.Min();
                channelData.YAxisMax = channelData.Values.Max();

                _channelsData.Add(channelData);
            }
        }

        public void OpenFile(string path)
        {
            var header = Biosig.constructHDR(0, 0);
            Biosig.sopen(path, "r", header);

            if (header.AS.B4C_ERRNUM != B4C_ERROR.B4C_NO_ERROR)
            {
                Biosig.sclose(header);
                Biosig.destructHDR(header);
                throw new FileLoadException();
            }

            var samplesPerRecord = header.SPR;
            var numberOfRecords = header.NRec;
            var samplesPerChannel = samplesPerRecord * numberOfRecords;
            var numberOfChannels = Biosig.biosig_get_number_of_channels(header);
            header.FLAG.ROW_BASED_CHANNELS = '\0';

            PrepareChartDataObjects(header, numberOfChannels, (int)samplesPerChannel);

            ReadFile(PowerOfTwo.TwentyTwo, header, (int)samplesPerRecord, (int)numberOfRecords, numberOfChannels);


            Biosig.sclose(header);
            Biosig.destructHDR(header);
        }
    }
}
