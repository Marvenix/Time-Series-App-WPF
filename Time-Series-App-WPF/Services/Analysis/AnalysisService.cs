using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Analysis;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        public (double[], double[]) DTWAnalysis(List<MadeAnnotation> madeAnnotations, WpfPlot plotControl, int stepSize)
        {
            var mergedMadeAnnotation = MergeMadeAnnotations(madeAnnotations, plotControl) ?? throw new Exception();

            var signal = (Signal?)plotControl.Plot.PlottableList.FirstOrDefault(x => x is Signal);
            var yData = signal!.Data.GetYs();
            var iterations = ((yData.Count - mergedMadeAnnotation.Length) / stepSize) + 1;
            var yDataPart = new double[mergedMadeAnnotation.Length];
            var results = new double[iterations];

            Parallel.For(0, iterations, i =>
            {
                var yPart = new double[mergedMadeAnnotation.Length];

                for (int j = 0; j < yPart.Length; j++)
                {
                    yPart[j] = yData[j + (i * stepSize)];
                }

                results[i] = FastDtw.CSharp.Dtw.GetScore(mergedMadeAnnotation, yPart);
            });

            return (mergedMadeAnnotation, results);
        }

        public (double[], double[]) CrossCorrelationAnalysis(List<MadeAnnotation> madeAnnotations, WpfPlot plotControl)
        {
            var mergedMadeAnnotation = MergeMadeAnnotations(madeAnnotations, plotControl) ?? throw new Exception();

            var signal = (Signal?)plotControl.Plot.PlottableList.FirstOrDefault(x => x is Signal);
            var yData = signal!.Data.GetYs().ToArray();

            var results = CrossCorrelation.Calculate(mergedMadeAnnotation, yData, "full");

            return (mergedMadeAnnotation, results);
        }

        private static double[] MergeMadeAnnotations(List<MadeAnnotation> madeAnnotations, WpfPlot plotControl)
        {
            if (!madeAnnotations.Any() || madeAnnotations.Where(x => x.ChannelId != plotControl.Uid).Any())
                throw new Exception();

            var madeAnnotationsData = new double[madeAnnotations.Count][];

            var signal = (Signal?)plotControl.Plot.PlottableList.FirstOrDefault(x => x is Signal) ?? throw new Exception();

            var yData = signal.Data.GetYs();

            for(int i = 0; i < madeAnnotations.Count; i++)
            {
                var start = signal.Data.GetIndex(madeAnnotations[i].Start, false);
                var end = signal.Data.GetIndex(madeAnnotations[i].End, false);
                var length = end - start;

                madeAnnotationsData[i] = new double[length];

                for(int j = 0; j < length; j++)
                {
                    madeAnnotationsData[i][j] = yData[start + j];
                }
            }


            return DBA.PerformDBA(madeAnnotationsData);
        }
    }
}
