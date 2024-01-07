using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.ViewModel;

namespace Time_Series_App_WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.MessageBoxRequest += new EventHandler<MessageBoxEventArgs>(ShowMessageBox);
        }

        private void ShowMessageBox(object? sender, MessageBoxEventArgs e)
        {
            MessageBox.Show(this, e.Message, e.Caption);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = (string)Application.Current.TryFindResource("OpenFileDialog-Filter");
            
            if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName))
            {
                viewModel!.OpenFileCommand.Execute(openFileDialog.FileName);
            }
        }
    }
}
