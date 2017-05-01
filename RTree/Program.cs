using System;
using System.Linq;
using System.Collections.Generic;

namespace RTree
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			var test = new RTreeTest();
			var data = test.Build1DTestData();

//			var x = string.Join("\r\n", data.Item1.Select(xx=>xx[0]));
//			var y = string.Join("\r\n", data.Item2);

			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);
			var z = string.Join("\r\n", xy);


//			Console.WriteLine(z);


//			var settings = new RTreeRegressionSettings(5, 0.1);
			var settings = new RTreeRegressionSettings(10, 0.1);
			var reg = new RTreeRegressor(settings);
			reg.Train(data.Item1, data.Item2);

			var reggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) {
				reggedY.Add(reg.Evaluate(data.Item1[i]));
			}

			var xyy = xy.Zip(reggedY, (a, b) => a + ";" + b);
			var zz = string.Join("\r\n", xyy);
			Console.WriteLine(zz);

//			Console.WriteLine ("******** x *********");
//			Console.WriteLine(x);
//			Console.WriteLine ("******** y *********");
//			Console.WriteLine(y);
		}
	}
}
