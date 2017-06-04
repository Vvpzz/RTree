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
			var d = new RData(points);

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


//		[Test()]
//		public void RDataSortInvarianceTest()
//		{
//			var points = new RDataPoint[] 
//			{
//				new RDataPoint(new[]{ 0.4,	0 }, 1.12761555588819),
//				new RDataPoint(new[]{ 0.4, 0.2 }, 0.970693150910113),
//				new RDataPoint(new[]{ 0.4, 	-0.1 }, 0.804883637720712),
//				new RDataPoint(new[]{ 0.45, 0.15 }, 0.935915715800037),
//				new RDataPoint(new[]{ 0.45,	0.1 }, 0.993109678409756),
//				new RDataPoint(new[]{ 0.45,	-0.05 }, 0.785798383808597),
//				new RDataPoint(new[]{ 0.45,	0.2 }, 0.823471073082816)
//			};
//
//			var pointsRef = new RDataPoint[] 
//			{
//				new RDataPoint(new[]{ 0.4,	0 }, 1.12761555588819),
//				new RDataPoint(new[]{ 0.4, 0.2 }, 0.970693150910113),
//				new RDataPoint(new[]{ 0.4, 	-0.1 }, 0.804883637720712),
//				new RDataPoint(new[]{ 0.45, 0.15 }, 0.935915715800037),
//				new RDataPoint(new[]{ 0.45,	0.1 }, 0.993109678409756),
//				new RDataPoint(new[]{ 0.45,	-0.05 }, 0.785798383808597),
//				new RDataPoint(new[]{ 0.45,	0.2 }, 0.823471073082816)
//			};
//
//			var d = new RData(points);
//
//			d.SortBetween(0, 0, d.NSample);
//			d.SortBetween(1, 0, d.NSample);
//			d.SortBetween(0, 0, d.NSample);
//
//			for(int i = 0; i < d.NSample; i++) 
//			{
//				Assert.AreEqual(pointsRef[i], d.Points[i].Y);
//			}
//		}

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
			var d = new RData(points);

			var newforestSettings = new RTreeRegressionSettings(3, 2, PruningType.None, 0.0, 0);
			var newforestReg = new RTreeRegressor(newforestSettings);
			newforestReg.Train(d);

			Console.WriteLine(newforestReg.Tree.Print());

			Assert.AreEqual(2.0, newforestReg.Tree.GetLeaves()[0].Average);
			Assert.AreEqual(5.0, newforestReg.Tree.GetLeaves()[1].Average);
		}

		[Test()]
		public void TestAvgAndMse()
		{
//			var points = new RDataPoint[] 
//			{
//				new RDataPoint(new[]{ 1.0 }, 1.0),
//				new RDataPoint(new[]{ 1.0 }, 2.0),
//				new RDataPoint(new[]{ 1.0 }, 3.0),
//				new RDataPoint(new[]{ 2.0 }, 4.0),
//				new RDataPoint(new[]{ 2.0 }, 5.0),
//				new RDataPoint(new[]{ 2.0 }, 6.0),
//			};
//			var d = new RData(points);

			const double tol = 1e-10;

			var testData = new RTreeTestData();
			var t = testData.Build2DTestData(20, true);
			var d = RData.FromRawData(t.Item1, t.Item2);

			d.SortBetween(1, 0, d.NSample);

			var lavg = 0.0;
			var lmse = 0.0;
			var ravg = d.Average(0, d.NSample);
			var rmse = d.MSE(0, d.NSample);
			for(int i = 0; i < d.NSample; i++) 
			{
				var llen = i + 1;
				d.PostAddUpdate(d.Points[i], ref lavg, ref lmse, llen);
				Assert.AreEqual(d.Average(0, llen), lavg, tol);
				Assert.AreEqual(d.MSE(0, llen), lmse, tol);

				var rlen = d.NSample - (i+1);
				d.PostRemoveUpdate(d.Points[i], ref ravg, ref rmse, rlen);
				Assert.AreEqual(d.Average(i + 1, rlen), ravg, tol);
				Assert.AreEqual(d.MSE(i + 1, rlen), rmse, tol);
			}
		}

		[Test()]
		public void TestAllRegressors()
		{

			var test = new RTreeTestData();
			var data = test.Build1DTestData();

			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);
//			var z = string.Join("\r\n", xy);

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
//				Assert.AreEqual(expectedLeavesId[i], leaves.ElementAt(i).Id, string.Format("Leaf {0} id changed", i));
				Assert.AreEqual(expectedLeavesSize[i], leaves.ElementAt(i).Length, string.Format("Leaf {0} nb elements changed", i));
				Assert.AreEqual(expectedLeavesValue[i], leaves.ElementAt(i).Average, 1e-3, string.Format("Leaf {0} value changed", i));
			}

			Console.WriteLine("*** Prune nodes ***");
			var emptyTree = tree.Prune(0, true);
			Console.WriteLine(emptyTree.Print());

			var oneLeafLessTree = tree.Prune(tree.NodePosition(tree.GetLeaves().ElementAt(0)), true);
			Console.WriteLine(oneLeafLessTree.Print());

			int pos;
			tree.GetChildren(0, out pos);
			var oneHalfTree = tree.Prune(pos, true);
			Console.WriteLine(oneHalfTree.Print());

			var otherHalfTree = tree.Prune(pos + 1, true);
			Console.WriteLine(otherHalfTree.Print());

			otherHalfTree.GetChildren(0, out pos);
			otherHalfTree.GetChildren(pos, out pos);
			var stillSmallerTree = otherHalfTree.Prune(pos, true);
			Console.WriteLine(stillSmallerTree.Print());

			Console.WriteLine("*** Prune nodes (start node not included) ***");
			tree.GetChildren(0, out pos);
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
		public void TestForestPerformance()
		{
			var test = new RTreeTestData();
			var data = test.Build1DTestData(30000);
			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
//			var xy = x.Zip(y, (a,b)=>a+";"+b);

			//TODO : test higher dimensions & split variable
			var forestSettings = new RForestRegressionSettings(2, 0.6, 100, 10, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2);
			var forestReggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
			}

//			var xyf = xy.Zip(forestReggedY, (a, b) => a + ";" + b);

		}

		[TestCase(true, 2.6396765002892344E-27d)]
		[TestCase(false, 1.7981593095030839d)]
		public void TestForestPerformance2D(bool simple, double refMse)
		{
			var test = new RTreeTestData();
			var data = test.Build2DTestData(20, simple);
			var x0 = data.Item1.Select(xx => xx[0]).ToArray();
//			var x1 = data.Item1.Select(xx => xx[1]).ToArray();
			var y = data.Item2;
			//			var xy = x.Zip(y, (a,b)=>a+";"+b);

			//TODO : test higher dimensions & split variable
			var forestSettings = new RForestRegressionSettings(20, 0.6, 5, 10, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2);
			//var forestReggedY = new List<double>();
			var mse = 0.0;
			for(int i = 0; i < x0.Count(); i++) 
			{
				//forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
				var tmp = forestReg.Evaluate(data.Item1[i]) - data.Item2[i];
				mse += tmp * tmp;
			}

			Console.WriteLine("MSE: " + mse);
			Assert.AreEqual(refMse, mse, 1e-15);
			//			var xyf = xy.Zip(forestReggedY, (a, b) => a + ";" + b);
		}


		[Test()]
		public void TestForestEvaluation()
		{
			double[] refValues = new [] {
				0.1211461703,
				0.1314006898,
				0.12872291,
				0.1378334936,
				0.1352627394,
				0.1423049561,
				0.1443278168,
				0.1388164412,
				0.1388164412,
				0.137867436,
				0.1483338997,
				0.1909797281,
				0.2126487945,
				0.2316715111,
				0.2358588814,
				0.239405308,
				0.23479131,
				0.2334155596,
				0.2662490828,
				0.2911787512,
				0.3591968655,
				0.4618653432,
				0.4618653432,
				0.4618653432,
				0.4871457261,
				0.5431490375,
				0.6449899862,
				0.7066126753,
				0.7113507352,
				0.8251085951,
				0.8565529704,
				0.8693359839,
				0.9151732749,
				0.937368033,
				0.9489392521,
				0.9562703963,
				0.9614375279,
				0.9867738548,
				1.0450933577,
				1.0623219222,
				1.0718411237,
				1.0718411237,
				1.0574780435,
				1.0750955891,
				1.0768251073,
				1.0768251073,
				1.0774689565,
				1.0834017085,
				1.0834017085,
				1.0782784906
			};

			var newtest = new RTreeTestData();
			var newdata = newtest.Build1DTestData();

			var newx = newdata.Item1.Select(xx => xx[0]).ToArray();
			var newy = newdata.Item2;
			var newxy = newx.Zip(newy, (a,b)=>a+";"+b);
//			var newz = string.Join("\r\n", newxy);

			var newforestSettings = new RForestRegressionSettings(20, 0.6, 5, 10000, 0);
			var newforestReg = new RForestRegressor(newforestSettings);
			newforestReg.Train(newdata.Item1, newdata.Item2);
			var newforestReggedY = new List<double>();

			for(int i = 0; i < newx.Count(); i++) 
			{
				newforestReggedY.Add(newforestReg.Evaluate(newdata.Item1[i]));
				Assert.AreEqual(refValues[i], newforestReg.Evaluate(newdata.Item1[i]), 1e-10);
			}

			var newxyyyf = newxy.Zip(newforestReggedY, (a, b) => a + ";" + b);

			var newzz = string.Join("\r\n", newxyyyf);
			Console.WriteLine("********* New version *********");
			Console.WriteLine("x;y;yForest");
			Console.WriteLine(newzz);
			Console.WriteLine("********* New version (end) *********");
		}
	}
}

