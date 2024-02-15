using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot.AxisRules;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Time_Series_App_WPF.Enums;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.Services.Analysis;
using Time_Series_App_WPF.Services.Charts;
using Time_Series_App_WPF.Services.Files;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class AnalyzeResultsWindowViewModel : BaseViewModel
    {
        private readonly DataHolder<AnalysisMethod> _analysisMethodDataHolder;
        private readonly DataHolder<WpfPlot> _wpfPlotDataHolder;
        private readonly DataHolder<int> _intDataHolder;
        private readonly ListDataHolder<MadeAnnotation> _madeAnnotationListDataHolder;
        private readonly IAnalysisService _analysisService;
        private readonly IChartService<SignalChartData> _chartService;
        private readonly IFileService _fileService;

        private readonly List<MadeAnnotation> _madeAnnotations;
        private readonly AnalysisMethod _analysisMethod;
        private readonly WpfPlot _wpfPlotToAnalyze;

        [ObservableProperty]
        private WpfPlot? _mergedMadeAnnotationPlot;

        [ObservableProperty]
        private WpfPlot? _analysisResultsPlot;

        private (double[], double[]) _result;
        private readonly int _stepSize;

        public AnalyzeResultsWindowViewModel(DataHolder<AnalysisMethod> analysisMethodDataHolder, ListDataHolder<MadeAnnotation> madeAnnotationListDataHolder, 
            DataHolder<WpfPlot> wpfPlotDataHolder, IAnalysisService analysisService, IChartService<SignalChartData> chartService, IFileService fileService, DataHolder<int> intDataHolder)
        {
            _analysisMethodDataHolder = analysisMethodDataHolder;
            _madeAnnotationListDataHolder = madeAnnotationListDataHolder;
            _analysisMethod = _analysisMethodDataHolder.Value;
            _madeAnnotations = (List<MadeAnnotation>)_madeAnnotationListDataHolder.Data!;
            _wpfPlotDataHolder = wpfPlotDataHolder;
            _wpfPlotToAnalyze = _wpfPlotDataHolder.Value!;
            _analysisService = analysisService;
            _chartService = chartService;
            _fileService = fileService;
            _intDataHolder = intDataHolder;
            _stepSize = _intDataHolder.Value;
        }

        [RelayCommand]
        private void PerformAnalysis()
        {
            try
            {
                switch (_analysisMethod)
                {
                    case AnalysisMethod.CrossCorrelation:
                        _result = _analysisService.CrossCorrelationAnalysis(_madeAnnotations, _wpfPlotToAnalyze);
                        break;

                    case AnalysisMethod.DTW:
                        _result = _analysisService.DTWAnalysis(_madeAnnotations, _wpfPlotToAnalyze, _stepSize);
                        break;
                }

                if (_result.Item1 == null || _result.Item2 == null)
                    ShowMessageBox((string)Application.Current.TryFindResource("PerformAnalysis-Error"), (string)Application.Current.TryFindResource("Error"));

                ShowResults();
            }
            catch(Exception)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("PerformAnalysis-Error"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        private void ShowResults()
        {
            var chartDataList = new List<SignalChartData>()
            {
                new SignalChartDataDouble
                {
                    Values = _result.Item1,
                    Label = "Merged Annotation",
                    Frequency = 1,
                    XAxisMin = 0,
                    XAxisMax = _result.Item1.Length

                },

                new SignalChartDataDouble
                {
                    Values = _result.Item2,
                    Label = "Analysis Results",
                    Frequency = 1,
                    XAxisMin = 0,
                    XAxisMax = _result.Item2.Length
                }
            };

            var plotList = _chartService.SetupChartsFromChartsData(chartDataList);

            foreach(var plot in plotList)
            {
                plot.Height = 175;
                LockedVertical verticalRule = new LockedVertical(plot.Plot.Axes.Left);
                plot.Plot.Axes.Rules.Add(verticalRule);
                plot.Refresh();
            }

            MergedMadeAnnotationPlot = plotList[0];
            AnalysisResultsPlot = plotList[1];
        }

        [RelayCommand]
        private async Task SaveResults(string fileName)
        {
            try
            {
                var data = new List<double[]>()
                {
                    _result.Item1,
                    _result.Item2
                };

                await _fileService.ExportFile(fileName, data);
            }
            catch (Exception)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("Exception-SaveResults"), (string)Application.Current.TryFindResource("Error"));
            }
        }
    }
}
