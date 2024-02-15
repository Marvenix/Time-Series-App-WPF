using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Analysis
{
    public class CrossCorrelation
    {
        public static double[] Calculate(double[] seriesA, double[] seriesB, string mode)
        {
            var npSeriesA = Numpy.np.array(seriesA);
            var npSeriesB = Numpy.np.array(seriesB);

            var result = Numpy.np.correlate(npSeriesA, npSeriesB, mode).GetData<double>();

            return result;
        }
    }
}
