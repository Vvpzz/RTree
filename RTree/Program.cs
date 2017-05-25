using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace RTree
{
	public class Program
	{
		public Program()
		{
		}

		static void Main() 
		{
			var sw = Stopwatch.StartNew();

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


//			var test = new RTreeTestData();
////			var data = test.Build1DTestData(15000);
//			var data = test.Build1DTestData(30000);
//			var x = data.Item1.Select(xx => xx[0]).ToArray();
//			var y = data.Item2;
//			var xy = x.Zip(y, (a,b)=>a+";"+b);
//
//			//TODO : test higher dimensions & split variable
//			var forestSettings = new RForestRegressionSettings(1, 0.6, 100, 6, 0);
////			var forestSettings = new RForestRegressionSettings(150, 0.6, 100, 0);
//			var forestReg = new RForestRegressor(forestSettings);
//			forestReg.Train(data.Item1, data.Item2);
////			var forestReggedY = new List<double>();
//////			for(int i = 0; i < Math.Min(x.Count(), 250); i++) 
////			for(int i = 0; i < x.Count(); i++) 
////			{
////				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
////			}
////			sw.Stop();
//			Console.WriteLine(string.Format("Finished [{0}]", sw.Elapsed));









			var useOld = true;


			var test = new RTreeTestData();
			var data = test.Build1DTestData();

			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);
			var z = string.Join("\r\n", xy);

			var settingsWoPruning = new RTreeRegressionSettings(5, 1000, PruningType.None, 0.1);
			var settings = new RTreeRegressionSettings(10, 1000, PruningType.CostComplexity, 0.1);

			var regWoPruning = new RTreeRegressor(settingsWoPruning);
			var reportWoPruning = regWoPruning.Train(data.Item1, data.Item2, useOld);

			var reg = new RTreeRegressor(settings);
			var report = reg.Train(data.Item1, data.Item2, useOld);

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
			var forestSettings = new RForestRegressionSettings(10, 0.6, 5, 10000, 0);
//			var forestSettings = new RForestRegressionSettings(10, 0.6, 5, 10000, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2, useOld);
			var forestReggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
			}

			var xyy = xy.Zip(reggedYWoPruning, (a, b) => a + ";" + b);
			var xyyy = xyy.Zip(reggedY, (a, b) => a + ";" + b);
			var xyyyf = xyyy.Zip(forestReggedY, (a, b) => a + ";" + b);

			var zz = string.Join("\r\n", xyyyf);
			Console.WriteLine("x;y;yTreeNoPruning;yTreePruning;yForest");
			Console.WriteLine(zz);

			Console.WriteLine ("*** Non Pruned tree ***");
			Console.WriteLine(reportWoPruning);
			var treeWoPruning = regWoPruning.Tree;
			Console.WriteLine(treeWoPruning.Print());
//			Assert.AreEqual(31, treeWoPruning.NbNodes, "Tree (no pruning) size changed");


			Console.WriteLine ("*** Pruned tree ***");
			Console.WriteLine(report);
			var tree = reg.Tree;
			Console.WriteLine(tree.Print());
//			Assert.AreEqual(11, tree.NbNodes, "Tree (pruned) size changed");

			var root = tree.GetRoot();
			Console.WriteLine("Root");
			Console.WriteLine(root);

			var leaves = tree.GetLeaves();
			Console.WriteLine("Leaves");
			Console.WriteLine(string.Join("\n", leaves.Select(l=>l.ToString())));
			var expectedLeavesId = new []{ 34, 35, 40, 42, 44, 45};
			var expectedLeavesSize = new []{ 20, 6, 7, 5, 9, 3};
			var expectedLeavesValue = new []{ 0.182, 0.477, 0.804, 0.968, 1.074, 1.110};
//			Assert.AreEqual(expectedLeavesId.Count(), leaves.Count, "Tree nb leaves changed");
//			for(int i = 0; i < leaves.Count; i++) 
//			{
//				Assert.AreEqual(expectedLeavesId[i], leaves.ElementAt(i).Id, string.Format("Leaf {0} id changed", i));
//				Assert.AreEqual(expectedLeavesSize[i], leaves.ElementAt(i).Length, string.Format("Leaf {0} nb elements changed", i));
//				Assert.AreEqual(expectedLeavesValue[i], leaves.ElementAt(i).Average, 1e-3, string.Format("Leaf {0} value changed", i));
//			}

			Console.WriteLine("*** Prune nodes ***");
			var emptyTree = tree.Prune(0, true);
			Console.WriteLine(emptyTree.Print());

			var oneLeafLessTree = tree.Prune(tree.NodePosition(tree.GetLeaves().ElementAt(0)), true);
			Console.WriteLine(oneLeafLessTree.Print());

			int pos;
			var n = tree.GetChildren(0, out pos);
			var oneHalfTree = tree.Prune(pos, true);
			Console.WriteLine(oneHalfTree.Print());

			var otherHalfTree = tree.Prune(pos + 1, true);
			Console.WriteLine(otherHalfTree.Print());

			var firstChild = otherHalfTree.GetChildren(0, out pos);
			var secondChild = otherHalfTree.GetChildren(pos, out pos);
			var stillSmallerTree = otherHalfTree.Prune(pos, true);
			Console.WriteLine(stillSmallerTree.Print());

			Console.WriteLine("*** Prune nodes (start node not included) ***");
			n = tree.GetChildren(0, out pos);
			var oneHalfTreeNodeNotIncluded = tree.Prune(pos, false);
			Console.WriteLine(oneHalfTreeNodeNotIncluded.Print());

			var oneHalfTreeAgain = tree.Prune(pos, true);
			Console.WriteLine(oneHalfTreeAgain.Print());

			Console.WriteLine("*** SubTree ***");
			var subTree = tree.SubTree(tree.GetChildren(0, out pos).Item1);
			Console.WriteLine(subTree.Print());

			int pp;
			var subTreeFromLeaves = tree.SubTree(tree.GetParent(tree.NodePosition(tree.GetLeaves().ElementAt(3)), out pp));
			Console.WriteLine(subTreeFromLeaves.Print());



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

