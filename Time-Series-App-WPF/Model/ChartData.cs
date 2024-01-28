using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Model
{
    public class ChartData
    {
        public string? Label { get; set; }
        public string? XUnit { get; set; }
        public string? YUnit { get; set; }
        public float XAxisMin { get; set; }
        public float XAxisMax { get; set; }
        public float YAxisMin { get; set; }
        public float YAxisMax { get; set; }
    }
}
