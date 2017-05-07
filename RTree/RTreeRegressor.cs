using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTree
{
	public enum PruningType{
		CostComplexity,
		WeakestLink,
		None
	}

	public class RTreeRegressionSettings{
		public int MinNodeSize {get; private set;}
		public PruningType PruningType {get; private set;}
		public double PruningCriterion {get; private set;}
		public int NbSplitVariables {get; private set;}

		public RTreeRegressionSettings(int minNodeSize, PruningType pruningType, double pruningCriterion, int nbSplitVariables = 0)
		{
			if(minNodeSize<=0 || pruningCriterion<0)
				throw new ArgumentException("Wrong regression settings!");
			
			MinNodeSize = minNodeSize;
			PruningType = pruningType;
			PruningCriterion = pruningCriterion;
			NbSplitVariables = nbSplitVariables;
		}
		
	}

	public class RTreeRegressionReport{

		public double MseBeforePruning {get; private set;}
		public double MseAfterPruning {get; private set;}
		public int TreeSizeBeforePruning {get; private set;}
		public int TreeSizeAfterPruning {get; private set;}

		public RTreeRegressionReport(double mseBeforePruning, double mseAfterPruning, int treeSizeBeforePruning, int treeSizeAfterPruning)
		{
			MseBeforePruning = mseBeforePruning;
			MseAfterPruning = mseAfterPruning;
			TreeSizeBeforePruning = treeSizeBeforePruning;
			TreeSizeAfterPruning = treeSizeAfterPruning;
		}

		public override string ToString(){
			var sb = new StringBuilder();
			sb.AppendLine(string.Format("{0}:{1:0.000}", "MSE before pruning", MseBeforePruning));
			sb.AppendLine(string.Format("{0}:{1:0.000}", "MSE after pruning", MseAfterPruning));
			sb.AppendLine(string.Format("Nb nodes before pruning:{0}", TreeSizeBeforePruning));
			sb.AppendLine(string.Format("Nb nodes after pruning:{0}", TreeSizeAfterPruning));
			return sb.ToString();
		}
		
	}

	public class RTreeRegressor
	{
		readonly RTreeRegressionSettings settings;

		public RTree Tree { get; private set; }
		public RTreeRegressionReport Report { get; private set; }

		public RTreeRegressor (RTreeRegressionSettings settings)
		{
			this.settings = settings;
			Tree = null;
		}
			
		public RTreeRegressionReport Train(double[][] x, double[] y)
		{
			var data = RData.FromRawData(x, y);
			return Train(data);
//			double mseBeforePruning;
//			var largeTree = BuildFullTree(data, out mseBeforePruning);
//			double mseAfterPruning;
//			var prunedTree = PruneTree(largeTree, out mseAfterPruning);
//			Report = new RTreeRegressionReport(mseBeforePruning, mseAfterPruning, largeTree.Size(), prunedTree.Size());
//			return Report;
		}

		public RTreeRegressionReport Train(RData data)
		{
//			var data = RData.FromRawData(x, y);
			double mseBeforePruning;
			var largeTree = BuildFullTree(data, out mseBeforePruning);
			double mseAfterPruning;
			var prunedTree = PruneTree(largeTree, out mseAfterPruning);
			Report = new RTreeRegressionReport(mseBeforePruning, mseAfterPruning, largeTree.Size(), prunedTree.Size());
			return Report;
		}

		//TODO : move to RTree : a tree should know how to evaluate one self
		// 3 levels : regressor, data structure, data structure builder
		public double Evaluate(double[] x){
			return Tree.Evaluate(x);
//			var leaves = Tree.GetLeaves();
//			for(int i = 0; i < leaves.Count; i++) 
//			{
//				var leaf = leaves.ElementAt(i);
//				if(Tree.EvaluateFullSplitPath(leaf, x))
//					return leaf.Data.Average;
//			}
//			throw new ArgumentOutOfRangeException("x", "No region for this x ?!");
		}
			

//		public RTree BuildFullTree(double[][] x, double[] y, out double mse)
//		{
//			var data = RData.FromRawData(x, y);
//			var rootNode = new RNode(RRegionSplit.None(), data);
//			Tree = new RTree(rootNode);
//			//Tree.AddChild(null, rootNode);
//			RecursiveBuildFullTree(Tree, rootNode);
//
//			mse = Tree.MSE();
//
//			return Tree;
//		}

		private RTree BuildFullTree(RData data, out double mse)
		{
			var rootNode = new RNode(RRegionSplit.None(), data);
			Tree = new RTree(rootNode);
			//Tree.AddChild(null, rootNode);
			RecursiveBuildFullTree(Tree, rootNode);

			mse = Tree.MSE();

			return Tree;
		}

		private void RecursiveBuildFullTree(RTree t, RNode node)
		{
			var data = node.Data;
			if(data.NSample <= settings.MinNodeSize)
				return;
			var minMse = double.PositiveInfinity;

			var nSplitVars = settings.NbSplitVariables == 0 ? data.NVars : settings.NbSplitVariables;
			var splitVars = GetSplitVars(data.NVars, nSplitVars);

			RData bestDataL = null, bestDataR = null;
			RRegionSplit bestRegionSplit = null;
//			for(int varId = 0; varId < data.NVars; varId++) 
			for(int i = 0; i < nSplitVars; i++) 
			{				
				int varId = splitVars[i];
				var splits = data.ComputeSplitPoints(varId);
				for (int j = 0; j < splits.Length; j++) 
				{
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
			t.AddChildNodes(node, Tuple.Create( nodeL, nodeR ));
			RecursiveBuildFullTree(t, nodeL);
			RecursiveBuildFullTree(t, nodeR);
		}

		private static int[] GetSplitVars(int nVars, int nSplitVars)
		{
			int[] splitVars;
			if(nSplitVars != nVars) {
				var rnd = new Random();
				var bs = new BootStrap(rnd, nVars, nSplitVars);
				splitVars = bs.DoSample();
			}
			else {
				splitVars = Enumerable.Range(0, nVars).ToArray();
			}
			return splitVars;
		}
			
		private RTree PruneTree(RTree largeTree, out double mse)
		{
			switch(settings.PruningType) 
			{
			case PruningType.CostComplexity:
				return largeTree.CCPrune(settings.PruningCriterion, out mse);
			case PruningType.WeakestLink:
				return largeTree.WLPrune(out mse);
			case PruningType.None:
				mse = double.NaN;
				return largeTree;
			default:
				throw new ArgumentException("Unknown pruning type");
//				break;
			}
		}
			
	}


}

