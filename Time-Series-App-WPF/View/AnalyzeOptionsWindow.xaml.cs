using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.ViewModel;

namespace Time_Series_App_WPF.View
{
    public partial class AnalyzeOptionsWindow : Window
    {
        public AnalyzeOptionsWindow(AnalyzeOptionsWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.MessageBoxRequest += new EventHandler<MessageBoxEventArgs>(ShowMessageBox);
            viewModel.AnalyzeResultsWindowRequest += new EventHandler(ShowAnalyzeResultsWindow);
        }

        private void ShowMessageBox(object? sender, MessageBoxEventArgs e)
        {
            MessageBox.Show(this, e.Message, e.Caption);
        }

        private void ShowAnalyzeResultsWindow(object? sender, EventArgs e)
        {
            var analyzeResultsWindow = App.AppHost!.Services.GetRequiredService<AnalyzeResultsWindow>();
            analyzeResultsWindow.ShowDialog();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Regex nonZeroRegex = FirstNonZeroRegex();

                if (textBox.SelectionLength > 0)
                {
                    var newText = textBox.Text;

                    for (int i = textBox.SelectionStart; i < textBox.SelectionStart + textBox.SelectionLength; i++)
                    {
                        newText = newText.Remove(textBox.SelectionStart, 1);
                    }
                    newText = newText.Insert(textBox.SelectionStart, e.Text);
                    e.Handled = !nonZeroRegex.IsMatch(newText);
                }
                else
                {
                    var newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                    e.Handled = !nonZeroRegex.IsMatch(newText);
                }
            }
        }

        [GeneratedRegex("^[1-9][0-9]*$")]
        private static partial Regex FirstNonZeroRegex();

        private void ArgumentTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox textBox && (bool)e.NewValue == false)
            {
                textBox.Text = String.Empty;
            }
        }
    }
}
