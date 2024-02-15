using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.View
{
    public class AnalyzeButtonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 4 && values[0] is Annotation && values[1] is bool isCheckedOne && values[2] is bool isCheckedTwo && values[3] is string text)
            {
                return (isCheckedOne && !string.IsNullOrEmpty(text)) || isCheckedTwo;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
