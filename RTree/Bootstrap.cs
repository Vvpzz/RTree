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

//	public static class BootstrapExtension
//	{
//		public static T[] BootStrap<T>(this T[] source, int[] sampleIndices)
//		{
//			int n = sampleIndices.Length;
//			var res = new T[n];
//			for(int i = 0; i < n; i++) {
//				res[i] = source[sampleIndices[i]];
//			}
//			return res;
//		}	
//	}


//	public class Randomizer
//	{
//		readonly Random rand;
////		readonly double[][] x;
////		readonly double[] y;
//		readonly int randomSampleSize;
//		readonly int[] randomizedIndices;
//
//		public Randomizer(/*double[][] x, double[] y,*/ int randomSampleSize)
//		{
//			rand = new Random(1234);
////			this.x = x;
////			this.y = y;
//			this.randomSampleSize = randomSampleSize;
//			randomizedIndices = new int[randomSampleSize];
//		}
//
//		public void Randomize()
//		{
//			randomizedIndices = bla;
//		}
//
//		public T[] RandomSample<T>(T[] source, int[] randomizedIndices)
//		{
//			var sample = new T[randomSampleSize];
//
//			for(int i = 0; i < sample.Length; i++) {
//				sample[i] = source[randomizedIndices[i]];
//			}
//
//			return sample;
//		}
//	}
//
//	static class RandomExtensions
//	{
//		//Fisher-Yates
//		public static void Shuffle<T> (this Random rng, T[] array)
//		{
//			int n = array.Length;
//			while (n > 1) 
//			{
//				int k = rng.Next(n--);
//				T temp = array[n];
//				array[n] = array[k];
//				array[k] = temp;
//			}
//		}
//	}
}

