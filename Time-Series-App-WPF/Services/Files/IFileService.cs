using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Files
{
    public interface IFileService
    {
        public List<SignalChartData> ChannelsData { get; }
        public void ReadFile(string path);
    }
}
