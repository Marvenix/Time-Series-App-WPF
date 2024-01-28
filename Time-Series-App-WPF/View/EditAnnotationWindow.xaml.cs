using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class EditAnnotationWindow : Window
    {
        public EditAnnotationWindow(AddEditAnnotationWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.MessageBoxRequest += new EventHandler<MessageBoxEventArgs>(ShowMessageBox);
        }

        private void ShowMessageBox(object? sender, MessageBoxEventArgs e)
        {
            MessageBox.Show(this, e.Message, e.Caption);
        }
    }
}
