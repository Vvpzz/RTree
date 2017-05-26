
namespace RTree
{
	public class RRegionSplit
	{
		public int VarId { get; private set; }
		readonly double varLimit;
		readonly bool strictlyGreater;

		public RRegionSplit(int varId, double varLimit, bool strictlyGreater)
		{
			VarId = varId;
			this.varLimit = varLimit;
			this.strictlyGreater = strictlyGreater;
		}

		public bool InDomain(double[] xs)
		{
			if(strictlyGreater)
				return xs[VarId] > varLimit;
			else 
				return xs[VarId] <= varLimit;	
		}

		public RRegionSplit Complement()
		{
			return new RRegionSplit(VarId, varLimit, !strictlyGreater);
		}

		public static RRegionSplit None()
		{
			return new RRegionSplit(0, double.NegativeInfinity, true);
		}

		public override string ToString()
		{
			return string.Format("x{0} {1} {2:N1}", VarId, strictlyGreater ? ">" : "<=", varLimit);
		}
	}
}

