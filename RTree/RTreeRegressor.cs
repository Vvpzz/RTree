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

		public RTreeRegressionSettings(int minNodeSize, PruningType pruningType, double pruningCriterion)
		{
			if(minNodeSize<=0 || pruningCriterion<0)
				throw new ArgumentException("Wrong regression settings!");
			
			MinNodeSize = minNodeSize;
			PruningType = pruningType;
			PruningCriterion = pruningCriterion;
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

		public RTree Tree {
			get ;
			private set;
		}

		RTreeRegressionReport report;
		public RTreeRegressor (RTreeRegressionSettings settings)
		{
			this.settings = settings;
			Tree = null;
		}
			
		public RTreeRegressionReport Train(double[][] x, double[] y){
			double mseBeforePruning;
			var largeTree = BuildFullTree(x, y, out mseBeforePruning);
			double mseAfterPruning;
			var prunedTree = PruneTree(largeTree, out mseAfterPruning);
			return new RTreeRegressionReport(mseBeforePruning, mseAfterPruning, largeTree.Size(), prunedTree.Size());//TODO
		}

		public double Evaluate(double[] x){
			var leaves = Tree.GetLeaves();
			for(int i = 0; i < leaves.Count; i++) 
			{
				var leaf = leaves.ElementAt(i);
				if(Tree.EvaluateFullSplitPath(leaf, x))
					return leaf.Data.Average;
			}
			throw new ArgumentOutOfRangeException("x", "No region for this x ?!");
		}
			

		private RTree BuildFullTree(double[][] x, double[] y, out double mse)
		{
			var data = RData.FromRawData(x, y);
			var rootNode = new RNode(RRegionSplit.None(), data);
			Tree = new RTree(rootNode);
			//Tree.AddChild(null, rootNode);
			RecursiveBuildFullTree(Tree, rootNode);

			mse = Tree.MSE();

			return Tree;
		}

		public void RecursiveBuildFullTree(RTree Tree, RNode node)
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
			Tree.AddChildNodes(node, Tuple.Create( nodeL, nodeR ));
			RecursiveBuildFullTree(Tree, nodeL);
			RecursiveBuildFullTree(Tree, nodeR);
		}

		//TODO : écrire cette méthode de manière récursive. Voir ce qu'on veut comme output (arbre + cc)?
		private /*List<Tuple<RTree, double>>*/RTree PruneTree(RTree largeTree, out double mse)
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
				break;
			}
//			var leaves = largeTree.GetLeaves();
//			var nLeaves = leaves.Count;
//			double minCc = double.PositiveInfinity;
//			RTree bestPrunedTree = null;
//			for(int i = 0; i < nLeaves; i++) {
//				RTree prunedTree = Tree.Prune(Tree.GetParent(leaves[i]));
//				if(prunedTree == null)
//					return;
//				var cc = CostComplexityCriterion(prunedTree);
//				if(cc < minCc) 
//				{
//					minCc = cc;
//					bestPrunedTree = prunedTree;
//				}
//			}
//			return PruneTree(bestPrunedTree);


			//algo : on parcourt les feuilles, on en supprime une à chaque fois.
			//On garde le sous-arbre avec une feuille en moins qui ajoute le moins d'erreur.
			//Puis on continue de même sur ce sous-arbre pour supprimer une 2eme feuille.
			//Ainsi de suite jusqu'à ce qu'on ai supprimé toutes les feuilles.

//			ici!!!
			//TODO
			//cost complexity pruning != weakest link pruning!!


			throw new NotImplementedException();
		}
			
	}


}

