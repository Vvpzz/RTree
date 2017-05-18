using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTree
{
	public class RTree
	{
		readonly List<RNode> nodes;
		int treeDepth;

		//set at first evaluation, once the tree is build, and reused after
		List<int> leaves;

		public RTree(RNode root)
		{
			nodes = new List<RNode>(){root};
			treeDepth = 0;
		}

		public static RTree Empty()
		{
			RNode n = null;
			return new RTree(n);
		}

		private RTree(List<RNode> nodes)
		{
			this.nodes = new List<RNode>(nodes);
			this.treeDepth = TreeDepth();		
		}

		public Tuple<RNode, RNode> GetChildren(RNode n)
		{
			int i = nodes.IndexOf(n);
			int lChildPos;
			return GetChildren(i, out lChildPos);
		}

		public Tuple<RNode, RNode> GetChildren(int pos, out int leftChildPos)
		{
			leftChildPos = ChildIndex(pos);

			if(leftChildPos >= nodes.Count)
			{
				leftChildPos = -1;
				return null;
			}

			
			return Tuple.Create(nodes[leftChildPos], nodes[leftChildPos + 1]);
		}

		//TODO : find smthg faster
		public int NbNodes
		{
			get{ return nodes.Count(n => n != null); }
		}

		public int Depth
		{
			get{ return TreeDepth(); }
		}

		public RNode GetRoot()
		{
			return nodes[0];
		}


		public RNode GetParent(RNode n, out int parentPos)
		{
			int i = nodes.IndexOf(n);
			return GetParent(i, out parentPos);
		}

		public RNode GetParent(int pos, out int parentPos)
		{
			parentPos = ParentIndex(pos);

			return parentPos < 0 ? null : nodes[parentPos];
		}

		private int ParentIndex(int childIndex)
		{
			if(childIndex>0)
				return (childIndex - 1) / 2;
			
			return -1;
		}

		public static int ChildIndex(int parentIndex)
		{
			return 2 * parentIndex + 1;
		}


		public static int DepthAtPos(int pos)
		{
			int i = 0;
			int l = 1;
			int u = 2;
			while(!(l - 1 <= pos && pos < u - 1)) 
			{
				i++;
				l *= 2;
				u *= 2;
			}

			return i;
		}

		private int TreeDepth()
		{
			int size = nodes.Count;
			if(size == 0)
				return 0;

			int pow = 1;
			int d = 0;
			int s = size;
			while(s > 1) 
			{
				s /= 2;
				d += 1;
				pow *= 2;
			}

			//sum of series 1+2+4+8+....+2^(d-1) = 2^d-1
			if(size - 2 * pow + 1 != 0)
				throw new ArgumentException("Wrong size!");

			return d;
		}

		private void Resize(int pos)
		{
			//if index is < internal storage size, do nothing
			if(pos <= nodes.Count)
				return;

			//else increment tree depth and resize internal storage
			var d = DepthAtPos(pos) + 1;
			//sum of series 1+2+4+8+....+2^(d-1)
			int size = (int)(Math.Pow(2, d)-1);

			if(size <= nodes.Count)
				throw new ArgumentException("Something is wrong in resize");

			nodes.AddRange(new RNode[size - nodes.Count]);
			treeDepth = TreeDepth();
		}

		public void AddChildNodes(RNode n, RNode leftChild, RNode rightChild, out int leftChildPos)
		{
			int i = nodes.IndexOf(n);
			AddChildNodes(i, leftChild, rightChild, out leftChildPos);
		}


		public void AddChildNodes(int parentPos, RNode leftChild, RNode rightChild, out int leftChildPos)
		{
			leftChildPos = ChildIndex(parentPos);

			Resize(leftChildPos + 1);

			nodes[leftChildPos] = leftChild;
			nodes[leftChildPos+1] = rightChild;
		}

		public List<RNode> GetSlice(int depth)
		{
			int i = (int)Math.Pow(2, depth);
			return nodes.GetRange(i - 1, i);
		}

		public Tuple<int, int> GetSliceRange(int depth)
		{
			int i = (int)Math.Pow(2, depth);
			return Tuple.Create(i - 1, i);
		}

		public List<RNode> GetLeaves()
		{
			//single node : root = leaf
			if(treeDepth == 0)
				return new List<RNode>(){ nodes[0] };
			
			var leavesRg = GetSliceRange(treeDepth);

			var start = leavesRg.Item1;
			var len = leavesRg.Item2;
			var leaves = new List<RNode>(len);
			for(int i = 0; i < len; i++) 
			{
				var idx = start + i;
				var l = nodes[idx];
				int ii = idx;
				while(l == null)
					l = GetParent(ii, out ii);

				//TODO : find smthg faster
				if(!leaves.Contains(l))
					leaves.Add(l);
			}

			return leaves;
		}

		public List<int> GetLeavesPos()
		{
			//single node : root = leaf
			if(treeDepth == 0)
				return new List<int>(){ 0 };

			var leavesRg = GetSliceRange(treeDepth);

			var start = leavesRg.Item1;
			var len = leavesRg.Item2;
			var leaves = new List<int>(len);
			for(int i = 0; i < len; i++) 
			{
				var idx = start + i;
				var l = nodes[idx];
				int ii = idx;
				while(l == null)
					l = GetParent(ii, out ii);

				//TODO : find smthg faster
				if(!leaves.Contains(ii))
					leaves.Add(ii);
			}

			return leaves;
		}

		//probably slow
		public List<RNode> GetInternalNodes()
		{
			return new List<RNode>(nodes.Except(GetLeaves()));
		}

		private bool EvaluateFullSplitPath(RNode n, double[] x)
		{
			int i = nodes.IndexOf(n);
			return EvaluateFullSplitPath(n, i, x);
		}

		private bool EvaluateFullSplitPath(RNode n, int pos, double[] x)
		{
			if(!n.NodeSplit.InDomain(x))
				return false;

			int i = pos;
			var parent = GetParent(i, out i);
			if(parent == null)
				return true;

			return EvaluateFullSplitPath(parent, i, x);
		}

		public double Evaluate(double[] x)
		{
			leaves = leaves ?? GetLeavesPos();
			for(int i = 0; i < leaves.Count; i++) 
			{
				int idx = leaves[i];
				var leaf = nodes[idx];
				if(EvaluateFullSplitPath(leaf, idx, x))
					return leaf.Data.Average;
			}
			throw new ArgumentOutOfRangeException("x", "No region for this x ?!");
		}

		public RTree Clone(){
			return new RTree(nodes);
		}

		public RTree SubTree(RNode node){
			var sub = new RTree(node);
			sub.RecursivePickChildrenFrom(this, node, nodes.IndexOf(node));
			return sub;
		}

		public void RecursivePickChildrenFrom(RTree source, RNode node, int sourcePos){
			if(node == null) return;

			var kids = source.GetChildren(node);
			if(kids == null)
				return;
			int lChildPos;
			AddChildNodes(node, kids.Item1, kids.Item2, out lChildPos);
			RecursivePickChildrenFrom(source, kids.Item1, lChildPos);
			RecursivePickChildrenFrom(source, kids.Item2, lChildPos + 1);
		}
			
		public RTree Prune(RNode n, bool nodeIncluded)
		{
			int pos = nodes.IndexOf(n);
			return Prune(pos, nodeIncluded);
		}

		public RTree Prune(int pos, bool nodeIncluded)
		{
			var t = Clone();
			int leftChildPos;
			var children = t.GetChildren(pos, out leftChildPos);
			if(children != null) 
			{
//				t = t.RemoveNode(children.Item1, true);
//				t = t.RemoveNode(children.Item2, true);
				t = t.Prune(leftChildPos, true);
				t = t.Prune(leftChildPos + 1, true);
			}

			if(!nodeIncluded)
				return t;

			int parentPos;
			var rParent = t.GetParent(pos, out parentPos);
			if(parentPos < 0) //node is root node : return empty tree
				return RTree.Empty();

			if(pos < 0 || t.nodes.Count <= pos)
				throw new ArgumentException();

			t.nodes[pos] = null;

			return t;
		}

		void Chop()
		{

			var leavesRg = GetSliceRange(treeDepth);

			var start = leavesRg.Item1;
			var len = leavesRg.Item2;

			bool canChop = true;
			for(int i = 0; i < len; i++) {
				if(nodes[start + i] != null) {
					canChop = false;
					continue;
				}
			}

			if(!canChop)
				return;
			
			nodes.RemoveRange(start, len);
			treeDepth -= 1;
			Chop();


		}

		public RTree CCPrune(double pruningWeight, out double mse){
			double bestMse = double.PositiveInfinity;
			RTree bestTree = null;
			for(int i = 0; i < nodes.Count; i++) {
				var tmpTree = Prune(nodes[i], false);
				//TODO : Chop() should be part of Prune
				tmpTree.Chop();
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
			var lvs = GetLeaves();
			var nLeaves = lvs.Count;
			double cc = pruningWeight * nLeaves;
			for(int i = 0; i < nLeaves; i++) {
				cc += lvs[i].Data.MSE;
				//TODO : break loop if cc > threshold passed as parameter (i.e. stop evaluate MSE if already too big)
			}
			return cc;
		}


		public double MSE(){
			var lvs = GetLeaves();
			var nLeaves = lvs.Count;
			double mse = 0.0;
			for(int i = 0; i < nLeaves; i++) {
				mse += lvs[i].Data.MSE;
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
				n = GetRoot();

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

