﻿using System;
using System.Diagnostics;
using FloodSpill.Queues;
using FloodSpill.Utilities;
using NUnit.Framework;

namespace FloodSpill.Benchmarks
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
			var parameters = new FloodParameters(new LifoPositionQueue(), startPosition.X, startPosition.Y)
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
			var parameters = new FloodParameters(new LifoPositionQueue(), startPosition.X, startPosition.Y)
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
			var parameters = new FloodParameters(new LifoPositionQueue(), startPosition.X, startPosition.Y)
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
		public void SpillFlood_HugeOpenArea_ScanlineAndNormalPerformanceComparison()
		{
			int size = 1000;
			var result = new int[size, size];

			var startPosition = new Position(size/2, size/2);
			var queue = new FifoPositionQueue();
			var parameters = new FloodParameters(queue, startPosition.X, startPosition.Y);

			var stopwatch = Stopwatch.StartNew();
			new FloodScanlineSpiller().SpillFlood(parameters, result);
			Console.WriteLine("scanline:" + stopwatch.ElapsedMilliseconds);
			stopwatch.Restart();


			new FloodSpiller().SpillFlood(parameters, result);
			Console.WriteLine("normal:" + stopwatch.ElapsedMilliseconds);
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

			Assert.True(walkability[startPosition.X, startPosition.Y], "it's an initial requirement for test data");
			Console.WriteLine("Walkability ratio: " + ((float)walkableCount) / (size * size));
			Predicate<int, int> qualifier = (x, y) => walkability[x, y];

			var parameters = new FloodParameters(new FifoPositionQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier,
				NeighbourhoodType = NeighbourhoodType.Four
			};

			Console.WriteLine("time:");

			var stopwatch = Stopwatch.StartNew();
			var normal = new FloodSpiller();
			for (int i = 0; i < 5; i++)
			{
				normal.SpillFlood(parameters, resultForNormal);
			}
			Console.WriteLine("normal------------:" + stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			var scanline = new FloodScanlineSpiller();
			for (int i = 0; i < 5; i++)
			{
				scanline.SpillFlood(parameters, resultForScanline);
			}
			Console.WriteLine("scanline------------:" + stopwatch.ElapsedMilliseconds);
		}
	}
}