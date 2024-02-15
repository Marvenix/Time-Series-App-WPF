using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Analysis
{
    public interface IAnalysisService
    {
        public (double[], double[]) DTWAnalysis(List<MadeAnnotation> madeAnnotations, WpfPlot plotControl, int stepSize);
        public (double[], double[]) CrossCorrelationAnalysis(List<MadeAnnotation> madeAnnotations, WpfPlot plotControl);
    }
}
