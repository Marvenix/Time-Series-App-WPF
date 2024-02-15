using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Model
{
    public class SignalChartDataDouble : SignalChartData
    {
        public required double[] Values { get; set; }
    }
}
