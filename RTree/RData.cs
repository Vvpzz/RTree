using System;
using System.Collections.Generic;
using System.Linq;

namespace RTree
{
	public class RData{
		public int NSample {get; private set;}
		public int NVars {get; private set;}
		public List<RDataPoint> Points { get; private set; }

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
					cachedMSE = ComputeMSE();
					mseSet = true;
				}
				return cachedMSE;
			} 
		}

		private bool avgSet, mseSet;
		private double cachedAvg, cachedMSE;

		private RData(List<RDataPoint> points, int nVars, int nSample)
		{
			Points = points;
			NVars = nVars;
			NSample = nSample;

			//			cachedAvg = double.NaN;
			//			cachedMSE = double.NaN;
		}

		public static RData FromRawData(double[][] x, double[] y)
		{
			var nSample = y.Length;
			if(x.Length != nSample)
				throw new ArgumentException("Inconsistent x and y sample size!");
			
			if(!(x.Select(a => a.Length).Distinct().Count()<2))
				throw new ArgumentException("Inconsistent x sample size!");
			int nVars = x[0].Length;

			var points = new List<RDataPoint>();
			for(int i = 0; i < nSample; i++) {

				var dataX = new double[nVars];
				for (int j = 0; j < nVars; j++) {
					dataX[j] = x[i][j];
				}
				var dp = new RDataPoint(dataX, y[i]);
				points.Add(dp);
			}

			return new RData(points, nVars, nSample);
		}

		public Tuple<RRegionSplit, RData>[] Partitions(RRegionSplit split)
		{
			var ptsIn = new List<RDataPoint>();
			var ptsOut = new List<RDataPoint>();
			for(int i = 0; i < NSample; i++) {
				var dp = Points[i];
				if(dp.InRegion(split))
					ptsIn.Add(dp);
				else
					ptsOut.Add(dp);
			}
			var dataIn = new RData(ptsIn, NVars, ptsIn.Count);
			var dataOut = new RData(ptsOut, NVars, NSample-ptsIn.Count);

			return new Tuple<RRegionSplit, RData>[]{ Tuple.Create(split, dataIn), Tuple.Create(split.Complement(), dataOut) };
		}

		private double ComputeAverage()
		{
			var sum = 0.0;
			for(int i = 0; i < NSample; i++) {
				sum += Points[i].Y;
			}
			return sum / NSample;
		}

		private double ComputeMSE()
		{
			var avg = ComputeAverage();
			var mse = 0.0;
			for(int i = 0; i < NSample; i++) {
				var tmp = (Points[i].Y - avg);
				mse += tmp * tmp;
			}
			return mse;
		}

		public double[] ComputeSplitPoints(int varId)
		{
			var splits = new double[NSample - 1];
			for(int i = 0; i < NSample-1; i++) 
			{
				splits[i] = 0.5 * (Points[i].Xs[varId] + Points[i + 1].Xs[varId]);
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
			this.Xs = xs;
			this.Y = y;
			this.NbVars = xs.Length;
		}


		public bool InRegion(RRegionSplit split)
		{
			return split.InDomain(Xs);
		}


	}
}

