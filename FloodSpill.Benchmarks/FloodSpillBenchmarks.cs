using System;
using BenchmarkDotNet.Attributes;
using FloodSpill.Queues;
using FloodSpill.Utilities;

namespace FloodSpill.Benchmarks
{
	[ShortRunJob, MarkdownExporter, AsciiDocExporter, HtmlExporter, CsvExporter, RPlotExporter, RankColumn, MemoryDiagnoser]
	public class FloodSpillBenchmarks
	{
		public enum AreaType
		{
			Open, // no walls - flood can spill freely
			WithUnwalkableCircles, // area filled with circles blocking flood (taking around half of whole area)
			WithSparseUnwalkablePillars // area filled with pillars blocking flood (taking one ninth of whole area) 
		}

		private bool[,] _wallMatrix;
		private int[,] _markMatrix;
		private Predicate<int, int> _qualifier;
		private FloodBounds _boundsRestriction;
		private FloodParameters _parameters;
		private FloodSpiller _floodSpiller;

		[GlobalSetup]
		public void Setup()
		{
			_markMatrix = new int[AreaSize,AreaSize];

			switch (ChosenAreaType)
			{
				case AreaType.Open:
					_wallMatrix = new bool[AreaSize, AreaSize]; break;

				case AreaType.WithUnwalkableCircles:
					_wallMatrix = CreateMatrixWithCircles(AreaSize); break;

				case AreaType.WithSparseUnwalkablePillars:
				default:
					_wallMatrix = CreateMatrixWithPillars(AreaSize); break;
			}

			_boundsRestriction = new FloodBounds(AreaSize, AreaSize);
			_qualifier = (x, y) => !_wallMatrix[x, y];

			int startX = AreaSize == 20 ? 0 : AreaSize / 2; // gives a non-wall position for area sizes 20, 200 and 2000.
			int startY = startX;
			if (_wallMatrix[startX, startY])
			{
				throw new InvalidOperationException($"{startX}, {startY} is a wall. You shouldn't start benchmark from a wall.");
			}
			_parameters = new FloodParameters(startX, startY)
			{
				BoundsRestriction = _boundsRestriction,
				Qualifier = _qualifier,
			};

			_floodSpiller = UsingScanline ? new FloodScanlineSpiller() : new FloodSpiller();
		}

		// when modifying, make sure to update RunFloodFill to start at non-wall position!
		[Params(20, 200, 2000)]
		public int AreaSize;

		[Params(AreaType.Open, AreaType.WithUnwalkableCircles, AreaType.WithSparseUnwalkablePillars)]
		public AreaType ChosenAreaType;

		[Params(true, false)]
		public bool UsingScanline;

		[Benchmark, MemoryDiagnoser]
		public void RunFloodFill()
		{
			_parameters.PositionsToVisitQueue = new FifoPositionQueue();
			_floodSpiller.SpillFlood(_parameters, _markMatrix);
		}

		private bool[,] CreateMatrixWithCircles(int size)
		{
			var wallMatrix = new bool[size, size];
			int repeatedAreaSize = 20;
			int wallCircleRadius = 8;
			int repeatedAreaCenterX = repeatedAreaSize / 2;
			int repeatedAreaCenterY = repeatedAreaSize / 2;
			// we fill the matrix with circles made of walls
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					int xInRepeteadArea = x % repeatedAreaSize;
					int yInRepeteadArea = y % repeatedAreaSize;
					double toCenter =
						Math.Sqrt(Math.Pow(xInRepeteadArea - repeatedAreaCenterX, 2) + Math.Pow(yInRepeteadArea - repeatedAreaCenterY, 2));

					bool isWall = toCenter <= wallCircleRadius;
					if (isWall)
					{
						wallMatrix[x, y] = true;
					}
				}
			}

			return wallMatrix;
		}

		private bool[,] CreateMatrixWithPillars(int areaMaxSize)
		{
			var wallMatrix = new bool[areaMaxSize, areaMaxSize];

			for (int x = 0; x < areaMaxSize; x++)
			{
				for (int y = 0; y < areaMaxSize; y++)
				{
					bool isPillar = (x % 3 == 2) && (y % 3 == 2);
					wallMatrix[x, y] = isPillar;
				}
			}
			return wallMatrix;
		}
	}
}
