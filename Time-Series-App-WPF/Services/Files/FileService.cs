using BiosigLibWin64;
using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using Time_Series_App_WPF.Enums;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Files
{
    public class FileService : IFileService
    {
        private readonly List<SignalChartDataFloat> _channelsData;
        private string? _fileID;

        public List<SignalChartDataFloat> ChannelsData { get { return _channelsData; } }

        public FileService()
        {
            _channelsData = new List<SignalChartDataFloat>();
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

                var channelData = new SignalChartDataFloat()
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

            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                _fileID = BitConverter.ToString(hash).Replace("-", "");
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

        public async Task ExportFile(string path, ObservableCollection<MadeAnnotation> madeAnnotations)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField(_fileID);
                csv.NextRecord();
                await csv.WriteRecordsAsync(madeAnnotations);
            }
        }

        public async Task ExportFile(string path, IEnumerable<double[]> data)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var doubleArray in data)
                {
                    await csv.WriteRecordsAsync(doubleArray);
                    csv.WriteField("", false);
                }
            }
        }

        public async Task<List<MadeAnnotation>?> ImportFile(string path)
        {
            List<MadeAnnotation> list;

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<MadeAnnotationMap>();

                await csv.ReadAsync();
                var fileID = csv.GetField<string>(0);
                if (fileID != _fileID)
                    return null;

                list = await csv.GetRecordsAsync<MadeAnnotation>().ToListAsync();
            }

            return list;
        }
    }
}
