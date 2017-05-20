using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace RTree
{
	public enum PruningType{
		CostComplexity,
		WeakestLink,
		None
	}

	public class RTreeRegressionSettings{
		public int MinNodeSize {get; private set;}
		public int MaxTreeDepth {get; private set;}
		public PruningType PruningType {get; private set;}
		public double PruningCriterion {get; private set;}
		public int NbSplitVariables {get; private set;}

		public RTreeRegressionSettings(int minNodeSize, int maxTreeDepth, PruningType pruningType, double pruningCriterion, int nbSplitVariables = 0)
		{
			if(minNodeSize<=0 || maxTreeDepth<0 || pruningCriterion<0)
				throw new ArgumentException("Wrong regression settings!");
			
			MinNodeSize = minNodeSize;
			MaxTreeDepth = maxTreeDepth;
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

		//TODO : warning, Train assumes x is sorted (1D case). What happens in nD?
		public RTreeRegressionReport Train(double[][] x, double[] y)
		{
			var data = RData.FromRawData(x, y);
			return Train(data);
		}

		public RTreeRegressionReport Train(RData data)
		{
			double mseBeforePruning;
			var largeTree = BuildFullTree(data, out mseBeforePruning);
			double mseAfterPruning;
			var prunedTree = PruneTree(largeTree, out mseAfterPruning);

			Tree = prunedTree;

			Report = new RTreeRegressionReport(mseBeforePruning, mseAfterPruning, largeTree.NbNodes, prunedTree.NbNodes);
			return Report;
		}

		public double Evaluate(double[] x){
			return Tree.Evaluate(x);
		}
			

		private RTree BuildFullTree(RData data, out double mse)
		{
			var rootNode = RNode.Root(data);
			var buildTree = new RTree(rootNode);
			RecursiveBuildFullTree(buildTree, rootNode, 0);

			mse = buildTree.MSE();

			return buildTree;
		}

		private void RecursiveBuildFullTree(RTree t, RNode node, int pos)
		{
			var data = node.Data;
			var nodeDepth = RTree.DepthAtPos(pos);
			if(data.NSample <= settings.MinNodeSize || nodeDepth >= settings.MaxTreeDepth)
				return;
			var minMse = double.PositiveInfinity;

			var nSplitVars = settings.NbSplitVariables == 0 ? data.NVars : settings.NbSplitVariables;
			var splitVars = GetSplitVars(data.NVars, nSplitVars);

			RData bestDataL = null, bestDataR = null;
			RRegionSplit bestRegionSplit = null;
			for(int i = 0; i < nSplitVars; i++) 
			{				
				int varId = splitVars[i];
				var splits = data.ComputeSplitPoints(varId);
				RData dataL = RData.Empty(data.NVars, varId);
				RData dataR = new RData(data);
				for (int j = 0; j < splits.Length; j++) 
				{
					var lowerSplit = new RRegionSplit(varId, splits[j], false);
					data.IterativePartitions(lowerSplit, ref dataL, ref dataR);
					var mse = dataL.MSE + dataR.MSE;
					if(mse<minMse){
						minMse = mse;
						bestDataL = new RData(dataL);
						bestDataR = new RData(dataR);
						bestRegionSplit = lowerSplit;
					}						
				}
			}
			//TODO : add info in report
			var nodeL = new RNode(bestRegionSplit, bestDataL);
			var nodeR = new RNode(bestRegionSplit.Complement(), bestDataR);
			int leftChildPos;
			t.AddChildNodes(pos, nodeL, nodeR, out leftChildPos);
			RecursiveBuildFullTree(t, nodeL, leftChildPos);
			RecursiveBuildFullTree(t, nodeR, leftChildPos + 1);
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
			var prunedTree = largeTree.Clone();
			switch(settings.PruningType) 
			{
			case PruningType.CostComplexity:
				prunedTree = largeTree.CCPrune(settings.PruningCriterion, out mse);
				break;
			case PruningType.WeakestLink:
				prunedTree = largeTree.WLPrune(out mse);
				break;
			case PruningType.None:
				mse = double.NaN;
				break;
			default:
				throw new ArgumentException("Unknown pruning type");
			}

			return prunedTree;
		}
			
	}


}

