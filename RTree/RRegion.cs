using System;
using System.Collections.Generic;

namespace RTree
{
	public class RRegionSplit{
		readonly int varId;
		readonly double varLimit;
		readonly bool strictlyGreater;

		public RRegionSplit(int varId, double varLimit, bool strictlyGreater)
		{
			this.varId = varId;
			this.varLimit = varLimit;
			this.strictlyGreater = strictlyGreater;
		}

		public bool InDomain(double[] xs)
		{
			if(strictlyGreater)
				return xs[varId] > varLimit;
			else 
				return xs[varId] <= varLimit;	
		}

		public RRegionSplit Complement()
		{
			return new RRegionSplit(varId, varLimit, !strictlyGreater);
		}

		public static RRegionSplit None()
		{
			return new RRegionSplit(0, double.NegativeInfinity, true);
		}

		public override string ToString()
		{
			return string.Format("x{0} {1} {2:N1}", varId, strictlyGreater ? ">" : "<=", varLimit);
		}
	}

//	public abstract class RRegionSplit{
//		protected readonly int varId;
//		protected readonly double varLimit;
//
//		public RRegionSplit(int varId, double varLimit)
//		{
//			this.varId = varId;
//			this.varLimit = varLimit;
//		}
//
//		public abstract bool InDomain(double[] xs);
//
//		public static UpperRegionSplit None()
//		{
//			return new UpperRegionSplit(0, double.NegativeInfinity);
//		}
//	}
//
//	public class LowerRegionSplit : RRegionSplit
//	{
//		public LowerRegionSplit(int varId, double varLimit) : base(varId, varLimit)
//		{
//		}
//
//		public override bool InDomain(double[] xs)
//		{
//			return xs[varId] <= varLimit;
//		}
//
//		public UpperRegionSplit Complement()
//		{
//			return new UpperRegionSplit(varId, varLimit);
//		}
//
//		public override string ToString()
//		{
//			return string.Format("x{0} {1} {2:N1}", varId, "<=", varLimit);
//		}
//	}
//
//	public class UpperRegionSplit : RRegionSplit
//	{
//		public UpperRegionSplit(int varId, double varLimit) : base(varId, varLimit)
//		{
//		}
//
//		public override bool InDomain(double[] xs)
//		{
//			return xs[varId] > varLimit;
//		}
//
//		public LowerRegionSplit Complement()
//		{
//			return new LowerRegionSplit(varId, varLimit);
//		}
//
//		public override string ToString()
//		{
//			return string.Format("x{0} {1} {2:N1}", varId, ">", varLimit);
//		}
//	}
}

