using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Files
{
    public interface IFileService
    {
        public List<SignalChartData> ChannelsData { get; }
        public void OpenFile(string path);
        public Task ExportFile(string path, ObservableCollection<MadeAnnotation> madeAnnotations);
        public Task<List<MadeAnnotation>?> ImportFile(string path);
    }
}
