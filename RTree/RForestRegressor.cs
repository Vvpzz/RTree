using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RTree
{
	public class RForestRegressionSettings
	{
		public int NbTrees {get; private set;}
		public double SampleProportion {get; private set;}
		public int MinNodeSize {get; private set;}
		public int NbSplitVariables {get; private set;}

		//TODO : see settings for limiting tree size
		public RForestRegressionSettings(int nbTrees, double sampleProportion, int minNodeSize, int nbSplitVariables)
		{
			NbTrees = nbTrees;
			SampleProportion = sampleProportion;
			MinNodeSize = minNodeSize;
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

		public RForestRegressionReport Train(double[][] x, double[] y){

			var data = RData.FromRawData(x, y);
			return Train(data);
//			var dataSize = data.NSample;
//			var nTrees = settings.NbTrees;
//			var trees = new List<RTree>(nTrees);
//			var rand = new Random(1234);
//			var bs = new BootStrap(rand, dataSize, (int)(dataSize * settings.SampleProportion));
//			var treeSettings = new RTreeRegressionSettings(settings.MinNodeSize, PruningType.None, double.NaN);
//			for(int i = 0; i < nTrees; i++) {
//				var bsIndices = bs.DoSample();
//				var bsData = data.BootStrap(bsIndices);
//				trees.Add(GrowTree(bsData, settings.NbSplitVariables, treeSettings));
//			}
//
//			forest = new RForest(trees);
//
//			return new RForestRegressionReport();
		}

		public RForestRegressionReport Train(RData data)
		{
//			var data = RData.FromRawData(x, y);
			var dataSize = data.NSample;
			var nTrees = settings.NbTrees;
			var trees = new List<RTree>(nTrees);
			var rand = new Random(1234);
			var bs = new BootStrap(rand, dataSize, (int)(dataSize * settings.SampleProportion));
			var treeSettings = new RTreeRegressionSettings(settings.MinNodeSize, PruningType.None, double.NaN, settings.NbSplitVariables);

			for(int i = 0; i < nTrees; i++) 
			{
				var sw = Stopwatch.StartNew();
				var bsIndices = bs.DoSample();
				var bsData = data.BootStrap(bsIndices);
				trees.Add(GrowTree(bsData, treeSettings));
				sw.Stop();
				Console.WriteLine (string.Format("Build tree {0}/{1} [n={3}][{2}]", i+1, nTrees, sw.Elapsed, trees.Last().Size()));
			}

			forest = new RForest(trees);

			return new RForestRegressionReport();
		}

		private RTree GrowTree(RData data, RTreeRegressionSettings treeSettings)
		{
			var treeReg = new RTreeRegressor(treeSettings);
			double mse;
//			return treeReg.BuildFullTree(data, out mse);
			var report = treeReg.Train(data);
			return treeReg.Tree;
		}

		public double Evaluate(double[] x)
		{
			return forest.Evaluate(x);
//			//var trees = forest.Trees;
//			var nTrees = settings.NbTrees;
//			var res = 0;
//			for(int i = 0; i < nTrees; i++) {
//				res += treeRegs[i].Evaluate;
//				//TODO : stocker les tree reg au lieu des trees!
//			}
//			return res / nTrees;
		}
	}
}

