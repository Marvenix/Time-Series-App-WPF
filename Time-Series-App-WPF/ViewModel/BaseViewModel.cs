using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.ViewModel
{
    public partial class BaseViewModel : ObservableObject
    {
        public event EventHandler<MessageBoxEventArgs>? MessageBoxRequest;

        protected void ShowMessageBox(string message, string caption)
        {
            this.MessageBoxRequest?.Invoke(this, new MessageBoxEventArgs(message, caption));
        }
    }
}
