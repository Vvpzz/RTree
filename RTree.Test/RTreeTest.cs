using NUnit.Framework;
using System;
using RTree;
using System.Linq;
using System.Collections.Generic;

namespace RTree.Test
{
	[TestFixture()]
	public class RTreeTest
	{
		[SetUp]
		public void Setup()
		{
			
		}

		[Test()]
		public void BasicTest()
		{
			Assert.AreEqual(0, RTree.DepthAtPos(0));
			Assert.AreEqual(1, RTree.DepthAtPos(1));
			Assert.AreEqual(1, RTree.DepthAtPos(2));
			Assert.AreEqual(2, RTree.DepthAtPos(3));
			Assert.AreEqual(2, RTree.DepthAtPos(6));
			Assert.AreEqual(3, RTree.DepthAtPos(7));
			Assert.AreEqual(3, RTree.DepthAtPos(14));
			Assert.AreEqual(4, RTree.DepthAtPos(15));
		}

		[Test()]
		public void RDataTest()
		{
			const double tol = 1e-10;
			var d = RData.Empty(1, 0);
			var dp1 = new RDataPoint(new[]{ 1.0 }, 1.0);
			var dp2 = new RDataPoint(new[]{ 2.0 }, 2.0);
			var dp3 = new RDataPoint(new[]{ 3.0 }, 3.0);
			var dp4 = new RDataPoint(new[]{ 4.0 }, 4.0);
			d.Add(dp1, 0);
			Assert.AreEqual(1, d.Average);
			Assert.AreEqual(0, d.MSE);
			d.Add(dp2, 0);
			Assert.AreEqual(1.5, d.Average);
			Assert.AreEqual(0.5, d.MSE);
			d.RemoveAt(1);
			Assert.AreEqual(1, d.Average);
			Assert.AreEqual(0, d.MSE);
			d.Add(dp2, 0);
			Assert.AreEqual(1.5, d.Average);
			Assert.AreEqual(0.5, d.MSE);
			d.Add(dp3, 0);
			Assert.AreEqual(2, d.Average);
			Assert.AreEqual(2, d.MSE);
			d.Add(dp4, 0);
			Assert.AreEqual(2.5, d.Average);
			Assert.AreEqual(5, d.MSE);
			d.RemoveAt(2);
			Assert.AreEqual(2.3333333333, d.Average, tol);
			Assert.AreEqual(4.6666666667, d.MSE, tol);
		}

		[Test()]
		public void RDataSortBetweenTest()
		{
			var points = new RDataPoint[] 
			{
				new RDataPoint(new[]{ 1.0, 0.8 }, 1.0),
				new RDataPoint(new[]{ 2.0, 0.6 }, 2.0),
				new RDataPoint(new[]{ 3.0, 0.9 }, 3.0),
				new RDataPoint(new[]{ 4.0, 0.1 }, 4.0),
				new RDataPoint(new[]{ 5.0, 0.2 }, 5.0),
				new RDataPoint(new[]{ 6.0, 0.5 }, 6.0),
				new RDataPoint(new[]{ 7.0, 0.4 }, 7.0),
				new RDataPoint(new[]{ 8.0, 0.3 }, 8.0)
			};
			var d = new RData(points.ToList(), 2, 0);

			d.SortBetween(1, 0, 5);
			d.SortBetween(1, 5, 3);
			Assert.AreEqual(d.Points[0].Xs[0], 4);
			Assert.AreEqual(d.Points[0].Xs[1], 0.1);
			Assert.AreEqual(d.Points[1].Xs[0], 5);
			Assert.AreEqual(d.Points[1].Xs[1], 0.2);
			Assert.AreEqual(d.Points[2].Xs[0], 2);
			Assert.AreEqual(d.Points[2].Xs[1], 0.6);
			Assert.AreEqual(d.Points[3].Xs[0], 1);
			Assert.AreEqual(d.Points[3].Xs[1], 0.8);
			Assert.AreEqual(d.Points[4].Xs[0], 3);
			Assert.AreEqual(d.Points[4].Xs[1], 0.9);
			Assert.AreEqual(d.Points[5].Xs[0], 8);
			Assert.AreEqual(d.Points[5].Xs[1], 0.3);
			Assert.AreEqual(d.Points[6].Xs[0], 7);
			Assert.AreEqual(d.Points[6].Xs[1], 0.4);
			Assert.AreEqual(d.Points[7].Xs[0], 6);
			Assert.AreEqual(d.Points[7].Xs[1], 0.5);


			d.SortBetween(0, 0, 2);
			d.SortBetween(0, 2, 3);
			d.SortBetween(0, 5, 2);
			Assert.AreEqual(d.Points[0].Xs[0], 4);
			Assert.AreEqual(d.Points[0].Xs[1], 0.1);
			Assert.AreEqual(d.Points[1].Xs[0], 5);
			Assert.AreEqual(d.Points[1].Xs[1], 0.2);
			Assert.AreEqual(d.Points[2].Xs[0], 1);
			Assert.AreEqual(d.Points[2].Xs[1], 0.8);
			Assert.AreEqual(d.Points[3].Xs[0], 2);
			Assert.AreEqual(d.Points[3].Xs[1], 0.6);
			Assert.AreEqual(d.Points[4].Xs[0], 3);
			Assert.AreEqual(d.Points[4].Xs[1], 0.9);
			Assert.AreEqual(d.Points[5].Xs[0], 7);
			Assert.AreEqual(d.Points[5].Xs[1], 0.4);
			Assert.AreEqual(d.Points[6].Xs[0], 8);
			Assert.AreEqual(d.Points[6].Xs[1], 0.3);
			Assert.AreEqual(d.Points[7].Xs[0], 6);
			Assert.AreEqual(d.Points[7].Xs[1], 0.5);


		}

		[Test()]
		public void RData2SortBetweenTest()
		{
			var points = new RDataPoint[] 
			{
				new RDataPoint(new[]{ 1.0, 0.8 }, 1.0),
				new RDataPoint(new[]{ 2.0, 0.6 }, 2.0),
				new RDataPoint(new[]{ 3.0, 0.9 }, 3.0),
				new RDataPoint(new[]{ 4.0, 0.1 }, 4.0),
				new RDataPoint(new[]{ 5.0, 0.2 }, 5.0),
				new RDataPoint(new[]{ 6.0, 0.5 }, 6.0),
				new RDataPoint(new[]{ 7.0, 0.4 }, 7.0),
				new RDataPoint(new[]{ 8.0, 0.3 }, 8.0)
			};
			var d = new RData2(points);

			d.SortBetween(1, 0, 5);
			d.SortBetween(1, 5, 3);
			Assert.AreEqual(d.Points[0].Xs[0], 4);
			Assert.AreEqual(d.Points[0].Xs[1], 0.1);
			Assert.AreEqual(d.Points[1].Xs[0], 5);
			Assert.AreEqual(d.Points[1].Xs[1], 0.2);
			Assert.AreEqual(d.Points[2].Xs[0], 2);
			Assert.AreEqual(d.Points[2].Xs[1], 0.6);
			Assert.AreEqual(d.Points[3].Xs[0], 1);
			Assert.AreEqual(d.Points[3].Xs[1], 0.8);
			Assert.AreEqual(d.Points[4].Xs[0], 3);
			Assert.AreEqual(d.Points[4].Xs[1], 0.9);
			Assert.AreEqual(d.Points[5].Xs[0], 8);
			Assert.AreEqual(d.Points[5].Xs[1], 0.3);
			Assert.AreEqual(d.Points[6].Xs[0], 7);
			Assert.AreEqual(d.Points[6].Xs[1], 0.4);
			Assert.AreEqual(d.Points[7].Xs[0], 6);
			Assert.AreEqual(d.Points[7].Xs[1], 0.5);


			d.SortBetween(0, 0, 2);
			d.SortBetween(0, 2, 3);
			d.SortBetween(0, 5, 2);
			Assert.AreEqual(d.Points[0].Xs[0], 4);
			Assert.AreEqual(d.Points[0].Xs[1], 0.1);
			Assert.AreEqual(d.Points[1].Xs[0], 5);
			Assert.AreEqual(d.Points[1].Xs[1], 0.2);
			Assert.AreEqual(d.Points[2].Xs[0], 1);
			Assert.AreEqual(d.Points[2].Xs[1], 0.8);
			Assert.AreEqual(d.Points[3].Xs[0], 2);
			Assert.AreEqual(d.Points[3].Xs[1], 0.6);
			Assert.AreEqual(d.Points[4].Xs[0], 3);
			Assert.AreEqual(d.Points[4].Xs[1], 0.9);
			Assert.AreEqual(d.Points[5].Xs[0], 7);
			Assert.AreEqual(d.Points[5].Xs[1], 0.4);
			Assert.AreEqual(d.Points[6].Xs[0], 8);
			Assert.AreEqual(d.Points[6].Xs[1], 0.3);
			Assert.AreEqual(d.Points[7].Xs[0], 6);
			Assert.AreEqual(d.Points[7].Xs[1], 0.5);


		}

		[Test()]
		public void TestSameXs()
		{
			var points = new RDataPoint[] 
			{
				new RDataPoint(new[]{ 1.0 }, 1.0),
				new RDataPoint(new[]{ 1.0 }, 2.0),
				new RDataPoint(new[]{ 1.0 }, 3.0),
				new RDataPoint(new[]{ 2.0 }, 4.0),
				new RDataPoint(new[]{ 2.0 }, 5.0),
				new RDataPoint(new[]{ 2.0 }, 6.0),
			};
			var d = new RData2(points);

			var newforestSettings = new RTreeRegressionSettings(3, 2, PruningType.None, 0.0, 0);
			var newforestReg = new RTreeRegressor(newforestSettings);
			newforestReg.Train2(d);

			Console.WriteLine(newforestReg.Tree.Print());

			Assert.AreEqual(2.0, newforestReg.Tree.GetLeaves()[0].Average);
			Assert.AreEqual(5.0, newforestReg.Tree.GetLeaves()[1].Average);
		}

		[Test()]
		public void TestAllRegressors()
		{

			var test = new RTreeTestData();
			var data = test.Build1DTestData();

			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);
			var z = string.Join("\r\n", xy);

			var settingsWoPruning = new RTreeRegressionSettings(5, 1000, PruningType.None, 0.1);
			var settings = new RTreeRegressionSettings(10, 1000, PruningType.CostComplexity, 0.1);

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
			var forestSettings = new RForestRegressionSettings(10, 0.6, 5, 10000, 0);
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
			Console.WriteLine("x;y;yTreeNoPruning;yTreePruning;yForest");
			Console.WriteLine(zz);

			Console.WriteLine ("*** Non Pruned tree ***");
			Console.WriteLine(reportWoPruning);
			var treeWoPruning = regWoPruning.Tree;
			Console.WriteLine(treeWoPruning.Print());
			Assert.AreEqual(31, treeWoPruning.NbNodes, "Tree (no pruning) size changed");


			Console.WriteLine ("*** Pruned tree ***");
			Console.WriteLine(report);
			var tree = reg.Tree;
			Console.WriteLine(tree.Print());
			Assert.AreEqual(11, tree.NbNodes, "Tree (pruned) size changed");

			var root = tree.GetRoot();
			Console.WriteLine("Root");
			Console.WriteLine(root);

			var leaves = tree.GetLeaves();
			Console.WriteLine("Leaves");
			Console.WriteLine(string.Join("\n", leaves.Select(l=>l.ToString())));
			var expectedLeavesId = new []{ 34, 35, 40, 42, 44, 45};
			var expectedLeavesSize = new []{ 20, 6, 7, 5, 9, 3};
			var expectedLeavesValue = new []{ 0.182, 0.477, 0.804, 0.968, 1.074, 1.110};
			Assert.AreEqual(expectedLeavesId.Count(), leaves.Count, "Tree nb leaves changed");
			for(int i = 0; i < leaves.Count; i++) 
			{
				Assert.AreEqual(expectedLeavesId[i], leaves.ElementAt(i).Id, string.Format("Leaf {0} id changed", i));
				Assert.AreEqual(expectedLeavesSize[i], leaves.ElementAt(i).Length, string.Format("Leaf {0} nb elements changed", i));
				Assert.AreEqual(expectedLeavesValue[i], leaves.ElementAt(i).Average, 1e-3, string.Format("Leaf {0} value changed", i));
			}

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

		[Test()]
		public void TestForestPerformance(){
			var test = new RTreeTestData();
			var data = test.Build1DTestData(30000);
			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);

			//TODO : test higher dimensions & split variable
			var forestSettings = new RForestRegressionSettings(2, 0.6, 100, 10, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2);
			var forestReggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
			}

			var xyf = xy.Zip(forestReggedY, (a, b) => a + ";" + b);

		}
	}
}

