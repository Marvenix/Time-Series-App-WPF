using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Time_Series_App_WPF.Messages;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.Services.Annotations;
using Time_Series_App_WPF.View;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class AnnotationWindowViewModel : BaseViewModel, IRecipient<AddItemMessage>, IRecipient<EditItemMessage>
    {
        private readonly ObservableCollection<Annotation> _annotationTypes;
        private readonly IAnnotationService _annotationService;
        private readonly IMessenger _messenger;
        private readonly DataHolder<Annotation> _annotationDataHolder;
        private readonly ListDataHolder<Annotation> _annotationListDataHolder;

        public ObservableCollection<Annotation> AnnotationTypes { get { return _annotationTypes; } }
        public event EventHandler? AddAnnotationWindowRequest;
        public event EventHandler? EditAnnotationWindowRequest;

        public AnnotationWindowViewModel(IAnnotationService annotationService, IMessenger messenger, DataHolder<Annotation> annotationDataHolder, ListDataHolder<Annotation> annotationListDataHolder)
        {
            _annotationService = annotationService;
            _annotationDataHolder = annotationDataHolder;
            _annotationListDataHolder = annotationListDataHolder;
            _messenger = messenger;
            _messenger.Register<AddItemMessage>(this);
            _messenger.Register<EditItemMessage>(this);
            _annotationTypes = (ObservableCollection<Annotation>)_annotationListDataHolder.Data!;
        }

        private void DisplayAddAnnotationWindow()
        {
            this.AddAnnotationWindowRequest?.Invoke(this, new EventArgs());
        }

        private void DisplayEditAnnotationWindow()
        {
            this.EditAnnotationWindowRequest?.Invoke(this, new EventArgs());
        }

        [RelayCommand]
        private void ShowAddAnnotationWindow()
        {
            DisplayAddAnnotationWindow();
        }

        [RelayCommand]
        private void ShowEditAnnotationWindow(Annotation annotation)
        {
            _annotationDataHolder.Value = annotation;
            DisplayEditAnnotationWindow();
        }

        [RelayCommand]
        private async Task DeleteAnnotation(Guid id)
        {
            var annotation = await _annotationService.DeleteAsync(id);

            if (annotation != null && _annotationTypes.Contains(annotation))
            {
                _annotationTypes.Remove(annotation);
            }
            else
            {
                ShowMessageBox((string)Application.Current.TryFindResource("AnnotationWindow-Remove-Error"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        public void Receive(AddItemMessage message)
        {
            _annotationTypes.Add(message.Value);
        }

        public void Receive(EditItemMessage message)
        {
            if (_annotationTypes.Contains(message.Value))
            {
                var annotation = _annotationTypes.First(x => x.Id == message.Value.Id);
                annotation.Name = message.Value.Name;
                annotation.Color = message.Value.Color;

                var index = _annotationTypes.IndexOf(annotation);
                _annotationTypes[index] = annotation;
            }
        }
    }
}
