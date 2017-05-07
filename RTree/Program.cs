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


			var settingsWoPruning = new RTreeRegressionSettings(5, PruningType.None, 0.1);
			var settings = new RTreeRegressionSettings(10, PruningType.CostComplexity, 0.1);

			var regWoPruning = new RTreeRegressor(settingsWoPruning);
			var reportWoPruning = regWoPruning.Train(data.Item1, data.Item2);

			var reg = new RTreeRegressor(settings);
			var report = reg.Train(data.Item1, data.Item2);

			var reggedYWoPruning = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				reggedYWoPruning.Add(regWoPruning.Evaluate(data.Item1[i]));
			}

			var reggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				reggedY.Add(reg.Evaluate(data.Item1[i]));
			}

			//TODO : test split variable
			var forestSettings = new RForestRegressionSettings(10, 0.6, 5, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2);
			var forestReggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
			}

			var xyy = xy.Zip(reggedYWoPruning, (a, b) => a + ";" + b);
			var xyyy = xyy.Zip(reggedY, (a, b) => a + ";" + b);
			var xyyyf = xyyy.Zip(forestReggedY, (a, b) => a + ";" + b);

			var zz = string.Join("\r\n", xyyyf);
			Console.WriteLine(zz);

			Console.WriteLine ("*** Non Pruned tree ***");
			Console.WriteLine(reportWoPruning);
			var treeWoPruning = regWoPruning.Tree;
			Console.WriteLine(treeWoPruning.Print());


			Console.WriteLine ("*** Pruned tree ***");
			Console.WriteLine(report);
			var tree = reg.Tree;
			Console.WriteLine(tree.Print());

			var root = tree.GetRoot();
			Console.WriteLine("Root");
			Console.WriteLine(root.ToString());

			var leaves = tree.GetLeaves();
			Console.WriteLine("Leaves");
			Console.WriteLine(string.Join("\n", leaves.Select(l=>l.ToString())));


			Console.WriteLine("*** Prune nodes ***");
			var emptyTree = tree.Prune(tree.GetRoot(), true);
			Console.WriteLine(emptyTree.Print());

			var oneLeafLessTree = tree.Prune(tree.GetLeaves().ElementAt(0), true);
			Console.WriteLine(oneLeafLessTree.Print());

			var oneHalfTree = tree.Prune(tree.GetChildren(root).Item1, true);
			Console.WriteLine(oneHalfTree.Print());

			var otherHalfTree = tree.Prune(tree.GetChildren(root).Item2, true);
			Console.WriteLine(otherHalfTree.Print());

			var stillSmallerTree = otherHalfTree.Prune(otherHalfTree.GetChildren(otherHalfTree.GetChildren(otherHalfTree.GetChildren(root).Item1).Item1).Item1, true);
			Console.WriteLine(stillSmallerTree.Print());

			Console.WriteLine("*** Prune nodes (start node not included) ***");
			var oneHalfTreeNodeNotIncluded = tree.Prune(tree.GetChildren(root).Item1, false);
			Console.WriteLine(oneHalfTreeNodeNotIncluded.Print());

			var oneHalfTreeAgain = tree.Prune(tree.GetChildren(root).Item1, true);
			Console.WriteLine(oneHalfTreeAgain.Print());

			Console.WriteLine("*** SubTree ***");
			var subTree = tree.SubTree(tree.GetChildren(root).Item1);
			Console.WriteLine(subTree.Print());

			var subTreeFromLeaves = tree.SubTree(tree.GetParent(tree.GetLeaves().ElementAt(3)));
			Console.WriteLine(subTreeFromLeaves.Print());
//			Console.WriteLine ("******** x *********");
//			Console.WriteLine(x);
//			Console.WriteLine ("******** y *********");
//			Console.WriteLine(y);
		}
	}
}
