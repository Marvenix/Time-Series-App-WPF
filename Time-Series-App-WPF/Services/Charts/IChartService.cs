using ScottPlot.WPF;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.View;

namespace Time_Series_App_WPF.Services.Charts
{
    public interface IChartService<TChartData> where TChartData : ChartData
    {
        public List<WpfPlot> SetupChartsFromChartsData(List<TChartData> chartsData);
        public WpfPlot SetupChartFromChartData(TChartData chartData);
    }
}
