using OpenTK.Graphics.OpenGL;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*******************************************************************************
 * Copyright (C) 2018 Francois PETITJEAN 
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, version 3 of the License.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 ******************************************************************************/

namespace Time_Series_App_WPF.Analysis
{
    public class DBA
    {
        private static readonly int NIL = -1;
        private static readonly int DIAGONAL = 0;
        private static readonly int LEFT = 1;
        private static readonly int UP = 2;

        private static readonly int[] moveI = { -1, 0, -1 };
        private static readonly int[] moveJ = { -1, -1, 0 };

        public static double[] PerformDBA(double[][] sequences, int nIterations)
        {

            int maxLength = 0;
            for (int i = 0; i < sequences.Length; i++)
            {
                maxLength = Math.Max(maxLength, sequences[i].Length);
            }
            double[][] costMatrix = new double[maxLength][];

            for(int i = 0; i < costMatrix.Length; i++)
                costMatrix[i] = new double[maxLength];

            int[][] pathMatrix = new int[maxLength][];

            for (int i = 0; i < pathMatrix.Length; i++)
                pathMatrix[i] = new int[maxLength];

            int medoidIndex = ApproximateMedoidIndex(sequences, costMatrix);
            double[] center = new double[sequences[medoidIndex].Length];
            Array.Copy(sequences[medoidIndex], center, sequences[medoidIndex].Length);

            for (int i = 0; i < nIterations; i++)
            {
                center = DBAUpdate(center, sequences, costMatrix, pathMatrix);
            }
            return center;
        }

        public static double[] PerformDBA(double[][] sequences)
        {
            return PerformDBA(sequences, 15);
        }

        private static int ApproximateMedoidIndex(double[][] sequences, double[][] mat)
        {
            var allIndices = new List<int>();
            for (int i = 0; i < sequences.Length; i++)
            {
                allIndices.Add(i);
            }
            allIndices = allIndices.OrderBy(x => Random.Shared.Next()).ToList();
            var medianIndices = new List<int>();
            for (int i = 0; i < sequences.Length && i < 50; i++)
            {
                medianIndices.Add(allIndices.ElementAt(i));
            }

            int indexMedoid = -1;
            double lowestSoS = Double.MaxValue;

            foreach (int medianCandidateIndex in medianIndices)
            {
                double[] possibleMedoid = sequences[medianCandidateIndex];
                double tmpSoS = SumOfSquares(possibleMedoid, sequences, mat);
                if (tmpSoS < lowestSoS)
                {
                    indexMedoid = medianCandidateIndex;
                    lowestSoS = tmpSoS;
                }
            }
            return indexMedoid;
        }

        private static double SumOfSquares(double[] sequence, double[][] sequences, double[][] mat)
        {
            double sos = 0.0;
            for (int i = 0; i < sequences.Length; i++)
            {
                double dist = DTW(sequence, sequences[i], mat);
                sos += dist * dist;
            }
            return sos;
        }
        
        public static double DTW(double[] S, double[] T, double[][] costMatrix)
        {
            int i, j;
            costMatrix[0][0] = SquaredDistance(S[0], T[0]);
            for (i = 1; i < S.Length; i++)
            {
                costMatrix[i][0] = costMatrix[i - 1][0] + SquaredDistance(S[i], T[0]);
            }
            for (j = 1; j < T.Length; j++)
            {
                costMatrix[0][j] = costMatrix[0][j - 1] + SquaredDistance(S[0], T[j]);
            }
            for (i = 1; i < S.Length; i++)
            {
                for (j = 1; j < T.Length; j++)
                {
                    costMatrix[i][j] = Min3(costMatrix[i - 1][j - 1], costMatrix[i][j - 1], costMatrix[i - 1][j])
                                    + SquaredDistance(S[i], T[j]);
                }
            }

            return Math.Sqrt(costMatrix[S.Length - 1][T.Length - 1]);
        }

        private static double[] DBAUpdate(double[] C, double[][] sequences, double[][] costMatrix, int[][] pathMatrix)
        {
            double[] updatedMean = new double[C.Length];
            int[] nElementsForMean = new int[C.Length];

            int i, j, move;
            double res = 0.0;
            int centerLength = C.Length;
            int seqLength;

            foreach (double[] T in sequences)
            {
                seqLength = T.Length;

                costMatrix[0][0] = SquaredDistance(C[0], T[0]);
                pathMatrix[0][0] = DBA.NIL;

                for (i = 1; i < centerLength; i++)
                {
                    costMatrix[i][0] = costMatrix[i - 1][0] + SquaredDistance(C[i], T[0]);
                    pathMatrix[i][0] = DBA.UP;
                }
                for (j = 1; j < seqLength; j++)
                {
                    costMatrix[0][j] = costMatrix[0][j - 1] + SquaredDistance(T[j], C[0]);
                    pathMatrix[0][j] = DBA.LEFT;
                }

                for (i = 1; i < centerLength; i++)
                {
                    for (j = 1; j < seqLength; j++)
                    {
                        double diag = costMatrix[i - 1][j - 1], left = costMatrix[i][j - 1], top = costMatrix[i - 1][j];
                        if (diag <= left)
                        {
                            if (diag <= top)
                            {
                                res = diag;
                                move = DIAGONAL;
                            }
                            else
                            {
                                res = top;
                                move = UP;
                            }
                        }
                        else
                        {
                            if (left <= top)
                            {
                                res = left;
                                move = LEFT;
                            }
                            else
                            {
                                res = top;
                                move = UP;
                            }
                        }

                        pathMatrix[i][j] = move;
                        res = costMatrix[i + moveI[move]][j + moveJ[move]];
                        costMatrix[i][j] = res + SquaredDistance(C[i], T[j]);
                    }
                }

                i = centerLength - 1;
                j = seqLength - 1;

                while (pathMatrix[i][j] != DBA.NIL)
                {
                    updatedMean[i] += T[j];
                    nElementsForMean[i]++;
                    move = pathMatrix[i][j];
                    i += moveI[move];
                    j += moveJ[move];
                }

                if (i != 0 || j != 0)
                    throw new Exception();

                updatedMean[i] += T[j];
                nElementsForMean[i]++;
            }

            for (int t = 0; t < centerLength; t++)
            {
                updatedMean[t] /= nElementsForMean[t];
            }

            return updatedMean;

        }

        private static double Min3(double a, double b, double c)
        {
            if (a < b)
            {
                if (a < c)
                {
                    return a;
                }
                else
                {
                    return c;
                }
            }
            else
            {
                if (b < c)
                {
                    return b;
                }
                else
                {
                    return c;
                }
            }
        }

        private static double SquaredDistance(double a, double b)
        {
            double diff = a - b;
            return diff * diff;
        }
    }
}
