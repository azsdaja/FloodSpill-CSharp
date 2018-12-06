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
		public void SpillFlood_Neighbourhood4_ReturnsCorrectResultArray()
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
		public void SpillFlood_Neighbourhood8_ReturnsCorrectResultArray()
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
		/// Stop condition is met when next position we pick has smaller height than highest position until now 
		/// (because when water starts to go down, it becomes a river).
		/// </summary>
		[Test]
		public void FloodSpill_LakeSituationWithPriorityQueue_CorrectlySpillsLake()
		{
			var heightMap = new float[5,6];
			string heightInput = "98999" + Environment.NewLine + //.....	   
						         "95799" + Environment.NewLine + //.l...	   l - lake
  /*we start at 2 in this line*/ "92789" + Environment.NewLine + //.l...	   . - land
						         "93499" + Environment.NewLine + //.ll..	   
						         "96999" + Environment.NewLine + //.l...	   
						         "94999";                        //.L...       L - last lake position which will become a new river
						      								     //		       bottom-left corner is 0,0
			string[] inputLines = heightInput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			for (int x = 0; x < inputLines[0].Length; x++)
			{
				for (int y = 0; y < inputLines.Length; y++)
				{
					int lineIndex = inputLines.Length - y - 1;
					heightMap[x, y] = int.Parse(inputLines[lineIndex][x].ToString());
				}
			}

			var markMatrix = new int[5,6];
			Predicate<int, int> qualifier = (x, y) => true; // there are no other constraints apart from stop condition
			Position startingPosition = new Position(1, 3);

			float lakeSurfaceHeight = heightMap[startingPosition.X, startingPosition.Y];
			Predicate<int, int> stopConditionForCurrent = (x, y) => heightMap[x, y] < lakeSurfaceHeight;
			var lakePositions = new List<Position>();
			Action<int, int> processCurrent = (currentX, currentY) =>
			{
				float positionHeight = heightMap[currentX, currentY];
				if (positionHeight > lakeSurfaceHeight)
					lakeSurfaceHeight = positionHeight;

				lakePositions.Add(new Position(currentX, currentY));
			};

			Func<Position, Position, int> comparer =
				(first, second) => heightMap[first.X, first.Y].CompareTo(heightMap[second.X, second.Y]);

			var parameters = new FloodParameters(new PriorityPositionQueue(comparer), startingPosition.X, startingPosition.Y)
			{
				Qualifier = qualifier,
				SpreadingPositionVisitor = processCurrent,
				SpreadingPositionStopCondition = stopConditionForCurrent
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
			foreach (Position expectedLakePosition in expectedLakePositions)
			{
				markMatrix[expectedLakePosition.X, expectedLakePosition.Y].Should().BeLessThan(int.MaxValue);
			}
			lakePositions.Should().BeEquivalentTo(expectedLakePositions);
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