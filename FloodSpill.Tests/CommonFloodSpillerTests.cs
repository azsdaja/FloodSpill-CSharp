using System;
using FloodSpill.Queues;
using FloodSpill.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace FloodSpill.Tests
{
	[TestFixture]
	public class CommonFloodSpillerTests
	{
		private static readonly object[] AllSpillers = {new FloodSpiller(), new FloodScanlineSpiller()};

		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_QueueIsNotEmptyAtBeginning_ThrowsArgumentException(FloodSpiller floodSpiller)
		{
			var parameters = new FloodParameters(123, 456);
			parameters.PositionsToVisitQueue.Enqueue(1, 1);

			Action action = () => floodSpiller.SpillFlood(parameters, new int[10, 10]);

			action.Should().Throw<ArgumentException>();
		}

		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_ResultArrayIsSmallerThanGivenBounds_ThrowsArgumentException(FloodSpiller floodSpiller)
		{
			int boundsSize = 4;
			var bounds = new FloodBounds(-10, -10, boundsSize, boundsSize);
			int[,] result = new int[boundsSize - 1, boundsSize];

			Action action = () =>
			{
				var parameters = new FloodParameters(0, 0)
				{
					BoundsRestriction = bounds
				};
				floodSpiller.SpillFlood(parameters, result);
			};

			action.Should().Throw<ArgumentException>();
		}

		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_BoundsDoNotContainStartingPosition_ThrowsArgumentException(FloodSpiller floodSpiller)
		{
			// arrange
			var startingPoint = new Position(5, 5);
			var bounds = new FloodBounds(2, 2, 2, 2);

			int[,] markMatrix = new int[100, 100];
			Action action = () =>
			{
				var parameters = new FloodParameters(startingPoint.X, startingPoint.Y)
				{
					BoundsRestriction = bounds
				};
				floodSpiller.SpillFlood(parameters, markMatrix);
			};

			action.Should().Throw<ArgumentException>();
		}

		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_NoBoundsAndStartingPositionIsOutOfMarkMatrix_ThrowsArgumentException(FloodSpiller floodSpiller)
		{
			int[,] markMatrix = new int[5, 5]; // max x and y is 4
			var startingPoint = new Position(5, 4);
			Action action = () =>
			{
				var parameters = new FloodParameters(startingPoint.X, startingPoint.Y)
				{
				};
				floodSpiller.SpillFlood(parameters, markMatrix);
			};

			action.Should().Throw<ArgumentException>();
		}

		/// <summary>
		/// Case description:
		/// 
		///	####    
		/// #..#	  # - wall
		/// #..#      . - walkable, marked
		/// #0.#	  0, 1, 2, 3 - walkable, indicating marking number
		/// ####	  bottom-left corner position is 0,0
		/// 
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_CorrectlyPerformsInClosedArea(FloodSpiller floodSpiller)
		{
			var result = new int[4, 5];
			var startPosition = new Position(1, 1);
			Predicate<int, int> qualifierMatchingMiddlePositions = (x, y) => x >= 1 && x <= 2 && y >= 1 && y <= 3;

			var parameters = new FloodParameters(new LifoPositionQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifierMatchingMiddlePositions
			};
			floodSpiller.SpillFlood(parameters, result);

			result[0, 0].Should().Be(Int32.MaxValue);
			result[1, 0].Should().Be(Int32.MaxValue);
			result[2, 0].Should().Be(Int32.MaxValue);
			result[3, 0].Should().Be(Int32.MaxValue);

			result[0, 1].Should().Be(Int32.MaxValue);
			result[3, 1].Should().Be(Int32.MaxValue);

			result[0, 2].Should().Be(Int32.MaxValue);
			result[3, 2].Should().Be(Int32.MaxValue);

			result[0, 3].Should().Be(Int32.MaxValue);
			result[3, 3].Should().Be(Int32.MaxValue);

			result[0, 4].Should().Be(Int32.MaxValue);
			result[1, 4].Should().Be(Int32.MaxValue);
			result[2, 4].Should().Be(Int32.MaxValue);
			result[3, 4].Should().Be(Int32.MaxValue);

			result[1, 1].Should().Be(0);
			result[2, 1].Should().BeLessThan(Int32.MaxValue);
			result[1, 2].Should().BeLessThan(Int32.MaxValue);
			result[2, 2].Should().BeLessThan(Int32.MaxValue);
			result[1, 3].Should().BeLessThan(Int32.MaxValue);
			result[2, 3].Should().BeLessThan(Int32.MaxValue);
		}

		/// <summary>
		/// Case description:
		/// 
		///	x#X#    
		/// #x#X	  # - walkable, unreachable because is not satisfying given qualifier
		/// x#X#      X - walkable, unreached
		/// #0#x	  0, indicating marking number
		/// x#X#	  bottom-left corner position is 0,0
		/// 
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_CheckboardAndUsingFourDirectionsNeighbourHood_PositionsReachableByDiagonalsDoNotGetMarked(FloodSpiller floodSpiller)
		{
			var result = new int[4, 5];
			var startPosition = new Position(1, 1);
			Predicate<int, int> qualifierMatchingCheckboard = (x, y) => (x + y) % 2 == 0;

			var parameters = new FloodParameters(new LifoPositionQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifierMatchingCheckboard,
				NeighbourhoodType = NeighbourhoodType.Four
			};
			floodSpiller.SpillFlood(parameters, result);

			result[1, 1].Should().Be(0);

			for (int x = 0; x < result.GetLength(0); x++)
			{
				for (int y = 0; y < result.GetLength(1); y++)
				{
					if (x != 1 && y != 1)
						result[x, y].Should().Be(Int32.MaxValue);
				}
			}
		}

		/// <summary>
		/// Case description:
		/// 
		///	.#.#    
		/// #.#.	  # - wall
		/// .#.#      . - walkable and marked
		/// #0#.	  0 - walkable, indicating marking number
		/// .#.#	  bottom-left corner position is 0,0
		/// 
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_CheckboardAndUsingEigthDirectionsNeighbourHood_AllNodesReachableByDiagonalsGetMarked(FloodSpiller floodSpiller)
		{
			var result = new int[4, 5];
			var startPosition = new Position(1, 1);
			Predicate<int, int> qualifierMatchingEverySecondPosition = (x, y) =>
													(x + y) % 2 == 0;

			var parameters = new FloodParameters(new LifoPositionQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifierMatchingEverySecondPosition,
				NeighbourhoodType = NeighbourhoodType.Eight
			};
			floodSpiller.SpillFlood(parameters, result);

			result[startPosition.X, startPosition.Y].Should().Be(0);
			for (int x = 0; x < 4; x++)
			{
				for (int y = 0; y < 5; y++)
				{
					bool checkboardWalkable = (x + y)%2 == 0;
					if (checkboardWalkable)
						result[x, y].Should().BeLessThan(Int32.MaxValue);
					else
						result[x, y].Should().Be(Int32.MaxValue);
				}
			}
		}

		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_BoundsDoNotStartFromZero_LambdasAreCalledForCorrectPosition(FloodSpiller floodSpiller)
		{
			int startX = -5;
			int startY = 5;
			var bounds = new FloodBounds(startX, startY, 1, 1);
			int[,] markMatrix = new int[1,1];
			bool qualifierCalled = false;
			bool neighbourStopConditionCalled = false;
			bool neighbourProcessorCalled = false;
			bool spreadingPositionStopConditionCalled = false;
			bool spreadingPositionVisitorCalled = false;
			var parametersCheckingIfLambdasAreExecutedForStartingPosition = new FloodParameters(startX, startY)
			{
				BoundsRestriction = bounds,
				ProcessStartAsFirstNeighbour = true, // make first position be both processed as a neighbour and as a visited spreading position.
				Qualifier = (x, y) =>
				{
					if (x == startX && y == startY) qualifierCalled = true;
					return true;
				},
				NeighbourStopCondition = (x, y) =>
				{
					if (x == startX && y == startY) neighbourStopConditionCalled = true;
					return false;
				},
				NeighbourProcessor = (x, y, mark) =>
				{
					if (x == startX && y == startY) neighbourProcessorCalled = true;
				},
				SpreadingPositionStopCondition = (x, y) =>
				{
					if (x == startX && y == startY) spreadingPositionStopConditionCalled = true;
					return false;
				},
				SpreadingPositionVisitor = (x, y) =>
				{
					if (x == startX && y == startY) spreadingPositionVisitorCalled = true;
				}
			};

			floodSpiller.SpillFlood(parametersCheckingIfLambdasAreExecutedForStartingPosition, markMatrix);

			qualifierCalled.Should().BeTrue();
			neighbourStopConditionCalled.Should().BeTrue();
			neighbourProcessorCalled.Should().BeTrue();
			spreadingPositionStopConditionCalled.Should().BeTrue();
			spreadingPositionVisitorCalled.Should().BeTrue();
		}

		/// <summary>
		/// Case description:
		/// 
		/// ##   # - unreachable because doesn't satisfy given qualifier
		/// 01	 0, 1 - reachable, indicates mark number
		/// 	 bottom-left corner is -10,-10
		/// 
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_BoundsDoNotStartFromZero_ReturnsCorrectResultRespectingBounds(FloodSpiller floodSpiller)
		{
			// arrange
			int resultArraySize = 4;
			var startingPoint = new Position(-5, -5);
			var bounds = new FloodBounds(-5, -5, 2, 2);

			int[,] markMatrix = new int[resultArraySize, resultArraySize];
			var parameters = new FloodParameters(startingPoint.X, startingPoint.Y)
			{
				Qualifier = (x, y) => y == -5,
				BoundsRestriction = bounds
			};

			// act
			floodSpiller.SpillFlood(parameters, markMatrix);

			Console.WriteLine(MarkMatrixVisualiser.Visualise(markMatrix));

			// assert that reached positions got correct numbers
			markMatrix[0, 0].Should().Be(0);
			markMatrix[1, 0].Should().Be(1);
			markMatrix[2, 0].Should().Be(int.MaxValue); // out of bounds
			markMatrix[0, 1].Should().Be(int.MaxValue); // unreachable
			markMatrix[1, 1].Should().Be(int.MaxValue); // unreachable
		}

		/// <summary>
		///	     
		/// ...	  . - reachable, but not reached due to meeting stop condition when visiting first node
		/// .0.   0 - reached; indicates mark number
		/// ...	  bottom-left corner is 0,0
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_FirstVisitedPositionMeetsStopCondition_AlgorithmStopsBeforeEvenSpreadingToNeighbours(FloodSpiller spiller)
		{
			// arrange
			var startingPosition = new Position(1, 1);

			int neighboursProcessed = 0;

			Predicate<int, int> qualifier = (x, y) => true;
			Predicate<int, int> spreadingStopCondition = (x, y) => true;
			Action<int, int, int> neighbourProcessor = (x, y, mark) => ++neighboursProcessed;

			// act
			int[,] result = new int[3, 3];
			var parameters = new FloodParameters(startingPosition.X, startingPosition.Y)
			{
				Qualifier = qualifier,
				SpreadingPositionStopCondition = spreadingStopCondition,
				NeighbourProcessor = neighbourProcessor
			};
			bool wasStopped = spiller.SpillFlood(parameters, result);

			wasStopped.Should().BeTrue();
			neighboursProcessed.Should().Be(0);
			// assert that visited positions got correct numbers
			result[0, 0].Should().Be(int.MaxValue);
			result[1, 0].Should().Be(int.MaxValue);
			result[2, 0].Should().Be(int.MaxValue);

			result[0, 1].Should().Be(int.MaxValue);
			result[1, 1].Should().Be(0);
			result[2, 1].Should().Be(int.MaxValue);

			result[0, 2].Should().Be(int.MaxValue);
			result[1, 2].Should().Be(int.MaxValue);
			result[2, 2].Should().Be(int.MaxValue);
		}

		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_NoStopConditionIsMet_ReturnsFalse(FloodSpiller spiller)
		{
			int[,] result = new int[3, 3];

			var parameters = new FloodParameters(0, 0);
			bool wasStopped = spiller.SpillFlood(parameters, result);

			wasStopped.Should().BeFalse();
		}

		/// <summary>
		///	???.      ? - reachable; whether it got reached or not depends on implementation
		/// ???.	  . - reachable, but not reached due to meeting stop condition 
		/// ?0?.         (both normal and scanline implementation don't reach that far horizontally)
		/// ???.      0 - reached; indicates marking number
		/// 	      bottom-left corner is position 0,0
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_FirstProcessedNeighbourMeetsStopCondition_AlgorithmStopsAfterProcessingAllNeighbours(FloodSpiller spiller)
		{
			// arrange
			var startingPoint = new Position(1, 1);
			int visitedPositionsCount = 0;
			int processedNeighbourCount = 0;
			Predicate<int, int> qualifier = (x, y) => true;
			Predicate<int, int> neighbourStopCondition = (x, y) => true;
			Action<int, int, int> neighbourProcessor = (x, y, mark) => ++processedNeighbourCount;
			Action<int, int> spreadingPositionVisitor = (x, y) => ++visitedPositionsCount;

			// act
			int[,] result = new int[4, 4];
			var parameters = new FloodParameters(startingPoint.X, startingPoint.Y)
			{
				Qualifier = qualifier,
				NeighbourProcessor = neighbourProcessor,
				NeighbourStopCondition = neighbourStopCondition,
				SpreadingPositionVisitor = spreadingPositionVisitor
			};
			bool wasStopped = spiller.SpillFlood(parameters, result);

			wasStopped.Should().BeTrue();
			visitedPositionsCount.Should().Be(1); // only starting position should be visited
												  // assert that reached positions got correct numbers
			processedNeighbourCount.Should().BeGreaterThan(1); // it depends on implementation how many neighbours we reach,
															   // but for sure there should be more than one 
															  // even if first processed neighbour caused stop
		}

		/// <summary>
		/// 
		/// ~~~~   ~ - expected sea
		/// ~###   # - expected non-sea (too high)
		/// ~##.   . - expected non-sea (it's low enough, but is separated from the sea)
		/// ~~##   bottom-left corner is 0,0
		/// 
		/// </summary>
		[TestCaseSource(nameof(AllSpillers))]
		public void SpillFlood_FloodDoesNotReachPositionsThatAreValidButBlockedByUnReachablePositions(FloodSpiller spiller)
		{
			float seaLevel = 0.5f;
			var heights = new float[4, 4];
			var positionsToSetUnderThreshold = new[]
			{
				new Position(0,0), new Position(1, 0),
				new Position(0,1), new Position(3,1),
				new Position(0,2),
				new Position(0,3), new Position(1,3), new Position(2,0), new Position(3,3)
			};
			var positionsToSetAboveThreshold = new[]
			{
				new Position(2, 0), new Position(3, 0),
				new Position(1, 1), new Position(2, 1),
				new Position(1, 2), new Position(2, 2), new Position(3, 2)
			};
			foreach (Position positionUnder in positionsToSetUnderThreshold)
			{
				heights[positionUnder.X, positionUnder.Y] = 0f;
			}
			foreach (Position positionAbove in positionsToSetAboveThreshold)
			{
				heights[positionAbove.X, positionAbove.Y] = 1f;
			}

			Predicate<int, int> potentialSeaQualifier = (x, y) => heights[x, y] < seaLevel;

			var result = new int[4, 4];
			var parameters = new FloodParameters(0, 0)
			{
				Qualifier = potentialSeaQualifier
			};
			spiller.SpillFlood(parameters, result);

			result[0, 0].Should().Be(0); // starting position
			result[1, 0].Should().BeLessThan(int.MaxValue);
			result[0, 1].Should().BeLessThan(int.MaxValue);
			result[0, 2].Should().BeLessThan(int.MaxValue);
			result[0, 3].Should().BeLessThan(int.MaxValue);
			result[1, 3].Should().BeLessThan(int.MaxValue);
			result[2, 3].Should().BeLessThan(int.MaxValue);
			result[3, 3].Should().BeLessThan(int.MaxValue);
			result[1, 1].Should().Be(int.MaxValue); // above threshold
			result[2, 2].Should().Be(int.MaxValue); // above threshold
			result[3, 1].Should().Be(int.MaxValue); // under threshold, unreachable
		}


		[Test]
		public void FloodSpillerAndFloodScanlineSpiller_BigAreaWithSomeUnwalkableRegions_BothSpillersMarkSamePositions()
		{
			// arrange

			int size = 200;
			Console.WriteLine("size: " + size);
			var resultForScanline = new int[size, size];
			var resultForNormal = new int[size, size];

			var startPosition = new Position((int)(size / 2), size / 2); // somewhere around the middle

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
			var parameters = new FloodParameters(new FifoPositionQueue(), startPosition.X, startPosition.Y)
			{
				Qualifier = qualifier,
				NeighbourhoodType = NeighbourhoodType.Four
			};

			// act
			new FloodScanlineSpiller().SpillFlood(parameters, resultForScanline);
			new FloodSpiller().SpillFlood(parameters, resultForNormal);

			// assert
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					(resultForScanline[x, y] == int.MaxValue).Should().Be(resultForNormal[x, y] == int.MaxValue);
				}
			}
		}
	}
}