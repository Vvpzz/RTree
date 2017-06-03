using System;
using System.Collections.Generic;

namespace RTree.Test
{
	public class RTreeTestData
	{
		public RTreeTestData()
		{
		}

		public Tuple<double[][], double[]> Build1DTestData(int size = 50)
		{
			var x = new double[size][];
			var y = new double[size];

			var rand = new Random(1234);

			for(int i = 0; i < size; i++) {
				x[i] = new double[1]{ (10.0 * i) / size - 5 };
				y[i] = 1 / (1.0 + Math.Exp(-1.0 * x[i][0])) + 0.2*rand.NextDouble();
			}

			return Tuple.Create(x, y);
		}

		public Tuple<double[][], double[]> Build2DTestData(int sqrtSize = 50, bool simple=false)
		{
			var x = new List<double[]>();
			var y = new List<double>();

			var rand = new Random(1234);

			const double sigma1 = 0.33;
			const double sigma2 = 0.2;
			const double rho = 0.3;

			for(int i = 0; i < sqrtSize; i++) 
			{
				for (int j = 0; j < sqrtSize; j++) 
				{
					var xx = new double[2]{ (1.0 * i) / sqrtSize - 0.5, (1.0 * j) / sqrtSize - 0.5 };
					x.Add(xx);
					if(simple)
						y.Add(SimpleTest(xx[0], xx[1]));
					else
						y.Add(1 * Gaussian2DDensity(sigma1, sigma2, rho, xx[0], xx[1]) + 0.0 * rand.NextDouble());
				}
			}

			return Tuple.Create(x.ToArray(), y.ToArray());
		}

		public static double Gaussian2DDensity(double sigma1, double sigma2, double rho, double x1, double x2)
		{
			return 1.0 / (2.0 * Math.PI * sigma1 * sigma2 * Math.Sqrt(1.0 - rho * rho)) * Math.Exp(-1.0 / (2.0 * (1.0 - rho * rho)) * (x1 * x1 / sigma1 / sigma1 + x2 * x2 / sigma2 / sigma2 - 2.0 * rho * x1 * x2 / sigma1 / sigma2));
		}

		public static double SimpleTest(double x1, double x2)
		{
			if(x1 <= 0.0 && x2 <= 0.0)
				return 1.0;
			else if(x1 <= 0.0 && x2 > 0.0)
				return 2.0;
			else if(x1 > 0.0 && x2 <= 0.0)
				return 3.0;
			else//if(x1 > 0.0 && x2 > 0.0)
				return 4.0;
		}
	}
}

