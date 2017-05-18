using System;

namespace RTree
{

	//bootstrap = random draw with replacement
	public class BootStrap{

		readonly Random rand;
		readonly int sourceSize;
		readonly int randomSampleSize;
		readonly int[] sourceIndices;

		public BootStrap(Random rand, int sourceSize, int randomSampleSize)
		{
			this.rand = rand;
			this.sourceSize = sourceSize;
			this.randomSampleSize = randomSampleSize;
			sourceIndices = new int[sourceSize];
			for(int i = 0; i < sourceSize; i++) {
				sourceIndices[i] = i;
			}
		}
		
		public int[] DoSample()
		{
			var bs = new int[randomSampleSize];
			for(int i = 0; i < randomSampleSize; i++) {
				bs[i] = rand.Next(0, sourceSize);
			}
			return bs;
		}
	}
}

