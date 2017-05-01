using System;
using System.Collections.Generic;

namespace RTree
{
	public class RTreeRegressionSettings{
		public int MinNodeSize {get; private set;}
		public double PruningCriterion {get; private set;}

		public RTreeRegressionSettings(int minNodeSize, double pruningCriterion)
		{
			MinNodeSize = minNodeSize;
			PruningCriterion = pruningCriterion;
		}
		
	}

	public class RTreeRegressionReport{
		
		
	}

	public class RTreeRegressor
	{
		RTreeRegressionSettings settings;
		RTree tree;
		public RTreeRegressor (RTreeRegressionSettings settings)
		{
			this.settings = settings;
			tree = new RTree();
		}
			
		public RTreeRegressionReport Train(double[][] x, double[] y){
			var largeTree = BuildFullTree(x, y);
//			var prunedTree = PruneTree(largeTree);
			return new RTreeRegressionReport();//TODO
		}

		public double Evaluate(double[] x){
			var leaves = tree.GetLeaves();
			for(int i = 0; i < leaves.Count; i++) {
				var leaf = leaves[i];
				if(tree.EvaluateFullSplitPath(leaf, x))
					return leaf.Data.Average;
			}
			throw new ArgumentOutOfRangeException("x", "No region for this x ?!");
		}
			

		private RTree BuildFullTree(double[][] x, double[] y)
		{
			var data = RData.FromRawData(x, y);
			var rootNode = new RNode(RRegionSplit.None(), data);
			tree.AddChild(null, rootNode);
			RecursiveBuildFullTree(tree, rootNode);

			return tree;
		}

		public void RecursiveBuildFullTree(RTree tree, RNode node)
		{
			var data = node.Data;
			if(data.NSample <= settings.MinNodeSize)
				return;
			var minMse = double.PositiveInfinity;
			RData bestDataL = null, bestDataR = null;
			RRegionSplit bestRegionSplit = null;
			for(int varId = 0; varId < data.NVars; varId++) 
			{				
				var splits = data.ComputeSplitPoints(varId);
				for (int j = 0; j < splits.Length; j++) {
					var rSplit = new RRegionSplit(varId, splits[j], false);
					var partitions = data.Partitions(rSplit);
					var dataL = partitions[0].Item2;
					var dataR = partitions[1].Item2;
					var mse = dataL.MSE + dataR.MSE;
					if(mse<minMse){
						minMse = mse;
						bestDataL = dataL;
						bestDataR = dataR;
						bestRegionSplit = rSplit;
					}						
				}
			}
			//TODO : add info in report

			var nodeL = new RNode(bestRegionSplit, bestDataL);
			var nodeR = new RNode(bestRegionSplit.Complement(), bestDataR);
			tree.AddChildren(node, new List<RNode>(){ nodeL, nodeR });
			RecursiveBuildFullTree(tree, nodeL);
			RecursiveBuildFullTree(tree, nodeR);
		}

		private RTree PruneTree(RTree largeTree)
		{
			throw new NotImplementedException();
		}
	}


}

