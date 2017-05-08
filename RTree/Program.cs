using System;
using System.Linq;
using System.Collections.Generic;

namespace RTree
{
	public class Program
	{
		public Program()
		{
		}

		static void Main() 
		{

//			var test = new RTreeTestData();
//			var data = test.Build1DTestData(5000);
//			var x = data.Item1.Select(xx => xx[0]).ToArray();
//			var y = data.Item2;
//			var xy = x.Zip(y, (a,b)=>a+";"+b);
//
//			//TODO : test higher dimensions & split variable
//			var forestSettings = new RForestRegressionSettings(10, 0.6, 5, 0);
//			var forestReg = new RForestRegressor(forestSettings);
//			forestReg.Train(data.Item1, data.Item2);
//			var forestReggedY = new List<double>();
//			for(int i = 0; i < x.Count(); i++) 
//			{
//				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
//			}
//
//			var xyf = xy.Zip(forestReggedY, (a, b) => a + ";" + b);
//
//			var zz = string.Join("\r\n", xyf);
//			Console.WriteLine("x;y;yTreeNoPruning;yTreePruning;yForest");
//			Console.WriteLine(zz);


			var test = new RTreeTestData();
			var data = test.Build1DTestData(30000);
			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);

			//TODO : test higher dimensions & split variable
			var forestSettings = new RForestRegressionSettings(150, 0.6, 100, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2);
			var forestReggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
			}
		}
	}
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

	}
}

