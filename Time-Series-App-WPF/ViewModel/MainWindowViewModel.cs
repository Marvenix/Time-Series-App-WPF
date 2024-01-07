using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.Services.Files;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly IFileService _fileService;

        public MainWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
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
                _fileService.ReadFile(fileName);
                //ShowAndSetupCharts();
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
    }
}
