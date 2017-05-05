using System;
using System.Collections.Generic;

namespace RTree
{
	public class RTreeRegressionSettings{
		public int MinNodeSize {get; private set;}
		public double PruningCriterion {get; private set;}

		public RTreeRegressionSettings(int minNodeSize, double pruningCriterion)
		{
			if(minNodeSize<=0 || pruningCriterion<0)
				throw new ArgumentException("Wrong regression settings!");
			
			MinNodeSize = minNodeSize;
			PruningCriterion = pruningCriterion;
		}
		
	}

	public class RTreeRegressionReport{
		double mse;
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
			var largeTree = BuildFullTree(x, y);
//			var prunedTree = PruneTree(largeTree);
			return new RTreeRegressionReport();//TODO
		}

		public double Evaluate(double[] x){
			var leaves = Tree.GetLeaves();
			for(int i = 0; i < leaves.Count; i++) {
				var leaf = leaves[i];
				if(Tree.EvaluateFullSplitPath(leaf, x))
					return leaf.Data.Average;
			}
			throw new ArgumentOutOfRangeException("x", "No region for this x ?!");
		}
			

		private RTree BuildFullTree(double[][] x, double[] y)
		{
			var data = RData.FromRawData(x, y);
			var rootNode = new RNode(RRegionSplit.None(), data);
			Tree = new RTree(rootNode);
			//Tree.AddChild(null, rootNode);
			RecursiveBuildFullTree(Tree, rootNode);

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
		private List<Tuple<RTree, double>> PruneTree(RTree largeTree)
		{
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




			throw new NotImplementedException();
		}

		private double CostComplexityCriterion(RTree t){
			var leaves = t.GetLeaves();
			var nLeaves = leaves.Count;
			double cc = settings.PruningCriterion * nLeaves;
			for(int i = 0; i < nLeaves; i++) {
				cc += leaves[i].Data.MSE;
				//TODO : break loop if cc > threshold passed as parameter (i.e. stop evaluate MSE if already too big)
			}
			return cc;
		}
	}


}

