﻿using System;
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

		int nSplitVars;
		int[] splitVars;

		int lastSortedVar=-1;

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
		}


		public RTreeRegressionReport Train(RData data)
		{
			double mseBeforePruning;
			var largeTree = BuildFullTree(ref data, out mseBeforePruning);
			double mseAfterPruning;
			var prunedTree = PruneTree(largeTree, out mseAfterPruning);

			Tree = prunedTree;

			Report = new RTreeRegressionReport(mseBeforePruning, mseAfterPruning, largeTree.NbNodes, prunedTree.NbNodes);
			return Report;
		}

		public double Evaluate(double[] x){
			return Tree.Evaluate(x);
		}

		private RTree BuildFullTree(ref RData data, out double mse, bool breadthFirst = true)
		{
			if(breadthFirst) 
			{
				// Breadth first (non recursive) tree building. Faster than depth first.
				nSplitVars = settings.NbSplitVariables == 0 ? data.NVars : settings.NbSplitVariables;
				splitVars = GetSplitVars(data.NVars, nSplitVars);
				var t = BuildFullTreeBreadthFirst(data);
				mse = t.MSE();
				return t;
			} 
			else 
			{
				// Depth first (recursive) tree building
				var rootNode = RNode.Root(data);
				var buildTree = new RTree(rootNode);
				nSplitVars = settings.NbSplitVariables == 0 ? data.NVars : settings.NbSplitVariables;
				splitVars = GetSplitVars(data.NVars, nSplitVars);
				//sort on first dimension
				data.SortBetween(0, 0, data.Points.Length);
				lastSortedVar = 0;
				BuildFullTreeDepthFirst(buildTree, data, 0, 0, data.Points.Length, data.Average(0, data.Points.Length), data.MSE(0, data.Points.Length));
				mse = buildTree.MSE();
				return buildTree;
			}
		}
			
		private RTree BuildFullTreeBreadthFirst(RData data)
		{
			//sort on first dimension
			SortIfNeeded(data, 0, data.Points.Length, 0, -1);

			var rootNode = RNode.Root(data);
			RTree t =  new RTree(rootNode);
			int pos = 0;
			var nodeDepth = RTree.DepthAtPos(0);


			bool exit = false;
			while(!exit)
			{

				var parentSlice = t.GetSliceRange(nodeDepth);
				var parentStart = parentSlice.Item1;
				var parentLength = parentSlice.Item2;
				var parentEnd = parentStart + parentLength;
				exit = true;

				for(int p = parentStart; p < parentEnd; p++) 
				{
					var parentNode = t.GetNodeAtPos(p);
					if(parentNode == null)
					{
						pos += 1;
						continue;
					}
					
					var length = parentNode.Length;
					if(length <= settings.MinNodeSize)
					{
						pos += 1;
						continue;
					}

					//we will add a node, so we won't exit at this depth
					exit = false;

					var start = parentNode.Start;
					var parentAvg = parentNode.Average;
					var parentMse = parentNode.MSE;

					int parentSort = parentNode.NodeSplit.VarId;
					OptimizeSplitVars(parentSort);

					var minMse = double.PositiveInfinity;
					double mse, avgL, avgR, mseL, mseR;
					int bestSplit = -1, bestVarId = -1;
					double bestAvgL = double.NaN, bestAvgR = double.NaN, bestMseL = double.NaN, bestMseR = double.NaN;
					for(int i = 0; i < nSplitVars; i++) 
					{				
						int varId = splitVars[i];

						SortIfNeeded(data, start, length, varId, parentSort);

						int split = data.BestSplitBetween(varId, start, length, out mse, out avgL, out avgR, out mseL, out mseR, parentAvg, parentMse);
						//The second (arbitrary) condition is added so as not to depend on the order of the tested split variables
						if(mse<minMse || (mse==minMse && varId<bestVarId))
						{
							minMse = mse;
							bestSplit = split;
							bestAvgL = avgL;
							bestAvgR = avgR;
							bestMseL = mseL;
							bestMseR = mseR;
							bestVarId = varId;
						}						
					}

					//Sort according to the best split (needed for evaluation)
					SortIfNeeded(data, start, length, bestVarId, splitVars[nSplitVars-1]);

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
					pos += 1;
				}

				nodeDepth += 1;
				if(nodeDepth >= settings.MaxTreeDepth)
					exit = true;
			}
			return t;
		}


		private void BuildFullTreeDepthFirst(RTree t, RData data, int pos, int start, int length, double parentAvg, double parentMse)
		{
			var nodeDepth = RTree.DepthAtPos(pos);
			if(length <= settings.MinNodeSize || nodeDepth >= settings.MaxTreeDepth)
				return;
			
			var minMse = double.PositiveInfinity;

			OptimizeSplitVars();

			double mse, avgL, avgR, mseL, mseR;
			int bestSplit = -1, bestVarId = -1;
			double bestAvgL = double.NaN, bestAvgR = double.NaN, bestMseL = double.NaN, bestMseR = double.NaN;
			for(int i = 0; i < nSplitVars; i++) 
			{				
				int varId = splitVars[i];

				SortIfNeeded(data, start, length, varId);

				int split = data.BestSplitBetween(varId, start, length, out mse, out avgL, out avgR, out mseL, out mseR, parentAvg, parentMse);
				if(mse<minMse || (mse==minMse && varId<bestVarId))
				{
					minMse = mse;
					bestSplit = split;
					bestAvgL = avgL;
					bestAvgR = avgR;
					bestMseL = mseL;
					bestMseR = mseR;
					bestVarId = varId;
				}						
			}

			//Sort according to the best split
			SortIfNeeded(data, start, length, bestVarId);

			//TODO : add info in report
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
			BuildFullTreeDepthFirst(t, data, leftChildPos, start, lengthL, bestAvgL, bestMseL);

			//Sort according to the best split
			SortIfNeeded(data, start, length, bestVarId);

			BuildFullTreeDepthFirst(t, data, leftChildPos + 1, bestSplit + 1, lengthR, bestAvgR, bestMseR);
		}

		private void SortIfNeeded(RData data, int start, int length, int varId)
		{
			if(varId != lastSortedVar) 
			{
				data.SortBetween(varId, start, length);
				lastSortedVar = varId;
			}
		}

		private void SortIfNeeded(RData data, int start, int length, int varId, int alreadySorted)
		{
			if(varId != alreadySorted) 
			{
				data.SortBetween(varId, start, length);
				lastSortedVar = varId;
			}
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

		void OptimizeSplitVars()
		{
			if(splitVars[0] == lastSortedVar)
				return;
			
			int i = Array.IndexOf(splitVars, lastSortedVar);
			var temp = splitVars[0];
			splitVars[0] = lastSortedVar;
			splitVars[i] = temp;
		}

		void OptimizeSplitVars(int alreadySorted)
		{
			if(splitVars[0] == alreadySorted)
				return;

			int i = Array.IndexOf(splitVars, alreadySorted);
			var temp = splitVars[0];
			splitVars[0] = alreadySorted;
			splitVars[i] = temp;
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

