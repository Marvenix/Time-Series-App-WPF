using ScottPlot.WPF;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;
using ScottPlot.AxisRules;
using Time_Series_App_WPF.View;

namespace Time_Series_App_WPF.Services.Charts
{
    public class SignalChartService : IChartService<SignalChartData>
    {
        public List<WpfPlot> SetupChartsFromChartsData(List<SignalChartData> chartsData)
        {
            var charts = new List<WpfPlot>();

            foreach (var chartData in chartsData)
            {
                var chart = SetupChartFromChartData(chartData);
                chart.Uid = chartsData.IndexOf(chartData).ToString();
                charts.Add(chart);
            }

            return charts;
        }

        public WpfPlot SetupChartFromChartData(SignalChartData chartData)
        {
            if (chartData.Frequency <= 0 || chartData.XAxisMax < chartData.XAxisMin || chartData.YAxisMax < chartData.YAxisMin)
            {
                throw new ArgumentException();
            }

            var wpfPlot = new WpfPlot();

            if (chartData.XUnit != null)
            {
                wpfPlot.Plot.XLabel(chartData.XUnit);
            }

            if (chartData.YUnit != null)
            {
                wpfPlot.Plot.YLabel(chartData.YUnit);
            }

            if (chartData.Label != null)
            {
                wpfPlot.Plot.Title(chartData.Label);
            }

            wpfPlot.Plot.Add.Signal(chartData.Values, 1 / chartData.Frequency);

            wpfPlot.Plot.Axes.Rules.Clear();

            wpfPlot.Plot.Axes.Rules.Add(new MaximumBoundary(wpfPlot.Plot.Axes.Bottom, wpfPlot.Plot.Axes.Left, AxisLimits.HorizontalOnly(chartData.XAxisMin, chartData.XAxisMax)));

            return wpfPlot;
        }
    }
}
