using System;

namespace RTree
{
	public class RTreeTest
	{
		public RTreeTest()
		{
		}

		public Tuple<double[][], double[]> Build1DTestData()
		{
			const int size = 50;
			var x = new double[size][];
			var y = new double[size];

			var rand = new Random(1234);

			for(int i = 0; i < size; i++) {
				x[i] = new double[1]{ (10.0 * i) / size - 5 };
				y[i] = 1 / (1.0 + Math.Exp(-1.0 * x[i][0])) + 0.2*rand.NextDouble();
			}

			return Tuple.Create(x, y);
		}

	}
}

