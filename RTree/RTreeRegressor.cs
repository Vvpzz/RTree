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

	public class RTreeRegressionSettings
	{
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
			return Train2(data);
		}


		public RTreeRegressionReport Train2(RData data)
		{
			double mseBeforePruning;
			var largeTree = BuildFullTree2(ref data, out mseBeforePruning);
			double mseAfterPruning;
			var prunedTree = PruneTree(largeTree, out mseAfterPruning);

			Tree = prunedTree;

			Report = new RTreeRegressionReport(mseBeforePruning, mseAfterPruning, largeTree.NbNodes, prunedTree.NbNodes);
			return Report;
		}

		public double Evaluate(double[] x){
			return Tree.Evaluate(x);
		}

		private RTree BuildFullTree2(ref RData data, out double mse)
		{
			var rootNode = RNode.Root(data);
			var buildTree = new RTree(rootNode);
			RecursiveBuildFullTree2(buildTree, data, 0, 0, data.Points.Length);

			mse = buildTree.MSE();

			return buildTree;
		}

		private void RecursiveBuildFullTree2(RTree t, RData data, int pos, int start, int length)
		{
			//			var data = node.Data;
			var nodeDepth = RTree.DepthAtPos(pos);
			if(length <= settings.MinNodeSize || nodeDepth >= settings.MaxTreeDepth)
				return;
			var minMse = double.PositiveInfinity;

			var nSplitVars = settings.NbSplitVariables == 0 ? data.NVars : settings.NbSplitVariables;
			var splitVars = GetSplitVars(data.NVars, nSplitVars);

//			RData bestDataL = null, bestDataR = null;
//			RRegionSplit bestRegionSplit = null;
			double mse, avgL, avgR, mseL, mseR;
			int bestSplit = -1, bestVarId = -1;
			double bestAvgL = double.NaN, bestAvgR = double.NaN, bestMseL = double.NaN, bestMseR = double.NaN;
			for(int i = 0; i < nSplitVars; i++) 
			{				
				int varId = splitVars[i];

				data.SortBetween(varId, start, length);
				int split = data.BestSplitBetween(varId, start, length, out mse, out avgL, out avgR, out mseL, out mseR);
//				var splits = data.ComputeSplitPoints(varId);
//				RData dataL = RData.Empty(data.NVars, varId);
//				RData dataR = new RData(data);
//				for (int j = 0; j < splits.Length; j++) 
//				{
//					var lowerSplit = new RRegionSplit(varId, splits[j], false);
//					data.IterativePartitions(lowerSplit, ref dataL, ref dataR);
//					int split = data.BestSplitBetween(start, length, out avgL, out avgR, out mseL, out mseR);
//					var mse = dataL.MSE + dataR.MSE;
				if(mse<minMse)
				{
					minMse = mse;
					bestSplit = split;
					bestAvgL = avgL;
					bestAvgR = avgR;
					bestMseL = mseL;
					bestMseR = mseR;
					bestVarId = varId;
				}						
//				}
			}
			//TODO : add info in report
			//TODO : check start index
//			var nodeL = new RNode(bestRegionSplit, 0, bestDataL.NSample, bestDataL.Average, bestDataL.MSE);
//			var nodeR = new RNode(bestRegionSplit.Complement(), 0, bestDataR.NSample, bestDataR.Average, bestDataR.MSE);

			var lengthL = bestSplit - start + 1;
			var lengthR = length - lengthL;
//			var splitL = new RRegionSplit(bestVarId, data.Points[bestSplit].Xs[bestVarId], false);
			var mid = 0.5 * (data.Points[bestSplit].Xs[bestVarId] + data.Points[bestSplit + 1].Xs[bestVarId]);
			var splitL = new RRegionSplit(bestVarId, mid, false);
			var splitR = splitL.Complement();
			var nodeL = new RNode(splitL, start, lengthL, bestAvgL, bestMseL);
			var nodeR = new RNode(splitR, bestSplit + 1, lengthR, bestAvgR, bestMseR);

			int leftChildPos;
			t.AddChildNodes(pos, nodeL, nodeR, out leftChildPos);
			RecursiveBuildFullTree2(t, data, leftChildPos, start, lengthL);
			RecursiveBuildFullTree2(t, data, leftChildPos + 1, bestSplit + 1, lengthR);
		}

		private static int[] GetSplitVars(int nVars, int nSplitVars)
		{
			int[] splitVars;
			if(nSplitVars != nVars) 
			{
				var rnd = new Random(1234);
				var bs = new BootStrap(rnd, nVars, nSplitVars);
				splitVars = bs.DoSample();
			}
			else 
			{
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

