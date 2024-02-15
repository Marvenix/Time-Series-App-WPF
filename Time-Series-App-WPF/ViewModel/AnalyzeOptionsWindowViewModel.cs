using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Enums;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class AnalyzeOptionsWindowViewModel : BaseViewModel
    {
        private readonly ListDataHolder<MadeAnnotation> _madeAnnotationListDataHolder;
        private readonly DataHolder<AnalysisMethod> _analysisMethodDataHolder;
        private readonly DataHolder<int> _intDataHolder;
        private readonly List<MadeAnnotation> _madeAnnotationList;
        private readonly ObservableCollection<Annotation> _annotationTypes;

        [ObservableProperty]
        private bool _isRadioButtonOneChecked;

        [ObservableProperty]
        private bool _isRadioButtonTwoChecked;

        [ObservableProperty]
        private Annotation? _selectedAnnotation;

        [ObservableProperty]
        private string? _text;

        public ObservableCollection<Annotation> AnnotationTypes { get { return _annotationTypes; } }
        public event EventHandler? AnalyzeResultsWindowRequest;

        public AnalyzeOptionsWindowViewModel(ListDataHolder<MadeAnnotation> madeAnnotationListDataHolder, DataHolder<AnalysisMethod> analysisMethodDataHolder, DataHolder<int> intDataHolder)
        {
            _madeAnnotationListDataHolder = madeAnnotationListDataHolder;
            _madeAnnotationList = (List<MadeAnnotation>)_madeAnnotationListDataHolder.Data!;
            _annotationTypes = new ObservableCollection<Annotation>(_madeAnnotationList.Select(x => x.Annotation).Distinct()!);
            _analysisMethodDataHolder = analysisMethodDataHolder;
            _intDataHolder = intDataHolder;
        }

        [RelayCommand]
        private void PerformAnalysis()
        {
            if (IsRadioButtonOneChecked)
            {
                _analysisMethodDataHolder.Value = AnalysisMethod.DTW;
                _intDataHolder.Value = int.Parse(Text!);
            }

            if (IsRadioButtonTwoChecked)
                _analysisMethodDataHolder.Value = AnalysisMethod.CrossCorrelation;

            _madeAnnotationListDataHolder.Data = _madeAnnotationList.Where(x => x.Annotation == SelectedAnnotation).ToList();
            DisplayAnalyzeResultsWindow();
        }

        private void DisplayAnalyzeResultsWindow()
        {
            AnalyzeResultsWindowRequest?.Invoke(this, new EventArgs());
        }
    }
}
