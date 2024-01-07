using BiosigLibWin64;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Files
{
    public class FileService : IFileService
    {
        private HDRTYPE? _header;
        private readonly List<SignalChartData> _channelsData;

        public List<SignalChartData> ChannelsData { get { return _channelsData; } }

        public FileService()
        {
            _channelsData = new List<SignalChartData>();
        }

        private void BufferData(double[] data, int numberOfChannels)
        {
            _channelsData.Clear();
            var samplesPerChannel = _header.SPR * _header.NRec;

            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                var channelInfo = Biosig.biosig_get_channel(_header, channel);

                var channelData = new SignalChartData()
                {
                    Values = new float[samplesPerChannel]
                };

                channelData.Label = channelInfo.Label;
                channelData.YUnit = Biosig.PhysDim3(channelInfo.PhysDimCode);
                channelData.Frequency = (float)_header.SampleRate;

                for (int sample = 0; sample < samplesPerChannel; sample++)
                {
                    channelData.Values[sample] = (float)data[sample + channel * samplesPerChannel];
                }

                _channelsData.Add(channelData);
            }
        }

        public void ReadFile(string path)
        {
            _header = Biosig.constructHDR(0, 0);
            Biosig.sopen(path, "r", _header);

            if (_header.AS.B4C_ERRNUM != B4C_ERROR.B4C_NO_ERROR)
            {
                Biosig.sclose(_header);
                Biosig.destructHDR(_header);
                throw new FileLoadException();
            }

            var samplesPerRecord = _header.SPR;
            var numberOfRecords = _header.NRec;
            var numberOfChannels = Biosig.biosig_get_number_of_channels(_header);

            double[] data = new double[samplesPerRecord * numberOfRecords * numberOfChannels];
            _header.FLAG.ROW_BASED_CHANNELS = '\0';
            Biosig.sread(data, 0, (uint)numberOfRecords, _header);

            BufferData(data, numberOfChannels);

            Biosig.sclose(_header);
            Biosig.destructHDR(_header);
        }
    }
}
