using System;
using System.Collections.Generic;
using System.Linq;

namespace RTree
{
	public class RData{
		public int NSample {get; private set;}
		public int NVars {get; private set;}
		public List<RDataPoint> Points { get; private set; }
		public int OrderedBy { get; private set; }

		public double Average {
			get 
			{
				if(!avgSet) 
				{
					cachedAvg = ComputeAverage();
					avgSet = true;
				}
				return cachedAvg;
			} 
		}
		public double MSE {
			get 
			{
				if(!mseSet) 
				{
					cachedMse = ComputeMSE();
					mseSet = true;
				}
				return cachedMse;
			} 
		}

		private bool CacheSet{ get { return avgSet && mseSet; } }

		private bool avgSet, mseSet;
		private double cachedAvg, cachedMse;

		private RData(List<RDataPoint> points, int nVars, int nSample, int orderedBy=-1, bool forcePrecompute=false)
		{
			Points = points;
			NVars = nVars;
			NSample = nSample;
			OrderedBy = orderedBy;
			if(forcePrecompute)
			{
				//force computation of Average and MSE
				var tmpAvg = Average;
				var tmpMse = MSE;
			}				
		}

		public RData(RData other):this(new List<RDataPoint>(other.Points), other.NVars, other.NSample, other.OrderedBy)
		{
			if(other.avgSet) 
			{
				cachedAvg = other.cachedAvg;
				avgSet = true;
			}
			if(other.mseSet) 
			{
				cachedMse = other.cachedMse;
				mseSet = true;
			}
		}

		public static RData Empty(int nVars, int orderedBy=-1)
		{
			return new RData(new List<RDataPoint>(), nVars, 0, orderedBy);
		}

		public static RData FromRawData(double[][] x, double[] y, int orderedBy=-1)
		{
			var nSample = y.Length;
			if(x.Length != nSample)
				throw new ArgumentException("Inconsistent x and y sample size!");
			
			if(x.Select(a => a.Length).Distinct().Count() >= 2)
				throw new ArgumentException("Inconsistent x sample size!");
			int nVars = x[0].Length;

			var points = new List<RDataPoint>(nSample);
			for(int i = 0; i < nSample; i++) 
			{
				var dataX = new double[nVars];
				for (int j = 0; j < nVars; j++) 
				{
					dataX[j] = x[i][j];
				}
				var dp = new RDataPoint(dataX, y[i]);
				points.Add(dp);
			}

			return new RData(points, nVars, nSample, orderedBy, true);
		}

		public void Add(RDataPoint dp, int orderedBy = -1)
		{
			Points.Add(dp);
			NSample += 1;
			if(orderedBy != OrderedBy)
				OrderedBy = -1;

			UpdateAverageAndMSE(dp, true);
		}

		public void RemoveAt(int i)
		{
			var dp = Points[i];

			Points.RemoveAt(i);
			NSample -= 1;

			UpdateAverageAndMSE(dp, false);
		}

		public void Sort(int xIndex = 0)
		{
			if(OrderedBy == xIndex)
				return;

			Points.Sort(new RDataPointComparer(xIndex));
			OrderedBy = xIndex;
		}

		public RData BootStrap(int[] sampleIndices)
		{
			int n = sampleIndices.Length;
			var dp = new List<RDataPoint>(n);
			for(int i = 0; i < n; i++) 
			{
				dp.Add(Points[sampleIndices[i]]);
			}
			return new RData(dp, NVars, n);
		}	

//		public RData[] Partitions(RRegionSplit split)
//		{
//			var ptsIn = new List<RDataPoint>(NSample);
//			var ptsOut = new List<RDataPoint>(NSample);
//			for(int i = 0; i < NSample; i++) 
//			{
//				var dp = Points[i];
//				if(split.InDomain(dp.Xs))
//					ptsIn.Add(dp);
//				else
//					ptsOut.Add(dp);
//			}
//			var nPtsIn = ptsIn.Count;
//			var dataIn = new RData(ptsIn, NVars, nPtsIn);
//			var dataOut = new RData(ptsOut, NVars, NSample-nPtsIn);
//
//			return new []{ dataIn, dataOut };
//		}

		public void IterativePartitions(RRegionSplit lowerSplit, ref RData left, ref RData right)
		{
			int start = left.NSample;
			for(int i = start; i < NSample; i++) 
			{
				var dp = Points[i];
				if(lowerSplit.InDomain(dp.Xs)) 
				{
					left.Add(dp, lowerSplit.VarId);
					right.RemoveAt(0);
				} 
				else
					break;
			}
		}

		private double ComputeAverage()
		{
			var sum = 0.0;

			if(NSample <= 0)
				return sum;

			for(int i = 0; i < NSample; i++) 
			{
				sum += Points[i].Y;
			}
			return sum / NSample;
		}

		private double ComputeMSE()
		{
			var avg = Average;
			var mse = 0.0;
			for(int i = 0; i < NSample; i++) 
			{
				var tmp = (Points[i].Y - avg);
				mse += tmp * tmp;
			}
			return mse;
		}

		private void UpdateAverageAndMSE(RDataPoint dp, bool added)
		{


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

//		private void UpdateAverage(RDataPoint dp, bool added)
//		{
//			if(!avgSet)
//			{
//				cachedAvg = Average;
//				return;
//			}
//			
//			if(added)
//			{
//				cachedAvg = (cachedAvg * (NSample - 1) + dp.Y) / NSample;
//			}
//			else
//			{
//				cachedAvg = (cachedAvg * (NSample + 1) - dp.Y) / NSample;
//			}
//		}
//
//		private void UpdateMse(RDataPoint dp, bool added)
//		{
//			//TODO : see if one can do better
//			cachedMse = MSE;
//
//
////			if(!mseSet)
////			{
////				MSE;
////				return;
////			}
////
////			if(added)
////			{
////				cachedMse = (cachedMse + dp.Y) / NSample;
////			}
////			else
////			{
////
////			}
//		}

		public double[] ComputeSplitPoints(int varId)
		{
			var splits = new double[NSample - 1];
			for(int i = 0; i < NSample-1; i++) 
			{
				splits[i] = Points[i].Xs[varId];//0.5 * (Points[i].Xs[varId] + Points[i + 1].Xs[varId]);
			}
			return splits;
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

