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
using CommunityToolkit.Mvvm.Messaging;
using Time_Series_App_WPF.Messages;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class MainWindowViewModel : BaseViewModel, IRecipient<RemoveAnnotationItemMessage>, IRecipient<EditMadeAnnotationItemMessage>
    {
        private readonly IFileService _fileService;
        private readonly IChartService<SignalChartData> _signalChartService;
        private readonly DataHolder<WpfPlot> _wpfPlotDataHolder;
        private readonly ListDataHolder<Annotation> _annotationListDataHolder;
        private readonly ListDataHolder<MadeAnnotation> _madeAnnotationListDataHolder;
        private readonly IAnnotationService _annotationService;
        private readonly IMessenger _messenger;
        private ObservableCollection<WpfPlot> _mainWindowPlots;
        private ObservableCollection<Annotation> _annotationTypes;
        private ObservableCollection<MadeAnnotation> _madeAnnotations;

        private CollectionViewSource _plotsToAnalyzeCollectionSource;

        [ObservableProperty]
        private ICollectionView _plotsToAnalyzeCollection;

        private bool _isFileLoaded;

        [ObservableProperty]
        private Annotation? _selectedAnnotation;

        [ObservableProperty]
        private WpfPlot? _selectedPlotToAnalyze;

        [ObservableProperty]
        private bool _isCheckedScrollNavigationButton;

        [ObservableProperty]
        private bool _isEnabledAnnotationMakeModeButton;

        [ObservableProperty]
        private bool _isCheckedAnnotationMakeModeButton;

        [ObservableProperty]
        private bool _isEnabledAnnotationEditModeButton;

        [ObservableProperty]
        private bool _isCheckedAnnotationEditModeButton;

        [ObservableProperty]
        private bool _isEnabledImportButton;

        public event EventHandler? AnnotationWindowRequest;
        public event EventHandler? ProgramInfoWindowRequest;
        public event EventHandler? ChangeViewValuesRequest;
        public event EventHandler? AnalyzeOptionsWindowRequest;
        public ObservableCollection<WpfPlot> MainWindowPlots { get { return _mainWindowPlots; } }
        public ObservableCollection<Annotation> AnnotationTypes { get { return _annotationTypes; } }
        public ObservableCollection<MadeAnnotation> MadeAnnotations { get { return _madeAnnotations; } }



        public MainWindowViewModel(IFileService fileService, IChartService<SignalChartData> signalChartService, ListDataHolder<Annotation> annotationListDataHolder, 
            IAnnotationService annotationService, IMessenger messenger, ListDataHolder<MadeAnnotation> madeAnnotationListDataHolder, DataHolder<WpfPlot> wpfPlotDataHolder)
        {
            _fileService = fileService;
            _signalChartService = signalChartService;
            _mainWindowPlots = new ObservableCollection<WpfPlot>();
            _annotationTypes = new ObservableCollection<Annotation>();
            _annotationListDataHolder = annotationListDataHolder;
            _annotationService = annotationService;
            _madeAnnotations = new ObservableCollection<MadeAnnotation>();
            _messenger = messenger;
            _messenger.Register<RemoveAnnotationItemMessage>(this);
            _messenger.Register<EditMadeAnnotationItemMessage>(this);
            _isCheckedScrollNavigationButton = true;
            _plotsToAnalyzeCollectionSource = new CollectionViewSource();
            _plotsToAnalyzeCollectionSource.Source = _mainWindowPlots;
            _plotsToAnalyzeCollection = _plotsToAnalyzeCollectionSource.View;
            _plotsToAnalyzeCollection.Filter = FilterPlotsForAnalyze;
            _madeAnnotations.CollectionChanged += UpdatePlotsForAnalyze;
            _madeAnnotationListDataHolder = madeAnnotationListDataHolder;
            _wpfPlotDataHolder = wpfPlotDataHolder;
        }

        private void UpdatePlotsForAnalyze(object? sender, NotifyCollectionChangedEventArgs e)
        {
            PlotsToAnalyzeCollection.Refresh();
        }

        private bool FilterPlotsForAnalyze(object item)
        {
            if (item is WpfPlot plotControl)
            {
                return _madeAnnotations.Where(x => x.ChannelId == plotControl.Uid).Count() > 0;
            }
            return false;
        }

        private void ChangeViewValues()
        {
            this.ChangeViewValuesRequest?.Invoke(this, new EventArgs());
        }
        private void EditMadeAnnotationPolygon(MadeAnnotation madeAnnotation)
        {
            var plotControl = _mainWindowPlots.FirstOrDefault(x => x.Uid == madeAnnotation.ChannelId);

            if (plotControl != null)
            {
                var graphicAnnotation = plotControl.Plot.PlottableList.FirstOrDefault(x => x is Polygon && ((Polygon)x).Label == madeAnnotation.Id.ToString());

                if (graphicAnnotation != null)
                {
                    var colorToConvert = (System.Windows.Media.Color)ColorConverter.ConvertFromString(madeAnnotation.Annotation!.Color);
                    ((Polygon)graphicAnnotation).FillStyle.Color = new ScottPlot.Color(colorToConvert.R, colorToConvert.G, colorToConvert.B).WithAlpha(.3);
                    plotControl.Refresh();
                    return;
                }
            }

            ShowMessageBox((string)Application.Current.TryFindResource("MainWindow-EditMadeAnnotationPolygons-Error"), (string)Application.Current.TryFindResource("Error"));
        }
        public void Receive(EditMadeAnnotationItemMessage message)
        {
            var madeAnnotationsToEdit = _madeAnnotations.Where(x => x.Annotation == message.Value).ToList();

            if (madeAnnotationsToEdit != null)
            {
                foreach (var madeAnnotation in madeAnnotationsToEdit)
                {
                    EditMadeAnnotationPolygon(madeAnnotation);

                    var madeAnnotationInList = _madeAnnotations.First(x => x.Id == madeAnnotation.Id);
                    var index = _madeAnnotations.IndexOf(madeAnnotationInList);

                    _madeAnnotations[index] = madeAnnotationInList;
                }
            }
        }
        public void Receive(RemoveAnnotationItemMessage message)
        {
            var madeAnnotationsToRemove = _madeAnnotations.Where(x => x.Annotation == message.Value).ToList();

            if (madeAnnotationsToRemove != null)
            {
                foreach (var madeAnnotation in madeAnnotationsToRemove)
                {
                    RemoveMadeAnnotation(madeAnnotation.Id);
                }
            }
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
                        ChangeViewValues();
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
        private void EditMadeAnnotation(MadeAnnotation editedAnnotation)
        {
            var madeAnnotation = _madeAnnotations.FirstOrDefault(x => x.Id == editedAnnotation.Id);

            if (madeAnnotation != null)
            {
                madeAnnotation.Start = editedAnnotation.Start;
                madeAnnotation.End = editedAnnotation.End;
                madeAnnotation.Duration = editedAnnotation.Duration;

                var index = _madeAnnotations.IndexOf(madeAnnotation);
                _madeAnnotations[index] = madeAnnotation;
            }
            else
            {
                ShowMessageBox((string)Application.Current.TryFindResource("MainWindow-EditMadeAnnotation-Error"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        [RelayCommand]
        private void SelectionChanged()
        {
            IsEnabledAnnotationMakeModeButton = SelectedAnnotation != null && _isFileLoaded ? true : false;
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

        private void DisplayAnalyzeOptionsWindow()
        {
            this.AnalyzeOptionsWindowRequest?.Invoke(this, new EventArgs());
        }

        private void SetupAndShowCharts()
        {
            var listOfCharts = _signalChartService.SetupChartsFromChartsData(new List<SignalChartData>(_fileService.ChannelsData));

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
        private void ShowAnalyzeOptionsWindow()
        {
            var channelId = SelectedPlotToAnalyze!.Uid;
            var madeAnnotationList = _madeAnnotations.Where(x => x.ChannelId == channelId).ToList();
            _wpfPlotDataHolder.Value = SelectedPlotToAnalyze;
            _madeAnnotationListDataHolder.Data = madeAnnotationList;
            DisplayAnalyzeOptionsWindow();
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

                if (!_isFileLoaded)
                {
                    IsEnabledImportButton = true;
                    IsEnabledAnnotationEditModeButton = true;
                    _isFileLoaded = true;
                    SelectionChanged();
                }
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
        private async Task ExportFile(string fileName)
        {
            try
            {
                await _fileService.ExportFile(fileName, MadeAnnotations);
            }
            catch (Exception)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("Exception-ExportFile"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        [RelayCommand]
        private async Task ImportFile(string fileName)
        {
            List<MadeAnnotation>? importedMadeAnnotations = null;
            try
            {
                importedMadeAnnotations = await _fileService.ImportFile(fileName);
            }
            catch (Exception)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("Error-InvalidData"), (string)Application.Current.TryFindResource("Error"));
                return;
            }

            if (importedMadeAnnotations == null)
            {
                ShowMessageBox((string)Application.Current.TryFindResource("Error-WrongFile"), (string)Application.Current.TryFindResource("Error"));
                return;
            }

            if (!ValidateData(importedMadeAnnotations))
            {
                ShowMessageBox((string)Application.Current.TryFindResource("Error-InvalidData"), (string)Application.Current.TryFindResource("Error"));
                return;
            }

            foreach (var importedMadeAnnotation in importedMadeAnnotations!)
            {
                var annotation = _annotationTypes.FirstOrDefault(x => x.Id == importedMadeAnnotation.Annotation!.Id);

                if (annotation == null || importedMadeAnnotation.Annotation!.Name != annotation.Name 
                    || importedMadeAnnotation.Annotation.Color != annotation.Color)
                {
                    var newAnnotation = new Annotation
                    {
                        Name = importedMadeAnnotation.Annotation!.Name,
                        Color = importedMadeAnnotation.Annotation!.Color
                    };
                    await _annotationService.CreateAsync(newAnnotation);
                    _annotationTypes.Add(newAnnotation);

                    importedMadeAnnotation.Annotation = newAnnotation;
                }
                else
                {
                    importedMadeAnnotation.Annotation = annotation;
                }

                if (_madeAnnotations.FirstOrDefault(x => x.Id == importedMadeAnnotation.Id) != null || importedMadeAnnotation.Id == Guid.Empty)
                    importedMadeAnnotation.Id = Guid.NewGuid();


                _madeAnnotations.Add(importedMadeAnnotation);
                CreateGraphicAnnotation(importedMadeAnnotation);
            }

        }

        private void CreateGraphicAnnotation(MadeAnnotation madeAnnotation)
        {
            var plotControl = _mainWindowPlots.FirstOrDefault(x => x.Uid == madeAnnotation.ChannelId);
            var minY = plotControl!.Plot.Axes.Left.Min;
            var maxY = plotControl!.Plot.Axes.Left.Max;
            var colorToConvert = (System.Windows.Media.Color)ColorConverter.ConvertFromString(madeAnnotation.Annotation!.Color);

            var rectangle = plotControl!.Plot.Add.Rectangle(madeAnnotation.Start, madeAnnotation.End, minY, maxY);

            rectangle.FillStyle.Color = new ScottPlot.Color(colorToConvert.R, colorToConvert.G, colorToConvert.B).WithAlpha(.3);
            rectangle.Label = madeAnnotation.Id.ToString();
            rectangle.LineStyle = new LineStyle
            {
                AntiAlias = true,
                Color = ScottPlot.Colors.Black,
                Pattern = LinePattern.Solid,
                Width = 2,
                IsVisible = false
            };
            rectangle.IsVisible = true;

            plotControl.Refresh();
        }

        private bool ValidateData(List<MadeAnnotation> importedMadeAnnotations)
        {
            foreach (var importedMadeAnnotation in importedMadeAnnotations)
            {
                if (importedMadeAnnotation.Start > importedMadeAnnotation.End)
                    return false;

                if ((importedMadeAnnotation.End - importedMadeAnnotation.Start) != importedMadeAnnotation.Duration)
                    return false;

                if (importedMadeAnnotation.Annotation == null)
                    return false;

                if (importedMadeAnnotation.Annotation.Name == null || importedMadeAnnotation.Annotation.Color == null)
                    return false;

                var plotControl = _mainWindowPlots.FirstOrDefault(x => x.Uid == importedMadeAnnotation.ChannelId);
                
                if (plotControl == null)
                    return false;

                if (plotControl.Plot.Axes.Title.Label.Text !=  importedMadeAnnotation.Channel)
                    return false;
            }

            return true;
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
