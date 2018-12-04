using FloodSpiller.NeighbourProcessors;
using FloodSpiller.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace FloodSpiller.Tests
{
	[TestFixture]
	public class FindHighestMarkNeighbourProcessorTests
	{
		[Test]
		public void Process_NoNeighbours_ReturnsMinPositionAndZeroMark()
		{
			var startingPosition = new Position(0, 0);
			int[,] result = new int[1, 1];
			var findHighestMark = new FindHighestMarkNeighbourProcessor();
			var parameters = new FloodParameters(startingPosition.X, startingPosition.Y)
			{
				NeighbourProcessor = findHighestMark.Process
			};

			new FloodSpiller().SpillFlood(parameters, result);

			findHighestMark.PositionWithHighestMark.Should().Be(Position.MinValue);
			findHighestMark.HighestMark.Should().Be(0);
		}

		/// <summary>
		/// 2##			 # - unreachable
		/// 111			 0, 1, 2 - reached; number indicates given mark
		/// 101			 bottom-left corner is 0,0
		/// 111
		/// </summary>
		[Test]
		public void Process_FloodIsSpilledAround_ReturnsLastProcessedNeighbourWithMarkHigherThanOthers()
		{
			var startingPosition = new Position(1, 1);
			int[,] result = new int[4, 4];
			Predicate<int, int> qualifier = (x, y) => y != 3 || x == 0;
			var findHighestMark = new FindHighestMarkNeighbourProcessor();
			var parameters = new FloodParameters(startingPosition.X, startingPosition.Y)
			{
				Qualifier = qualifier,
				NeighbourProcessor = findHighestMark.Process
			};

			new FloodSpiller().SpillFlood(parameters, result);

			var expectedPosition = new Position(0, 3);
			int expectedHighestMark = 2;
			findHighestMark.PositionWithHighestMark.Should().Be(expectedPosition);
			findHighestMark.HighestMark.Should().Be(expectedHighestMark);
		}
	}
}