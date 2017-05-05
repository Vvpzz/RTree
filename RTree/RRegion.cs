using System;
using System.Collections.Generic;

namespace RTree
{
//	public class RRegion{
//		public List<RRegionSplit> splits;
//
//		public RRegion()
//		{
//			splits = new List<RRegionSplit>();
//		}
//
//		public RRegion Append(RRegionSplit split){
//			splits.Add(split);
//		}
//			
//
//		public bool InDomain(double[] xs){			
//			for(int i = 0; i < splits.Count; i++)
//				//idea : last added splits are more discriminant than the first ones?
//				//for(int i = splits.Count-1; i >= 0; i--) 
//			{
//				if(!splits[i].InDomain(xs)) return false;
//			}
//			return true;
//		}
//
//	}

	public class RRegionSplit{
		readonly int varId;
		readonly double varLimit;
		readonly bool strictlyGreater;
		readonly Func<double, bool> inDomain;

		public RRegionSplit(int varId, double varLimit, bool strictlyGreater)
		{
			this.varId = varId;
			this.varLimit = varLimit;
			this.strictlyGreater = strictlyGreater;
			if(strictlyGreater)
				inDomain = x => x > varLimit;
			else 
				inDomain =  x => x <= varLimit;				
		}

		public bool InDomain(double[] xs)
		{
			return inDomain(xs[varId]);
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
}

