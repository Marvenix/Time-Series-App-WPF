using ScottPlot.WPF;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.Services.Charts;
using Time_Series_App_WPF.Services.Files;
using ScottPlot.AxisRules;
using Time_Series_App_WPF.View;
using System.Windows.Input;
using Time_Series_App_WPF.Services.Annotations;
using ScottPlot.Plottables;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly IFileService _fileService;
        private readonly IChartService<SignalChartData> _signalChartService;
        private readonly ListDataHolder<Annotation> _annotationListDataHolder;
        private readonly IAnnotationService _annotationService;
        private ObservableCollection<WpfPlot> _mainWindowPlots;
        private ObservableCollection<Annotation> _annotationTypes; 
        private ObservableCollection<MadeAnnotation> _madeAnnotations;

        [ObservableProperty]
        private Annotation? _selectedAnnotation;

        [ObservableProperty]
        private bool _isEnabledAnnotationMakeModeButton;

        [ObservableProperty]
        private bool _isCheckedScrollNavigationButton;

        [ObservableProperty]
        private bool _isCheckedAnnotationMakeModeButton;

        public event EventHandler? AnnotationWindowRequest;
        public event EventHandler? ProgramInfoWindowRequest;
        public ObservableCollection<WpfPlot> MainWindowPlots { get { return _mainWindowPlots; } }
        public ObservableCollection<Annotation> AnnotationTypes { get { return _annotationTypes; } }
        public ObservableCollection<MadeAnnotation> MadeAnnotations { get { return _madeAnnotations; } }


        public MainWindowViewModel(IFileService fileService, IChartService<SignalChartData> signalChartService, ListDataHolder<Annotation> listDataHolder, IAnnotationService annotationService)
        {
            _fileService = fileService;
            _signalChartService = signalChartService;
            _mainWindowPlots = new ObservableCollection<WpfPlot>();
            _annotationTypes = new ObservableCollection<Annotation>();
            _annotationListDataHolder = listDataHolder;
            _annotationService = annotationService;
            _madeAnnotations = new ObservableCollection<MadeAnnotation>();
            _isCheckedScrollNavigationButton = true;
        }

        [RelayCommand]
        private void RemoveMadeAnnotation(Guid id)
        {
            var madeAnnotation = _madeAnnotations.FirstOrDefault(x => x.Id == id);

            if (madeAnnotation != null)
            {
                var plotControl = _mainWindowPlots.FirstOrDefault(x => x.Uid == madeAnnotation.ChannelId);

                if (plotControl != null)
                {
                    var graphicAnnotation = plotControl.Plot.PlottableList.FirstOrDefault(x => x is Polygon && ((Polygon)x).Label == id.ToString());

                    if (graphicAnnotation != null)
                    {
                        plotControl.Plot.PlottableList.Remove(graphicAnnotation);
                        _madeAnnotations.Remove(madeAnnotation);
                        plotControl.Refresh();
                        return;
                    }
                }
            }

            ShowMessageBox((string)Application.Current.TryFindResource("MainWindow-RemoveMadeAnnotation-Error"), (string)Application.Current.TryFindResource("Error"));
        }

        [RelayCommand]
        private void AddMadeAnnotation(MadeAnnotation madeAnnotation)
        {
            _madeAnnotations.Add(madeAnnotation);
        }

        [RelayCommand]
        private void SelectionChanged()
        {
            IsEnabledAnnotationMakeModeButton = SelectedAnnotation != null ? true : false;
            if (!IsEnabledAnnotationMakeModeButton)
                IsCheckedAnnotationMakeModeButton = false;
        }

        private void DisplayAnnotationWindow()
        {
            _annotationListDataHolder.Data = _annotationTypes;
            this.AnnotationWindowRequest?.Invoke(this, new EventArgs());
        }

        private void DisplayProgramInfoWindow()
        {
            this.ProgramInfoWindowRequest?.Invoke(this, new EventArgs());
        }

        private void SetupAndShowCharts()
        {
            var listOfCharts = _signalChartService.SetupChartsFromChartsData(_fileService.ChannelsData);

            foreach (var chart in listOfCharts)
            {
                chart.Height = 200;

                int controlNumber = 0;

                while (controlNumber < listOfCharts.Count)
                {
                    int i = controlNumber;
                    if (chart != listOfCharts[i])
                    {
                        chart.Plot.RenderManager.AxisLimitsChanged += (s, e) => { ApplyLayoutToOtherPlot(chart, listOfCharts[i]); };
                    }
                    controlNumber++;
                }

                LockedVertical verticalRule = new LockedVertical(chart.Plot.Axes.Left);
                chart.Plot.Axes.Rules.Add(verticalRule);

                var XAxisMax = _fileService.ChannelsData[listOfCharts.IndexOf(chart)].XAxisMax;

                if (XAxisMax < 60)
                {
                    chart.Plot.Axes.SetLimitsX(0, XAxisMax);
                }
                else
                {
                    chart.Plot.Axes.SetLimitsX(0, 60);
                }

                chart.Refresh();
                _mainWindowPlots.Add(chart);
            }
        }

        private void ApplyLayoutToOtherPlot(IPlotControl source, IPlotControl dest)
        {
            AxisLimits axesBefore = dest.Plot.Axes.GetLimits();
            dest.Plot.Axes.SetLimitsX(source.Plot.Axes.GetLimits());
            AxisLimits axesAfter = dest.Plot.Axes.GetLimits();
            if (axesBefore != axesAfter)
            {
                dest.Refresh();
            }
        }

        [RelayCommand]
        private void ShowAnnotationWindow()
        {
            DisplayAnnotationWindow();
        }

        [RelayCommand]
        private void ShowProgramInfoWindow()
        {
            DisplayProgramInfoWindow();
        }

        [RelayCommand]
        private void ChangeLanguage(string language)
        {
            try
            {
                ResourceDictionary dictionary = new ResourceDictionary();

                switch (language)
                {
                    case "en":
                        dictionary.Source = new Uri(@"..\LanguageResources\Resources.en.xaml", UriKind.Relative);
                        break;
                    case "pl":
                        dictionary.Source = new Uri(@"..\LanguageResources\Resources.pl.xaml", UriKind.Relative);
                        break;
                    default:
                        dictionary.Source = new Uri(@"..\LanguageResources\Resources.en.xaml", UriKind.Relative);
                        break;
                }
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
            catch(Exception)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("ChangeLanguage-Exception"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        [RelayCommand]
        private void OpenFile(string fileName)
        {
            try
            {
                foreach (var plot in _mainWindowPlots)
                {
                    plot.Plot.Dispose();
                }

                _madeAnnotations.Clear();
                _mainWindowPlots.Clear();
                _fileService.OpenFile(fileName);

                SetupAndShowCharts();
            }
            catch (FileLoadException)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("OpenFile-FileLoadException"), (string)Application.Current.TryFindResource("Error"));
            }
            catch (ArgumentException)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("OpenFile-ArgumentException"), (string)Application.Current.TryFindResource("Error"));
            }
            catch (Exception)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("Exception-Unknown"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        [RelayCommand]
        private async Task GetAnnotationTypes()
        {
            _annotationTypes.Clear();
            var annotations = await _annotationService.GetAnnotationsAsync();
            foreach (var annotation in annotations)
            {
                _annotationTypes.Add(annotation);
            }
        }
    }
}
