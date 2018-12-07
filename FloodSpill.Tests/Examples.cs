using System;
using FloodSpill.Queues;
using FloodSpill.Utilities;
using NUnit.Framework;

namespace FloodSpill.Tests
{
	/// <summary>
	/// "Learn by examples" section at https://github.com/azsdaja/FloodSpill-CSharp/wiki/Home should match this class content.
	/// </summary>
	[TestFixture]
	public class Examples
	{
		[Test]
		public void NoWalls_Fifo()
		{
			var markMatrix = new int[10, 5];
			var floodParameters = new FloodParameters(startX: 1, startY: 1);

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void NoWalls_Lifo()
		{
			var markMatrix = new int[10, 5];
			var floodParameters = new FloodParameters(new LifoPositionQueue(), 1, 1);

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void NoWalls_PriorityQueue()
		{
			var markMatrix = new int[10, 10];
			Position center = new Position(5, 5);
			Func<Position, Position, int> distanceToCenterComparer = // favours positions closer to center
				(first, second) => Position.Distance(center, first).CompareTo(Position.Distance(center, second));
			var priorityQueue = new PriorityPositionQueue(distanceToCenterComparer);
			var floodParameters = new FloodParameters(priorityQueue, 1, 1);

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void Scanline_NoWalls_Fifo()
		{
			var markMatrix = new int[8, 10];
			var floodParameters = new FloodParameters(4, 2);

			new FloodScanlineSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void DisallowDiagonalNeighbourhood()
		{
			var markMatrix = new int[10, 5];
			var floodParameters = new FloodParameters(1, 1)
			{
				NeighbourhoodType = NeighbourhoodType.Four
			};

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void Bounds()
		{
			var markMatrix = new int[10, 5];
			var floodParameters = new FloodParameters(7, 7)
			{
				BoundsRestriction = new FloodBounds(minX: 5, minY: 5, sizeX: 5, sizeY: 3)
				// markMatrix will be accessed with offset, so that we don't have IndexOutOfRangeExceptions
			};

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void NegativeBounds()
		{
			var markMatrix = new int[10, 5];
			var floodParameters = new FloodParameters(-99, -99)
			{
				BoundsRestriction = new FloodBounds(minX: -100, minY: -100, sizeX: 10, sizeY: 5)
			};

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void SomeWalls()
		{
			var wallMatrix = new bool[6, 5];
			wallMatrix[2, 0] = wallMatrix[2, 1] = wallMatrix[2, 2] 
				= wallMatrix[3, 0] = wallMatrix[3, 1] = wallMatrix[3, 2] = true;

			Predicate<int, int> positionQualifier = (x, y) => wallMatrix[x, y] == false;

			var floodParameters = new FloodParameters(startX: 0, startY: 0)
			{
				Qualifier = positionQualifier
			};
			var markMatrix = new int[6, 5];

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void CallbacksForProcessingNeighboursAndVisitingPositions()
		{
			var markMatrix = new int[3,2];
			Action<int, int, int> neighbourProcessor = 
				(x, y, mark) => Console.WriteLine($"Processing {x}, {y} as a neighbour with {mark} mark.");
			Action<int, int> positionVisitor = 
				(x, y) => Console.WriteLine($"Visiting {x}, {y}");
			var floodParameters = new FloodParameters(0, 0)
			{
				NeighbourProcessor = neighbourProcessor,
				SpreadingPositionVisitor = positionVisitor
			};

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);
		}

		[Test]
		public void Scanline_CallbacksForProcessingNeighboursAndVisitingPositions()
		{
			var markMatrix = new int[3,5];
			Action<int, int, int> neighbourProcessor = 
				(x, y, mark) => Console.WriteLine($"Processing {x}, {y} as a neighbour with {mark} mark.");
			Action<int, int> positionVisitor = 
				(x, y) => Console.WriteLine($"Visiting {x}, {y}");
			var floodParameters = new FloodParameters(1, 1)
			{
				NeighbourProcessor = neighbourProcessor,
				SpreadingPositionVisitor = positionVisitor
			};

			new FloodScanlineSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void NeighbourStopCondition()
		{
			var markMatrix = new int[10, 5];
			Action<int, int, int> neighbourProcessor =
				(x, y, mark) => Console.WriteLine($"Processing {x}, {y} as a neighbour with {mark} mark.");
			Action<int, int> positionVisitor =
				(x, y) => Console.WriteLine($"Visiting {x}, {y}");
			Predicate<int, int> neighbourStopCondition = (x, y) =>
			{
				if (x > 1)
				{
					Console.WriteLine($"{x}, {y} causing stop!");
					return true;
				}
				return false;
			};
			var floodParameters = new FloodParameters(0, 0)
			{
				NeighbourProcessor = neighbourProcessor,
				SpreadingPositionVisitor = positionVisitor,
				NeighbourStopCondition = neighbourStopCondition
			};

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}

		[Test]
		public void SpreadingPositionStopCondition()
		{
			var markMatrix = new int[10, 5];
			Action<int, int, int> neighbourProcessor =
				(x, y, mark) => Console.WriteLine($"Processing {x}, {y} as a neighbour with {mark} mark.");
			Action<int, int> positionVisitor =
				(x, y) => Console.WriteLine($"Visiting {x}, {y}");
			Predicate<int, int> spreadingPositionStopCondition = (x, y) =>
			{
				if (x > 1)
				{
					Console.WriteLine($"{x}, {y} causing stop!");
					return true;
				}
				return false;
			};
			var floodParameters = new FloodParameters(0, 0)
			{
				NeighbourProcessor = neighbourProcessor,
				SpreadingPositionVisitor = positionVisitor,
				SpreadingPositionStopCondition = spreadingPositionStopCondition
			};

			new FloodSpiller().SpillFlood(floodParameters, markMatrix);

			string representation = MarkMatrixVisualiser.Visualise(markMatrix);
			Console.WriteLine(representation);
		}
	}
}