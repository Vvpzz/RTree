using System;
using System.Collections.Generic;
using System.Linq;

namespace RTree
{
	public class RData
	{
		public int NSample {get; private set;}
		public int NVars {get; private set;}
		public RDataPoint[] Points { get; private set; }
		private RDataPointComparer[] Comparers { get; set; }

		public RData(RDataPoint[] points)
		{
			Points = points;
			NVars = points.Select(p=>p.Xs.Length).Distinct().Single();
			NSample = points.Length;

			Comparers = new RDataPointComparer[NVars];
			for(int i = 0; i < NVars; i++) 
			{
				Comparers[i] = new RDataPointComparer(i, NVars);
			}
		}

		public static RData FromRawData(double[][] x, double[] y)
		{
			var nSample = y.Length;
			if(x.Length != nSample)
				throw new ArgumentException("Inconsistent x and y sample size!");

			if(x.Select(a => a.Length).Distinct().Count() >= 2)
				throw new ArgumentException("Inconsistent x sample size!");
			int nVars = x[0].Length;

			var points = new RDataPoint[nSample];
			for(int i = 0; i < nSample; i++) 
			{
				var dataX = new double[nVars];
				for (int j = 0; j < nVars; j++) 
				{
					dataX[j] = x[i][j];
				}
				var dp = new RDataPoint(dataX, y[i]);
				points[i] = dp;
			}

			return new RData(points);
		}

		public void SortBetween(int varIdx, int start, int length)
		{
			Array.Sort(Points, start, length, Comparers[varIdx]/*new RDataPointComparer(varIdx)*/);
		}

		public RData BootStrap(int[] sampleIndices)
		{
			int n = sampleIndices.Length;
			var dp = new RDataPoint[n];
			for(int i = 0; i < n; i++) 
			{
				dp[i] = Points[sampleIndices[i]];
			}
			return new RData(dp);
		}	



		public double Average(int start, int length)
		{
			var sum = 0.0;

			if(length <= 0)
				return sum;

			for(int i = 0; i < length; i++) 
			{
				sum += Points[start + i].Y;
			}
			return sum / length;
		}

		public double MSE(int start, int length)
		{
			var avg = Average(start, length);
			var mse = 0.0;
			for(int i = 0; i < length; i++) 
			{
				var tmp = (Points[start + i].Y - avg);
				mse += tmp * tmp;
			}
			return mse;
		}


		public int BestSplitBetween(int varId, int start, int length, out double bestMse, out double bestAvgL, out double bestAvgR, out double bestMseL, out double bestMseR, double parentAvg, double parentMse)
		{
//			if(length < 2)
//				throw new ArgumentException("Cannot split an array smaller than 2 elements");

			//init
			int upper = start + length;
			int split = start;

			int leftLen = 1;
			int rightLen = length - 1;

			double leftAvg = Points[start].Y;
			double leftMSE = 0.0;

			//Average & MSE previously computed for parent node. Here we online update them instead of recomputing them from scratch.
			double rightAvg = parentAvg;
			double rightMSE = parentMse;

			PostRemoveUpdate(Points[start], ref rightAvg, ref rightMSE, rightLen);

			bestMse = leftMSE + rightMSE;
			bestAvgL = leftAvg;
			bestAvgR = rightAvg;
			bestMseL = leftMSE;
			bestMseR = rightMSE;
			int bestSplit = start;

			while(rightLen>1)
			{
				var prevSplit = split;
				split = NextSplit(varId, split, upper);

				for(int i = prevSplit+1; i <= split; i++) 
				{
					leftLen += 1;
					rightLen -= 1;

					if(rightLen < 1)
						return bestSplit;

					PostAddUpdate(Points[i], ref leftAvg, ref leftMSE, leftLen);
					PostRemoveUpdate(Points[i], ref rightAvg, ref rightMSE, rightLen);
				}

				var mse = leftMSE + rightMSE;
				if(mse<bestMse)
				{
					bestMse = mse;
					bestSplit = split;
					bestAvgL = leftAvg;
					bestAvgR = rightAvg;
					bestMseL = leftMSE;
					bestMseR = rightMSE;
				}
			}
			return bestSplit;
		}

		/// <summary>
		/// Returns split in ]start, limit[
		/// </summary>
		/// <returns>The split.</returns>
		/// <param name="varId">Variable identifier.</param>
		/// <param name="start">Start.</param>
		/// <param name="limit">Limit.</param>
		private int NextSplit(int varId, int start, int limit)
		{
			int s = start + 1;
			while(s<limit-1)
			{
				if(Points[s].Xs[varId] < Points[s + 1].Xs[varId])
					return s;
				
				s+=1;
			}
			return s;

		}

		public void PostAddUpdate(RDataPoint dp, ref double avg, ref double mse, int nSample)
		{
			double delta = dp.Y - avg;
			avg += delta / nSample;
			double delta2 = dp.Y - avg;//avg has changed compared to delta :)
			mse += delta * delta2;
		}

		public void PostRemoveUpdate(RDataPoint dp, ref double avg, ref double mse, int nSample)
		{
			if(nSample == 0) 
			{
				avg = 0;
				mse = 0;
				return;
			}
			double delta = avg - dp.Y;
			avg += delta / nSample;
			double delta2 = dp.Y - avg;
			mse += delta * delta2;
		}
	}

	public class RDataPoint
	{
		public double[] Xs {get; private set;}
		public double Y {get; private set;}
		public int NbVars { get;}

		public RDataPoint(double[] xs, double y)
		{
			Xs = xs;
			Y = y;
			NbVars = xs.Length;
		}

		public override string ToString()
		{
			return string.Format("[RDataPoint: Xs={{{0}}}; Y={1}; NbVars={2}]", string.Join(";", Xs), Y, NbVars);
		}
	}

	public class RDataPointComparer : IComparer<RDataPoint>
	{
		readonly int xIndex;
		readonly int[] otherIndices;

		public RDataPointComparer(int xIndex, int nVars)
		{
			this.xIndex = xIndex;

			otherIndices = Enumerable.Range(0, nVars).Where(i => i != xIndex).ToArray();
		}
		

		#region IComparer implementation
		public int Compare(RDataPoint dp1, RDataPoint dp2)
		{
			var c = Compare(dp1, dp2, xIndex);
			if(c != 0)
				return c;

			for(int i = 0; i < otherIndices.Length; i++) 
			{
				var cc = Compare(dp1, dp2, otherIndices[i]);
				if(cc != 0)
					return cc;
			}

			return 0;
		}
		#endregion

		private static int Compare(RDataPoint dp1, RDataPoint dp2, int idx)
		{
			var x1 = dp1.Xs[idx];
			var x2 = dp2.Xs[idx];

			if(x1 < x2)
				return -1;

			if(x1 > x2)
				return 1;

			return 0;
		}
	}
}

