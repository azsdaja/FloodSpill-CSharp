using System;
using System.Linq;
using FloodSpiller.NeighbourProcessors;
using FluentAssertions;
using NUnit.Framework;

namespace FloodSpiller.Tests
{
	public class AddToListPositionProcessorTests
	{
		private static readonly object[] AllSpillers = {new FloodSpiller(), new FloodScanlineSpiller(),};

		[TestCaseSource(nameof(AllSpillers))]
		public void Visit_3x3OpenArea_All9PositionsAreAddedToResultAndFirstElementIsStartingPosition(FloodSpiller spiller)
		{
			var startingPosition = new Position(1, 1);

			int[,] matrix = new int[3, 3];
			int positionsCount = matrix.Length;
			var addToListProcessor = new AddToListNeighbourProcessor();
			Action<int, int, int> neighbourProcessor = addToListProcessor.Process;
			var parameters = new FloodParameters(startingPosition.X, startingPosition.Y)
			{
				ProcessStartAsFirstNeighbour = true,
				NeighbourProcessor = neighbourProcessor
			};

			spiller.SpillFlood(parameters, matrix);

			addToListProcessor.Result.Count.Should().Be(positionsCount);
			addToListProcessor.Result.First().Should().Be(startingPosition);
			for (int x = 0; x < matrix.GetLength(0); x++)
			{
				for (int y = 0; y < matrix.GetLength(1); y++)
				{
					addToListProcessor.Result.Should().Contain(new Position(x, y));
				}
			}
		}
	}
}