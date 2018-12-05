using System;
using System.Linq;
using FloodSpiller.PositionVisitors;
using FluentAssertions;
using NUnit.Framework;

namespace FloodSpiller.Tests.PositionVisitors
{
	public class AddToListPositionVisitorTests
	{
		private static readonly object[] AllSpillers = {new FloodSpiller(), new FloodScanlineSpiller(),};

		[Test]
		public void Visit_FloodSpillerAnd3x3OpenArea_All9PositionsAreAddedToResultAndFirstElementIsStartingPosition()
		{
			var startingPosition = new Position(1, 1);

			int[,] matrix = new int[3, 3];
			int positionsCount = matrix.Length;
			var addToListProcessor = new AddToListPositionVisitor();
			Action<int, int> positionVisitor = addToListProcessor.Visit;
			var parameters = new FloodParameters(startingPosition.X, startingPosition.Y)
			{
				ProcessStartAsFirstNeighbour = true,
				SpreadingPositionVisitor = positionVisitor
			};
			FloodSpiller spiller = new FloodSpiller();
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

		[Test]
		public void Visit_FloodScanlineSpillerAnd3x3OpenArea_SomePositionsAreAddedToResultAndFirstElementIsStartingPosition()
		{
			var startingPosition = new Position(1, 1);

			int[,] matrix = new int[3, 3];
			var addToListProcessor = new AddToListPositionVisitor();
			Action<int, int> positionVisitor = addToListProcessor.Visit;
			var parameters = new FloodParameters(startingPosition.X, startingPosition.Y)
			{
				ProcessStartAsFirstNeighbour = true,
				SpreadingPositionVisitor = positionVisitor
			};
			FloodSpiller spiller = new FloodSpiller();
			spiller.SpillFlood(parameters, matrix);

			// scanline doesn't visit all processed neigbhours, so we can only assume that at least one position will be visited
			addToListProcessor.Result.Count.Should().BeGreaterOrEqualTo(1); 
			addToListProcessor.Result.First().Should().Be(startingPosition);
		}
	}
}