using System;
using System.Collections.Generic;
using FloodSpill.Queues;
using FloodSpill.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace FloodSpill.Tests
{
	[TestFixture]
	public class FloodSpillerTests
	{
		/// <summary>
		/// Case description:
		/// 
		///	##4#      # - wall
		/// ##3#	  . - reachable, but out of range
		/// 012#      0, 1, 2, 3 - reachable, indicating number of steps from source
		/// ####	  bottom-left corner is 0,0
		/// 
		/// </summary>
		[Test]
		public void SpillFlood_NoDiagonalNeighbourhood_ReturnsCorrectResultArray()
		{
			// arrange
			int size = 4;
			var center = new Position(0, 1);

			bool[,] walkabilityArray = new bool[size, size];
			walkabilityArray[0, 1] = true;
			walkabilityArray[1, 1] = true;
			walkabilityArray[2, 1] = true;
			walkabilityArray[2, 2] = true;
			walkabilityArray[2, 3] = true;

			Predicate<int, int> qualifier = (x, y) => walkabilityArray[x, y];

			// act
			var parameters = new FloodParameters(center.X, center.Y)
			{
				Qualifier = qualifier,
				NeighbourhoodType = NeighbourhoodType.Four,
			};
			int[,] result = new int[size, size];
			new FloodSpiller().SpillFlood(parameters, result);

			// assert that reached positions got correct numbers
			result[0, 1].Should().Be(0);
			result[1, 1].Should().Be(1);
			result[2, 1].Should().Be(2);
			result[2, 2].Should().Be(3);
			result[2, 3].Should().Be(4);

			// assert that unvisited positions didn't get numbers
			result[0, 0].Should().Be(int.MaxValue);
			result[1, 0].Should().Be(int.MaxValue);
			result[2, 0].Should().Be(int.MaxValue);
			result[3, 0].Should().Be(int.MaxValue);

			result[3, 1].Should().Be(int.MaxValue);

			result[0, 2].Should().Be(int.MaxValue);
			result[1, 2].Should().Be(int.MaxValue);
			result[3, 2].Should().Be(int.MaxValue);

			result[0, 3].Should().Be(int.MaxValue);
			result[1, 3].Should().Be(int.MaxValue);
			result[3, 3].Should().Be(int.MaxValue);
		}

		/// <summary>
		/// Case description:
		/// 
		///	##3#      # - wall
		/// ##2#	  . - reachable, but out of range
		/// 012#      0, 1, 2, 3 - reachable, indicating number of steps from source
		/// ####	  bottom-left corner is 0,0
		/// 
		/// </summary>
		[Test]
		public void SpillFlood_DiagonalNeighbourhoodAllowed_ReturnsCorrectResultArray()
		{
			// arrange
			int size = 4;
			var center = new Position(0, 1);

			bool[,] walkabilityArray = new bool[size, size];
			walkabilityArray[0, 1] = true;
			walkabilityArray[1, 1] = true;
			walkabilityArray[2, 1] = true;
			walkabilityArray[2, 2] = true;
			walkabilityArray[2, 3] = true;

			Predicate<int, int> qualifier = (x, y) => walkabilityArray[x, y];
			int[,] result = new int[size, size];

			var parameters = new FloodParameters(center.X, center.Y)
			{
				Qualifier = qualifier,
				NeighbourhoodType = NeighbourhoodType.Eight,
			};

			// act
			new FloodSpiller().SpillFlood(parameters, result);

			// assert that reached positions got correct numbers
			result[0, 1].Should().Be(0);
			result[1, 1].Should().Be(1);
			result[2, 1].Should().Be(2);
			result[2, 2].Should().Be(2);
			result[2, 3].Should().Be(3);

			// assert that unvisited positions didn't get marks
			result[0, 0].Should().Be(int.MaxValue);
			result[1, 0].Should().Be(int.MaxValue);
			result[2, 0].Should().Be(int.MaxValue);
			result[3, 0].Should().Be(int.MaxValue);

			result[3, 1].Should().Be(int.MaxValue);

			result[0, 2].Should().Be(int.MaxValue);
			result[1, 2].Should().Be(int.MaxValue);

			result[3, 2].Should().Be(int.MaxValue);

			result[0, 3].Should().Be(int.MaxValue);
			result[1, 3].Should().Be(int.MaxValue);
			result[3, 3].Should().Be(int.MaxValue);
		}

		/// <summary>
		/// "Lake" should be created by joining positions with smallest height among positions adjacent to our current lake.
		/// Stop condition is met when next position we pick has smaller height than the highest position until now 
		/// (because when water starts to go down, it becomes a river).
		/// </summary>
		[Test] 
		public void FloodSpill_LakeSituationWithPriorityQueue_CorrectlySpillsLake()
		{
			string heightInput = "98999" + Environment.NewLine + //.....	   
						         "95799" + Environment.NewLine + //.l...	   l - expected lake
/*start position is at 2 --> */  "92789" + Environment.NewLine + //.l...	   . - expected land
								 "93499" + Environment.NewLine + //.ll..	   
						         "96999" + Environment.NewLine + //.l...	   
						         "94999";                        //.L...       L - last lake position which will become a new river
						      								     //		       bottom-left corner is 0,0
																 // water should spill to lowest positions adjacent to flood: 2, 3, 4, 5, 6, 4
			float[,] heightMap = CreateHeightMapFromString(heightInput);

			var markMatrix = new int[5,6];
			Position startingPosition = new Position(1, 3);

			float lakeSurfaceHeight = heightMap[startingPosition.X, startingPosition.Y];
			Predicate<int, int> stopConditionForVisited = (x, y) => heightMap[x, y] < lakeSurfaceHeight;
			var lakePositions = new List<Position>();
			Action<int, int> positionVisitorWithSurfaceAdjustmentAndListBuilding = (currentX, currentY) =>
			{
				float positionHeight = heightMap[currentX, currentY];
				if (positionHeight > lakeSurfaceHeight)
					lakeSurfaceHeight = positionHeight;

				lakePositions.Add(new Position(currentX, currentY));
			};

			Func<Position, Position, int> positionComparerByHeight =
				(first, second) => heightMap[first.X, first.Y].CompareTo(heightMap[second.X, second.Y]);

			var parameters = new FloodParameters(new PriorityPositionQueue(positionComparerByHeight), startingPosition.X, startingPosition.Y)
			{
				SpreadingPositionVisitor = positionVisitorWithSurfaceAdjustmentAndListBuilding,
				SpreadingPositionStopCondition = stopConditionForVisited
			};

			// act
			new FloodSpiller().SpillFlood(parameters, markMatrix);

			// assert lakePositions are calculated correctly and have proper marks
			Position[] expectedLakePositions =
			{
				new Position(1,0),
				new Position(1,1),
				new Position(1,2),
				new Position(2,2),
				new Position(1,3),
				new Position(1,4),
			};
			lakePositions.Should().BeEquivalentTo(expectedLakePositions);
			Console.WriteLine(MarkMatrixVisualiser.Visualise(markMatrix));
		}

		private static float[,] CreateHeightMapFromString(string heightInput)
		{
			var heightMap = new float[5,6];
			string[] inputLines = heightInput.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			for (int x = 0; x < inputLines[0].Length; x++)
			{
				for (int y = 0; y < inputLines.Length; y++)
				{
					int lineIndex = inputLines.Length - y - 1;
					heightMap[x, y] = int.Parse(inputLines[lineIndex][x].ToString());
				}
			}

			return heightMap;
		}

		[Test]
		public void SpillFlood_FirstVisitedPositionMeetsStopCondition_ItIsVisitedBeforeStoppingAlgorithm()
		{
			var startingPoint = new Position(1, 1);

			Predicate<int, int> qualifier = (x, y) => true;
			Predicate<int, int> spreadingPositionStopCondition = (x, y) => true;

			int[,] result = new int[3, 3];
			bool wasVisited = false;
			Action<int, int> positionVisitor = (x, y) => wasVisited = true;

			var parameters = new FloodParameters(startingPoint.X, startingPoint.Y)
			{
				Qualifier = qualifier,
				SpreadingPositionStopCondition = spreadingPositionStopCondition,
				SpreadingPositionVisitor = positionVisitor
			};
			new FloodSpiller().SpillFlood(parameters, result);

			wasVisited.Should().BeTrue();
		}
	}
}