using System;

namespace RTree
{
	public class BaseRNode
	{
		static int staticId = 0;
		public int Id {get; private set;}

		public BaseRNode ()
		{
			Id = staticId++;
		}
	}

	public class RNode : BaseRNode
	{
		public RRegionSplit NodeSplit { get; private set; }

		public int Start { get; private set; }
		public int Length { get; private set; }
		public double Average { get; private set; }
		public double MSE { get; private set; }

		public RNode(RRegionSplit split, int start, int length, double avg, double mse)
		{
			NodeSplit = split;
			Start = start;
			Length = length;
			Average = avg;
			MSE = mse;
		}

		public static RNode Root(RData data)
		{
			return new RNode(RRegionSplit.None(), 0, data.NSample, data.Average(0, data.NSample), data.MSE(0, data.NSample));
		}

		public override string ToString(){
			return string.Format("[{3}] {0} ; nX:{1} ; avgX:{2:0.000}", NodeSplit, Length, Average, Id);
		}
	}
}

