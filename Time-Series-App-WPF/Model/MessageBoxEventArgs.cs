using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Time_Series_App_WPF.Model
{
    public class MessageBoxEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly string _caption;

        public MessageBoxEventArgs(string message, string caption)
        {
            this._message = message;
            this._caption = caption;
        }

        public string Message { get { return _message; } }
        public string Caption { get { return _caption; } }
    }
}
