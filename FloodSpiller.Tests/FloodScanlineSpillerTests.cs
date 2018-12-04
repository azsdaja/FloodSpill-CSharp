using System;
using FloodSpiller.Queues;
using FloodSpiller.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace FloodSpiller.Tests
{
	[TestFixture]
	public class FloodScanlineSpillerTests
	{
		/// <summary>
		/// Case description:
		/// 
		///	.#..    
		/// .1..	  # - wall
		/// .1..      . - walkable, we don't care about marking number
		/// .0..	  0, 1, 2, 3 - walkable, indicating marking number
		/// .1..	  bottom-left corner position is 0,0
		/// 
		/// </summary>
		[Test]
		public void SpillFlood_ScanlineSpill_CorrectlyPerformsOnSingleHorizontalLine()
		{
			var result = new int[4, 5];
			var startPosition = new Position(1, 1);
			Position onlyUnwalkablePosition = new Position(1, 4);
			Predicate<int, int> qualifier = (x, y) => new Position(x, y) != onlyUnwalkablePosition;

			var parameters = new FloodParameters(new LifoQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier
			};
			new FloodScanlineSpiller().SpillFlood(parameters, result);

			result[1, 0].Should().Be(1);
			result[1, 1].Should().Be(0);
			result[1, 2].Should().Be(1);
			result[1, 3].Should().Be(1);
			result[1, 4].Should().Be(Int32.MaxValue);
		}
	}
}