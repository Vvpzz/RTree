using System;

namespace RTree
{
//	public interface IRNode : IEquatable<IRNode>{
//		int Id {get;}
//	}

	//TODO : distinguish left nodes (<=) from right nodes (>)?
	public class BaseRNode//:IEquatable<BaseRNode>//:IRNode
	{
		static int staticId = 0;
		public int Id {get; private set;}

		public BaseRNode ()
		{
			Id = staticId++;
		}

//		bool IEquatable<IRNode>.Equals (IRNode other)
//		bool IEquatable<BaseRNode>.Equals (BaseRNode other)
//		{
//			return Id == other.Id;
//		}
	}

	public class RNode : BaseRNode
	{
		public RRegionSplit NodeSplit { get; private set; }
		//TODO : replace RData by a smaller object containing only the useful information  (NSample, Average, MSE)
		public RData Data { get; private set; }

		public RNode(RRegionSplit split, RData data)
		{
			NodeSplit = split;
			Data = data;
		}

		public static RNode Root(RData data)
		{
			return new RNode(RRegionSplit.None(), data);
		}

		public override string ToString(){
			return string.Format("[{3}] {0} ; nX:{1} ; avgX:{2:0.000}", NodeSplit.ToString(), Data.NSample, Data.Average, Id);
		}
	}
}

