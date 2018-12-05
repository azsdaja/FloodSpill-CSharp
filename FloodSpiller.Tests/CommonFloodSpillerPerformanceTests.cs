using System;
using System.Diagnostics;
using FloodSpiller.Queues;
using FloodSpiller.Utilities;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace FloodSpiller.Tests
{
	[TestFixture]
	public class CommonFloodSpillerPerformanceTests
	{
		[Test]
		public void SpillFlood_BigAreaWithVerticalWalls_ScanlineAndFifoPerformanceComparison()
		{
			int size = 1000;
			var result = new int[size, size];

			var walkability = new bool[size, size];
			// vertical walls should be OPTIMAL for scanline because it skips opening nodes when going vertically
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					bool isWalkable = x % 2 == 0 || y == size / 2;
					walkability[x, y] = isWalkable;
				}
			}

			var startPosition = new Position(size / 5, size / 5);

			Predicate<int, int> qualifier = (x, y) => walkability[x, y];
			var parameters = new FloodParameters(new LifoQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier
			};

			var stopwatch = Stopwatch.StartNew();
			new FloodScanlineSpiller().SpillFlood(parameters, result);
			Console.WriteLine("scanline:" + stopwatch.ElapsedMilliseconds + " ms");
			stopwatch.Restart();
			new FloodSpiller().SpillFlood(parameters, result);
			Console.WriteLine("normal:" + stopwatch.ElapsedMilliseconds + " ms");
		}

		[Test]
		public void SpillFlood_BigAreaWithHorizontalWalls_ScanlineAndFifoPerformanceComparison()
		{
			int size = 1000;
			var result = new int[size, size];

			var walkability = new bool[size, size];
			// horizontal walls should be INOPTIMAL for scanline because it skips opening nodes when going vertically
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					bool isWalkable = x == size / 2 || y % 2 == 0;
					walkability[x, y] = isWalkable;
				}
			}

			var startPosition = new Position(size / 5, size / 5);

			Predicate<int, int> qualifier = (x, y) => walkability[x, y];
			var parameters = new FloodParameters(new LifoQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier
			};

			var stopwatch = Stopwatch.StartNew();
			new FloodScanlineSpiller().SpillFlood(parameters, result);
			Console.WriteLine("scanline:" + stopwatch.ElapsedMilliseconds + " ms");
			stopwatch.Restart();
			new FloodSpiller().SpillFlood(parameters, result);
			Console.WriteLine("normal:" + stopwatch.ElapsedMilliseconds + " ms");
		}

		[Test]
		public void SpillFlood_CheckboardWalls_ScanlineAndFifoPerformanceComparison()
		{
			int size = 1000;
			var result = new int[size, size];

			var walkability = new bool[size, size];
			// checkboard should be INOPTIMAL for scanline because it skips opening nodes when going vertically
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					if ((x + y) % 2 == 0)
						walkability[x, y] = true;
				}
			}

			var startPosition = new Position(size / 5, size / 5);

			Predicate<int, int> qualifier = (x, y) => walkability[x, y];
			var parameters = new FloodParameters(new LifoQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier
			};

			var stopwatch = Stopwatch.StartNew();
			new FloodScanlineSpiller().SpillFlood(parameters, result);
			Console.WriteLine("scanline:" + stopwatch.ElapsedMilliseconds + " ms");
			stopwatch.Restart();
			new FloodSpiller().SpillFlood(parameters, result);
			Console.WriteLine("normal:" + stopwatch.ElapsedMilliseconds + " ms");
		}

		//[DotMemoryUnit(CollectAllocations = true)]
		[Test]
		public void SpillFlood_HugeOpenArea_ScanlineAndNormalPerformanceComparison()
		{
			int size = 1000;
			var result = new int[size, size];

			var startPosition = new Position(size/2, size/2);
			var queue = new FifoQueue();
			//var queue = new PriorityQueue((p1, p2) => (p1.X + p1.Y).CompareTo((p2.X + p2.Y)));
			var parameters = new FloodParameters(queue, startPosition.X, startPosition.Y);

			var stopwatch = Stopwatch.StartNew();
			new FloodScanlineSpiller().SpillFlood(parameters, result);
			Console.WriteLine("scanline:" + stopwatch.ElapsedMilliseconds);
			stopwatch.Restart();


			//MemoryCheckPoint lastCheck = dotMemory.Check();

			new FloodSpiller().SpillFlood(parameters, result);
			Console.WriteLine("normal:" + stopwatch.ElapsedMilliseconds);
/*
			MemoryCheckPoint checkPoint = dotMemory.Check(memory =>
			{
				var traffic = memory.GetTrafficFrom(lastCheck);
				var difference = memory.GetDifference(lastCheck);

				Console.WriteLine("DIFFERENCE — NEW OBJECTS COUNT;" + difference.GetNewObjects().ObjectsCount);
				Console.WriteLine("DIFFERENCE — NEW OBJECTS SIZE IN BYTES;" + difference.GetNewObjects().SizeInBytes);
				Console.WriteLine("TRAFFIC — ALLOCATED OBJECTS COUNT;" + traffic.AllocatedMemory.ObjectsCount);
				Console.WriteLine("TRAFFIC — ALLOCATED SIZE IN BYTESl" + traffic.AllocatedMemory.SizeInBytes);
				Console.WriteLine("TRAFFIC — COLLECTED OBJECTS COUNT;" + traffic.CollectedMemory.ObjectsCount);
				Console.WriteLine("TRAFFIC — COLLECTED SIZE IN BYTESl" + traffic.CollectedMemory.SizeInBytes);
				Console.WriteLine();
				Console.WriteLine("NEW OBJECTS WITH COUNT OVER 50:");
				Console.WriteLine("OBJECT NAME;COUNT;BYTES");
				var types = difference.GetNewObjects().GroupByType();
				foreach (var tmi in types)
				{
					//if (tmi.ObjectsCount < 50) continue;
					string full = String.Format("{0};{1};{2}", tmi.Type.ToString(), tmi.ObjectsCount,
						tmi.SizeInBytes);
					Console.WriteLine(full);
				}
				Console.WriteLine();
				Console.WriteLine("TRAFFIC OF TYPES WITH COUNT OVER 50:");
				Console.WriteLine("TYPE NAME;ALLOCATED MEMORY;COLLECTED MEMORY");

				var types2 = traffic.GroupByType();
				foreach (var tmi in types2)
				{
					//if (tmi.AllocatedMemoryInfo.ObjectsCount > 50)
					{
						string full = String.Format("{0};{1};{2}", tmi.Type.ToString(), tmi.AllocatedMemoryInfo,
							tmi.CollectedMemoryInfo);
						Console.WriteLine(full);
					}
				}
			});
*/
		}

		[Test]
		public void SpillFlood_HugeAreaWithSomeUnwalkableRegions_PerformanceTest()
		{
			int size = 2000;
			Console.WriteLine("size: " + size);
			var resultForScanline = new int[size, size];
			var resultForNormal = new int[size, size];

			var startPosition = new Position(size/2,size/2); // somewhere around the middle

			var walkability = new bool[size, size];
			int walkableCount = 0;

			int repeatedAreaSize = 20;
			int unwalkableCircleRadius = 8;
			int repeatedAreaCenterX = repeatedAreaSize / 2;
			int repeatedAreaCenterY = repeatedAreaSize / 2;
			// we fill walkability matrix with unwalkable circles
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					int xInRepeteadArea = x % repeatedAreaSize;
					int yInRepeteadArea = y % repeatedAreaSize;
					double toCenter =
						Math.Sqrt(Math.Pow(xInRepeteadArea - repeatedAreaCenterX, 2) + Math.Pow(yInRepeteadArea - repeatedAreaCenterY, 2));

					bool inUnwalkableSquare = toCenter <= unwalkableCircleRadius;
					if (inUnwalkableSquare)
						continue;

					walkability[x, y] = true;
					++walkableCount;
				}
			}

			walkability[startPosition.X, startPosition.Y].Should().BeTrue("it's an initial requirement for test data");
			Console.WriteLine("Walkability ratio: " + ((float)walkableCount) / (size * size));
			Predicate<int, int> qualifier = (x, y) => walkability[x, y];

			var parameters = new FloodParameters(new FifoQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier,
				NeighbourhoodType = NeighbourhoodType.Four
			};

			var stopwatch = Stopwatch.StartNew();
			new FloodScanlineSpiller().SpillFlood(parameters, resultForScanline);
			Console.WriteLine("scanline:" + stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			new FloodSpiller().SpillFlood(parameters, resultForNormal);
			Console.WriteLine("normal:" + stopwatch.ElapsedMilliseconds);
		}

	}
}