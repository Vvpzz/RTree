using System;
using System.Collections.Generic;
using System.Linq;

namespace RTree
{
	public class RTree
	{
		readonly List<RNode> nodes;
		readonly Dictionary<RNode, RNode> parents;

		public RTree ()
		{
			nodes = new List<RNode>();
			parents = new Dictionary<RNode, RNode>();
		}

		public IEnumerable<RNode> GetChildren(RNode n){
			return parents.Where(kv => kv.Value == n).Select(kv => kv.Key);
		}

		public RNode GetParent(RNode n){
			RNode res;
			if (parents.TryGetValue(n, out res))
				return res;

			throw new ArgumentException("Node not found in tree!");
		}

		public void AddChild (RNode parent, RNode child)
		{
//			if(parent == null)
//			{
//				//root node
//				nodes.Add(child);
//				parents.Add(null, child);
//				return;
//			}

			nodes.Add(child);
			parents.Add(child, parent);
		}

		public void AddChildren(RNode parent, List<RNode> children){
			foreach (var child in children) {
				AddChild(parent, child);
			}
		}

		public List<RNode> GetLeaves(){
			var parentNodes = parents.Values.Distinct();
			return nodes.Where(n => !parentNodes.Contains(n)).ToList();
		}

		public bool EvaluateFullSplitPath(RNode n, double[] x){
			if(!n.NodeSplit.InDomain(x))
				return false;

			var parent = parents[n];
			if(parent == null)
				return true;

			return EvaluateFullSplitPath(parent, x);
		}
	}
}

