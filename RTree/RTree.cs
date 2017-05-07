using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTree
{
	public class RTree
	{
		//TODO : use set instead of list
		readonly HashSet<RNode> nodes;
		readonly Dictionary<RNode, RNode> parents;
		readonly Dictionary<RNode, Tuple<RNode, RNode>> children;
		readonly RNode root;

		public RTree(RNode root)
		{
			this.root = root;
			nodes = new HashSet<RNode>(){root};
			if(root == null)
				return;

			parents = new Dictionary<RNode, RNode>(){{root, null}};
			children = new Dictionary<RNode, Tuple<RNode, RNode>>();
		}

		public static RTree Empty()
		{
			return new RTree(null);
		}

//		private RTree(RTree tree) : this(tree.root, tree.nodes, tree.parents, tree.children)
//		{
//		}

		private RTree(RNode root, HashSet<RNode> nodes, Dictionary<RNode, RNode> parents, Dictionary<RNode, Tuple<RNode, RNode>> children)
		{
			this.root = root;
			this.nodes = new HashSet<RNode>(nodes);
			this.parents = new Dictionary<RNode, RNode>(parents);
			this.children = children.ToDictionary(kv => kv.Key, kv => Tuple.Create(kv.Value.Item1, kv.Value.Item2));
		}

		public Tuple<RNode, RNode> GetChildren(RNode n){
			//return parents.Where(kv => kv.Value == n).Select(kv => kv.Key);
			Tuple<RNode, RNode> kids;
			if(children.TryGetValue(n, out kids))
				return kids;

			return kids;
		}

		public int Size(){
			return nodes.Count;
		}

		public RNode GetRoot()
		{
			//return parents.Where(kv => kv.Value == null).Select(kv => kv.Key).Single();
			return root;
		}

		public RNode TryGetRoot()
		{
			return root;
			//return parents.Where(kv => kv.Value == null).Select(kv => kv.Key).SingleOrDefault();
		}

		public RNode GetParent(RNode n){
			RNode res;
			if (parents.TryGetValue(n, out res))
				return res;

			throw new ArgumentException("Node not found in tree!");
		}

		public void AddChildNodes (RNode parent, Tuple<RNode, RNode> lAndRChildren)
		{
//			if(parent == null)
//			{
//				//root node
//				nodes.Add(child);
//				parents.Add(null, child);
//				return;
//			}

			var lChild = lAndRChildren.Item1;
			var rChild = lAndRChildren.Item2;
			nodes.Add(lChild);
			nodes.Add(rChild);
			parents.Add(lChild, parent);
			parents.Add(rChild, parent);

			//Root node is noone's child
			if(parent == null)
				return;

//			Tuple<RNode, RNode> kids;
//			if(children.TryGetValue(parent, out kids))
//				kids.Add(lAndRChildren);
//			else
//				children.Add(parent, new List<RNode>(){ child });
			children.Add(parent, lAndRChildren);
		}

//		public void AddChildren(RNode parent, List<RNode> children){
//			foreach (var child in children) {
//				AddChild(parent, child);
//			}
//		}

		public HashSet<RNode> GetLeaves()
		{
			var parentNodes = children.Keys;//parents.Values.Distinct();
			return new HashSet<RNode>(nodes.Where(n => !parentNodes.Contains(n)));
		}

		public HashSet<RNode> GetInternalNodes()
		{
			return new HashSet<RNode>(nodes.Except(GetLeaves()));
		}

		private bool EvaluateFullSplitPath(RNode n, double[] x)
		{
			if(!n.NodeSplit.InDomain(x))
				return false;

			var parent = parents[n];
			if(parent == null)
				return true;

			return EvaluateFullSplitPath(parent, x);
		}


		public double Evaluate(double[] x)
		{
			var leaves = GetLeaves();
			for(int i = 0; i < leaves.Count; i++) 
			{
				var leaf = leaves.ElementAt(i);
				if(EvaluateFullSplitPath(leaf, x))
					return leaf.Data.Average;
			}
			throw new ArgumentOutOfRangeException("x", "No region for this x ?!");
		}

		public RTree Clone(){
			return new RTree(root, nodes, parents, children);
		}

		public RTree SubTree(RNode node){
			var sub = new RTree(node);
			sub.RecursivePickChildrenFrom(this, node);
			return sub;
		}

		public void RecursivePickChildrenFrom(RTree source, RNode node){
			if(node == null) return;

			var kids = source.GetChildren(node);
			if(kids == null)
				return;
			AddChildNodes(node, kids);
			RecursivePickChildrenFrom(source, kids.Item1);
			RecursivePickChildrenFrom(source, kids.Item2);
		}

//		public RTree Prune(RNode node)
//		{
//			var t = Clone();
//			var rParent = t.GetParent(node);
//			if(rParent == null) //node is root node : return empty tree
//				return RTree.Empty();
//
//			//remove reference to node as child
//			var nodeAsChild = t.children[rParent];
//			if(nodeAsChild.Item1 == node)
//				nodeAsChild = Tuple.Create<RNode, RNode>(null, nodeAsChild.Item2);
//			else
//				nodeAsChild = Tuple.Create<RNode, RNode>(nodeAsChild.Item1, null);
//			
//			if(nodeAsChild.Item1 == null && nodeAsChild.Item2 == null)
//				t.children.Remove(rParent);
//			else
//				t.children[rParent] = nodeAsChild;
//
//			//remove node's children recursively
//			var kids = t.GetChildren(node);
//			if(kids != null) 
//			{
//				t.Prune(kids.Item1);
//				t.Prune(kids.Item2);
//			}
//
//			//cleaning containers
//			t.nodes.Remove(node);
//			t.parents.Remove(node);
//			t.children.Remove(node);
//
//			return t;
//		}

		public RTree Prune(RNode node, bool nodeIncluded)
		{
			var t = Clone();

			//remove node's children recursively
			var kids = t.GetChildren(node);
			if(kids != null) 
			{
				t = t.Prune(kids.Item1, true);
				t = t.Prune(kids.Item2, true);
			}

			if(!nodeIncluded)
				return t;
				
			var rParent = t.GetParent(node);
			if(rParent == null) //node is root node : return empty tree
				return RTree.Empty();

			//remove reference to node as child
			var nodeAsChild = t.children[rParent];
			if(nodeAsChild.Item1 == node)
				nodeAsChild = Tuple.Create<RNode, RNode>(null, nodeAsChild.Item2);
			else
				nodeAsChild = Tuple.Create<RNode, RNode>(nodeAsChild.Item1, null);

			if(nodeAsChild.Item1 == null && nodeAsChild.Item2 == null)
				t.children.Remove(rParent);
			else
				t.children[rParent] = nodeAsChild;

			//cleaning containers
			t.nodes.Remove(node);
			t.parents.Remove(node);
			t.children.Remove(node);

			return t;
		}

		public RTree CCPrune(double pruningWeight, out double mse){
			double bestMse = double.PositiveInfinity;
			RTree bestTree = null;
			for(int i = 0; i < nodes.Count; i++) {
				var tmpTree = Prune(nodes.ElementAt(i), false);
				var tmpMse = tmpTree.CostComplexityCriterion(pruningWeight);//tmpTree.MSE();
				if(tmpMse<bestMse)
				{
					bestMse = tmpMse;
					bestTree = tmpTree;
				}
			}
			mse = bestMse;
			return bestTree;
		}

		private double CostComplexityCriterion(double pruningWeight){
			var leaves = GetLeaves();
			var nLeaves = leaves.Count;
			double cc = pruningWeight * nLeaves;
			for(int i = 0; i < nLeaves; i++) {
				cc += leaves.ElementAt(i).Data.MSE;
				//TODO : break loop if cc > threshold passed as parameter (i.e. stop evaluate MSE if already too big)
			}
			return cc;
		}


		public double MSE(){
			var leaves = GetLeaves();
			var nLeaves = leaves.Count;
			double mse = 0.0;
			for(int i = 0; i < nLeaves; i++) {
				mse += leaves.ElementAt(i).Data.MSE;
				//TODO : break loop if cc > threshold passed as parameter (i.e. stop evaluate MSE if already too big)
			}
			return mse;
		}

		public RTree WLPrune(out double mse){
			throw new NotImplementedException();
		}

		public string Print(RNode n = null, string prefix = null)
		{
			if(n == null)
				n = TryGetRoot();

			if(n==null)
				return "Null tree";

			var sb = new StringBuilder(n.ToString());
			sb.AppendLine();
			var kids = GetChildren(n);
			prefix += "\t";
			if(kids != null) 
			{
				if(kids.Item1 != null)
					sb.Append(string.Format("{1}{0}", Print(kids.Item1, prefix), prefix));
				if(kids.Item2 != null)
					sb.Append(string.Format("{1}{0}", Print(kids.Item2, prefix), prefix));
			}
			return sb.ToString();
		}
	}
}

