using System;
using System.Collections.Generic;

namespace RTree
{
	public interface IRNode : IEquatable<IRNode>{
		int Id {get;}
	}

	//TODO : distinguish left nodes (<=) from right nodes (>)?
	public class BaseRNode:IRNode
	{
		static int staticId = 0;
		public int Id {get; private set;}

		public BaseRNode ()
		{
			Id = staticId++;
		}

		bool IEquatable<IRNode>.Equals (IRNode other)
		{
			return Id == other.Id;
		}
	}

	public class RNode : BaseRNode{
		public RRegionSplit NodeSplit { get; private set; }
		public RData Data { get; private set; }

		public RNode(RRegionSplit split, RData data)
		{
			NodeSplit = split;
			Data = data;
		}

		public override string ToString(){
			return string.Format("[{3}] {0} ; nX:{1} ; avgX:{2:0.000}", NodeSplit.ToString(), Data.NSample, Data.Average, Id);
		}
	}
}

