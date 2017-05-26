using System;
using System.Collections.Generic;

namespace RTree
{
	public class RForest
	{
		public List<RTree> Trees {get; private set;}
		public int NbTrees { get {return Trees.Count;} }

		public RForest(List<RTree> trees)
		{
			Trees = trees;
		}

		public double Evaluate(double[] x)
		{
			var nTrees = NbTrees;
			var res = 0.0;
			for(int i = 0; i < nTrees; i++) 
			{
				res += Trees[i].Evaluate(x);
			}
			return res / nTrees;
		}
	}
}

