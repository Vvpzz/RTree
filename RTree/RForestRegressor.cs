using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RTree
{
	public class RForestRegressionSettings
	{
		public int NbTrees {get; private set;}
		public double SampleProportion {get; private set;}
		public int MinNodeSize {get; private set;}
		public int MaxTreeDepth {get; private set;}
		public int NbSplitVariables {get; private set;}

		//TODO : see settings for limiting tree size
		public RForestRegressionSettings(int nbTrees, double sampleProportion, int minNodeSize, int maxTreeDepth, int nbSplitVariables)
		{
			NbTrees = nbTrees;
			SampleProportion = sampleProportion;
			MinNodeSize = minNodeSize;
			MaxTreeDepth = maxTreeDepth;
			NbSplitVariables = nbSplitVariables;
		}
		
	}

	public class RForestRegressionReport
	{
		public List<RTreeRegressionReport> TreeRegReports {get; private set;}
	}

	public class RForestRegressor
	{
		readonly RForestRegressionSettings settings;
		readonly List<RTreeRegressor> treeRegs;
		RForest forest;

		public RForestRegressor(RForestRegressionSettings settings)
		{
			this.settings = settings;
			treeRegs = new List<RTreeRegressor>(settings.NbTrees);
		}

		public RForestRegressionReport Train(double[][] x, double[] y)
		{
			var data = RData.FromRawData(x, y);
			return Train(data);
		}

		public RForestRegressionReport Train(RData data)
		{
			var dataSize = data.NSample;
			var nTrees = settings.NbTrees;
			var trees = new List<RTree>(nTrees);
			var rand = new Random(1234);
			var bs = new BootStrap(rand, dataSize, (int)(dataSize * settings.SampleProportion));
			var treeSettings = new RTreeRegressionSettings(settings.MinNodeSize, settings.MaxTreeDepth, PruningType.None, double.NaN, settings.NbSplitVariables);

			for(int i = 0; i < nTrees; i++) 
			{
				var sw = Stopwatch.StartNew();
				var bsIndices = bs.DoSample();
				var bsData = data.BootStrap(bsIndices);
				//TODO : manage nD case
				bsData.Sort(0);
				var t = GrowTree(bsData, treeSettings);
				trees.Add(t);
				sw.Stop();
//				Console.WriteLine(t.Print());
				Console.WriteLine (string.Format("Build tree {0}/{1} [n={3}][d={4}][{2}]", i+1, nTrees, sw.Elapsed, t.NbNodes, t.Depth));
			}

			forest = new RForest(trees);

			//TODO
			return new RForestRegressionReport();
		}

		private RTree GrowTree(RData data, RTreeRegressionSettings treeSettings)
		{
			var treeReg = new RTreeRegressor(treeSettings);
			double mse;
			var report = treeReg.Train(data);
			return treeReg.Tree;
		}

		public double Evaluate(double[] x)
		{
			return forest.Evaluate(x);
		}
	}
}

