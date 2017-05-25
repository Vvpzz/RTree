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

		public RData(RDataPoint[] points)
		{
			Points = points;
			NVars = points.Select(p=>p.Xs.Length).Distinct().Single();
			NSample = points.Length;
		}

		public static RData FromRawData(double[][] x, double[] y, int orderedBy=-1)
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

		//data in input is sorted 
		public void IterativePartitionsInPlace(RRegionSplit lowerSplit, int start, int length)
		{



			throw new NotImplementedException();
//			int start = left.NSample;
//			for(int i = start; i < NSample; i++) 
//			{
//				var dp = Points[i];
//				if(lowerSplit.InDomain(dp.Xs)) 
//				{
//					left.Add(dp, lowerSplit.VarId);
//					right.RemoveAt(0);
//				} 
//				else
//					break;
//			}
		}

		public void SortBetween(int varIdx, int start, int length)
		{
			Array.Sort(Points, start, length, new RDataPointComparer(varIdx));
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


//		!!!!!!LE PB VIENT DU BOOTSTRAP!!!!!
		public int BestSplitBetween(int varId, int start, int length, out double bestMse, out double bestAvgL, out double bestAvgR, out double bestMseL, out double bestMseR)
		{
			if(length < 2)
				throw new ArgumentException("Cannot split an array smaller than 2 elements");

			//init
//			bestAvgL = double.NaN;
//			bestAvgR = double.NaN;
//			bestMseL = double.NaN;
//			bestMseR = double.NaN;
			int upper = start + length;
			int split = start;//NextSplit(start, upper);

//			double minMse = double.PositiveInfinity;
//			int bestSplit = -1;
			//double bestAvgL, bestAvgR, bestMseL, bestMseR;


			double leftAvg = Points[start].Y;
			double leftMSE = 0.0;

			//TODO : Average(start, upper) & MSE(start, upper) previously computed. Get it and use online update it instead of recomputing it from scratch
			double rightAvg = Average(start+1, length-1);
			double rightMSE = MSE(start+1, length-1);

			bestMse = leftMSE + rightMSE;
			bestAvgL = leftAvg;
			bestAvgR = rightAvg;
			bestMseL = leftMSE;
			bestMseR = rightMSE;
			int bestSplit = start;

			int splitLimit = start + length - 1;
			int leftLen = 1;
			int rightLen = length - 1;


			//while(split < splitLimit) 
			while(rightLen>1)
			{
				var prevSplit = split;
				split = NextSplit(varId, split, upper);

				//changed
//				for(int i = prevSplit; i < split; i++) 
				for(int i = prevSplit+1; i <= split; i++) 
				{
					//TODO					
					leftLen += 1;
					rightLen -= 1;

					if(rightLen < 1)
						return bestSplit;

//					if(leftLen > length - 1 || rightLen < 1)
//						throw new ArgumentException(string.Format("Wrong split: l={0}, r={1}, split = {2}, prevSplit = {3}", leftLen, rightLen, split, prevSplit));

					//changed
//					PostAddUpdate(Points[split], ref leftAvg, ref leftMSE, leftLen);
//					PostRemoveUpdate(Points[split], ref rightAvg, ref rightMSE, rightLen);
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

//				}
					

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
			//TODO
			return s;

		}

		private void UpdateAverageAndMSE(RDataPoint dp, bool added)
		{
			double cachedAvg = 0;
			double cachedMse = 0;

			if(added) 
			{
				double delta = dp.Y - cachedAvg;
				cachedAvg += delta / NSample;
				double delta2 = dp.Y - cachedAvg;//cachedAvg has changed compared to delta :)
				cachedMse += delta * delta2;
			}
			else
			{
				if(NSample == 0)
				{
					cachedAvg = 0;
					cachedMse = 0;
					return;
				}

				double delta = cachedAvg - dp.Y;
				cachedAvg += delta / NSample;
				double delta2 = dp.Y - cachedAvg;
				cachedMse += delta * delta2;
			}
		}

		private void PostAddUpdate(RDataPoint dp, ref double avg, ref double mse, int nSample)
		{
			double delta = dp.Y - avg;
			avg += delta / nSample;
			double delta2 = dp.Y - avg;//cachedAvg has changed compared to delta :)
			mse += delta * delta2;
		}

		private void PostRemoveUpdate(RDataPoint dp, ref double avg, ref double mse, int nSample)
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

//		public double[] ComputeSplitPoints(int varId, int start, int length)
//		{
//			var splits = new double[length - 1];
//			for(int i = 0; i < length-1; i++) 
//			{
////				splits[i] = Points[start + i].Xs[varId];//0.5 * (Points[i].Xs[varId] + Points[i + 1].Xs[varId]);
//				splits[i] = 0.5 * (Points[i].Xs[varId] + Points[i + 1].Xs[varId]);//Points[start + i].Xs[varId];//
//			}
//			return splits;
//		}
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
			return string.Format("[RDataPoint: Xs={0}; Y={1}; NbVars={2}]", string.Join(";", Xs), Y, NbVars);
		}
	}

	public class RDataPointComparer : IComparer<RDataPoint>
	{
		readonly int xIndex;

		public RDataPointComparer(int xIndex)
		{
			this.xIndex = xIndex;
		}
		

		#region IComparer implementation
		public int Compare(RDataPoint dp1, RDataPoint dp2)
		{
			var x1 = dp1.Xs[xIndex];
			var x2 = dp2.Xs[xIndex];

			if(x1 < x2)
				return -1;
			
			if(x1 > x2)
				return 1;
			
			return 0;
		}
		#endregion
		
	}
}

