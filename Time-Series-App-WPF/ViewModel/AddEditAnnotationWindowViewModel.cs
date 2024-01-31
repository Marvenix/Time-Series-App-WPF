using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Time_Series_App_WPF.Messages;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.Services.Annotations;
using Xceed.Wpf.Toolkit;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class AddEditAnnotationWindowViewModel : BaseViewModel
    {
        private readonly IAnnotationService _annotationService;
        private readonly IMessenger _messenger;
        private readonly DataHolder<Annotation> _annotationDataHolder;
        private readonly string _pattern = @"^[a-zA-Z0-9]{1,50}$";

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Color? _color;

        private Guid _id;

        public AddEditAnnotationWindowViewModel(IAnnotationService annotationService, IMessenger messenger, DataHolder<Annotation> annotationDataHolder)
        {
            _annotationService = annotationService;
            _messenger = messenger;
            _annotationDataHolder = annotationDataHolder;
            SetVariables();
        }

        [RelayCommand]
        private async Task AddAnnotation()
        {
            if (Name !=  null && Color != null && Regex.IsMatch(Name, _pattern))
            {
                var annotation = new Annotation
                {
                    Name = Name,
                    Color = Color.Value.ToString()
                };

                await _annotationService.CreateAsync(annotation);
                _messenger.Send(new AddAnnotationItemMessage(annotation));
            }
            else
            {
                ShowMessageBox((string)Application.Current.TryFindResource("AddEditAnnotationWindow-Add-Error"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        [RelayCommand]
        private async Task EditAnnotation()
        {
            if (Name != null && Color != null && Regex.IsMatch(Name, _pattern))
            {
                var annotation = new Annotation
                {
                    Name = Name,
                    Color = Color.Value.ToString()
                };

                var updatedAnnotation = await _annotationService.UpdateAsync(_id, annotation);

                if (updatedAnnotation != null)
                {
                    _messenger.Send(new EditAnnotationItemMessage(updatedAnnotation));
                }
                else
                {
                    ShowMessageBox((string)Application.Current.TryFindResource("AddEditAnnotationWindow-Edit-Error"), (string)Application.Current.TryFindResource("Error"));
                }
            }
            else
            {
                ShowMessageBox((string)Application.Current.TryFindResource("AddEditAnnotationWindow-Edit-Error"), (string)Application.Current.TryFindResource("Error"));
            }
        }

        private void SetVariables()
        {
            if (_annotationDataHolder.Value != null)
            {
                Name = _annotationDataHolder.Value.Name;
                Color = (Color)ColorConverter.ConvertFromString(_annotationDataHolder.Value.Color);
                _id = _annotationDataHolder.Value.Id;

                _annotationDataHolder.Value = null;
            }
        }
    }
}
