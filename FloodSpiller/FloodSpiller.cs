using System;
using FloodSpiller.Queues;
using FloodSpiller.Utilities;

namespace FloodSpiller
{
	/// <summary>
	/// Realisation of flood-spill algorithm.
	/// </summary>
	public class FloodSpiller
	{
		protected PositionQueueAdapter PositionsToVisit;
		protected int StartX;
		protected int StartY;
		protected bool ProcessStartAsFirstNeighbour;
		protected int[,] MarkMatrix;
		protected Predicate<int, int> Qualifier;
		protected NeighbourhoodType NeighbourhoodType;
		protected Action<int, int, int> NeighbourProcessor;
		protected Predicate<int, int> NeighbourStopCondition;
		protected Action<int, int> SpreadingPositionVisitor;
		protected Predicate<int, int> SpreadingPositionStopCondition;
		protected int OffsetX;
		protected int OffsetY;
		protected int MinX;
		protected int MinY;
		protected int MaxX;
		protected int MaxY;

		/// <summary>
		/// Spreads among positions that satisfy given conditions and marks them with growing numbers.<br/>
		/// Important definitions: <br/>
		/// VISITING A SPREADING POSITION means operating on a position just taken from the queue. <br/>
		/// PROCESSING A NEIGHBOUR means operating on a position reached while spreading from VISITED SPREADING POSITION. A neighbour may be added
		/// to the queue of positions to visit.<br/><br/>
		/// Neighbours are marked with numbers bigger by 1 than mark numbers of VISITED POSITION they are reached from.<br/>
		/// </summary>
		/// 
		/// <remarks>Before running the main loop we process the starting position as if it was a NEIGHBOUR being PROCESSED.<br/>
		/// Note that positions are marked before actually processing them, so if the algorithm stops with positions waiting in queue,
		/// they will be remain marked even though they haven't been processed yet.</remarks>
		/// 
		/// <param name="markMatrix">A matrix that will be initialized with int.MaxValues and then used for storing positions marks. </param>
		/// <param name="parameters">Algorithm parameters.</param>
		/// <returns>True if execution has been stopped by meeting a stop condition. False if ran out of elements in queue.</returns>
		public virtual bool SpillFlood(FloodParameters parameters, int[,] markMatrix)
		{
			Initialize(parameters, markMatrix);

			bool stopConditionMet;
			try
			{
				stopConditionMet = RunFlood();
			}
			finally
			{
				// we make sure we won't prevent the matrix from being garbage collected
				MarkMatrix = null;
			}
			return stopConditionMet;

		}

		protected virtual void Initialize(FloodParameters parameters, int[,] markMatrix)
		{
			MarkMatrix = markMatrix;
			
			// deconstructing parameters
			NeighbourhoodType = parameters.NeighbourhoodType;
			StartX = parameters.StartX;
			StartY = parameters.StartY;
			ProcessStartAsFirstNeighbour = parameters.ProcessStartAsFirstNeighbour;
			Qualifier = parameters.Qualifier;
			NeighbourProcessor = parameters.NeighbourProcessor;
			NeighbourStopCondition = parameters.NeighbourStopCondition;
			SpreadingPositionVisitor = parameters.SpreadingPositionVisitor;
			SpreadingPositionStopCondition = parameters.SpreadingPositionStopCondition;
			PositionsToVisit = parameters.PositionsToVisitQueue;
			FloodBounds boundsRestriction = parameters.BoundsRestriction
				?? new FloodBounds(markMatrix.GetLength(0), markMatrix.GetLength(1));
			GuardBounds(boundsRestriction, StartX, StartY);
			OffsetX = -boundsRestriction.MinX;
			OffsetY = -boundsRestriction.MinY;
			MinX = boundsRestriction.MinX;
			MinY = boundsRestriction.MinY;
			MaxX = boundsRestriction.MaxX;
			MaxY = boundsRestriction.MaxY;

			for (int x = 0; x < MarkMatrix.GetLength(0); x++)
			{
				for (int y = 0; y < MarkMatrix.GetLength(1); y++)
				{
					MarkMatrix[x, y] = int.MaxValue;
				}
			}
		}

		/// <summary>
		/// Main loop of flood spilling. Takes positions from the queue, visits them and spreads the flood from them, 
		/// processing and adding to queue some of their neighbours.
		/// </summary>
		/// <returns>True if execution has been stopped by meeting a stop condition. False if ran out of elements in queue.</returns>
		private bool RunFlood()
		{
			bool startingPositionCausedStop = OperateOnStartingPosition();
			if (startingPositionCausedStop) return true;

			while (PositionsToVisit.Any())
			{
				int currentX, currentY;
				PositionsToVisit.Dequeue(out currentX, out currentY);

				// visiting position that we just got from queue
				SpreadingPositionVisitor?.Invoke(currentX, currentY);
				if (SpreadingPositionStopCondition != null && SpreadingPositionStopCondition(currentX, currentY))
					return true;

				// spreading from visited position
				int markToGive = 1 + GetMark(currentX, currentY);
				bool neighbourCausedStop = SpreadToNeighbours(currentX, currentY, markToGive);
				if (neighbourCausedStop)
					return true;
			}
			return false;
		}

		private bool OperateOnStartingPosition()
		{
			if (!IsValidPosition(StartX, StartY)) return false;

			if (ProcessStartAsFirstNeighbour)
			{
				bool startingPositionCausedStop = ProcessNeighbour(StartX, StartY, 0);
				if (startingPositionCausedStop)
				{
					return true;
				}
			}
			else
			{
				SetMark(StartX, StartY, 0);
				PositionsToVisit.Enqueue(StartX, StartY);
			}
			return false;
		}

		/// <remarks>
		/// Note that a "neighbour" definition depends on implementation. It doesn't necessarily mean that it's adjacent to visited position.
		/// </remarks>
		/// <returns>True if a stop condition in one of neighbours was met. Otherwise false.</returns>
		protected virtual bool SpreadToNeighbours(int visitedX, int visitedY, int markToGive)
		{
			bool neighbourCausedStop = false;

			// intentional construction; comparing to using a generated list of neighbours - saves memory and performs 2 times faster
			if (NeighbourhoodType == NeighbourhoodType.Eight)
			{
				// left side
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX - 1, visitedY - 1, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX - 1, visitedY + 0, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX - 1, visitedY + 1, markToGive);

				// below and above
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX, visitedY -1, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX, visitedY +1, markToGive);

				// right side
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX + 1, visitedY - 1, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX + 1, visitedY + 0, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX + 1, visitedY + 1, markToGive);
			}
			else
			{
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX, visitedY -1, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX, visitedY + 1, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX - 1, visitedY, markToGive);
				neighbourCausedStop |= ProcessNeighbourIfValid(visitedX + 1, visitedY, markToGive);

			}
			return neighbourCausedStop;
		}

		/// <returns>True if a stop condition in one of neighbours was met. Otherwise false.</returns>
		protected bool ProcessNeighbourIfValid(int neighbourX, int neighbourY, int markToGive)
		{
			bool shouldBeProcessed = IsValidPosition(neighbourX, neighbourY);
			if (shouldBeProcessed)
			{
				bool neighbourCausedStop = ProcessNeighbour(neighbourX, neighbourY, markToGive);
				return neighbourCausedStop;
			}

			return false;
		}

		/// <returns>True if the position is within given bounds, it hasn't been marked yet and it satisifies the qualifier.</returns>
		protected bool IsValidPosition(int neighbourX, int neighbourY)
		{
			return neighbourX >= MinX && neighbourX <= MaxX && neighbourY >= MinY && neighbourY <= MaxY
			       && GetMark(neighbourX, neighbourY) == int.MaxValue
			       && (Qualifier == null || Qualifier(neighbourX, neighbourY));
		}

		/// <returns>True if the stop condition was met. Otherwise false.</returns>
		protected bool ProcessNeighbour(int neighbourX, int neighbourY, int markToGive, bool shouldEnqueue = true)
		{
			SetMark(neighbourX, neighbourY, markToGive);
			NeighbourProcessor?.Invoke(neighbourX, neighbourY, markToGive);
			if(NeighbourStopCondition != null && NeighbourStopCondition(neighbourX, neighbourY))
				return true;
			if(shouldEnqueue)
				PositionsToVisit.Enqueue(neighbourX, neighbourY);

			return false;
		}

		/// <summary>
		/// Sets given position in MarkMatrix to given mark.
		/// </summary>
		protected void SetMark(int x, int y, int mark)
		{
			MarkMatrix[x + OffsetX, y + OffsetY] = mark;
		}

		/// <summary>
		/// Gets mark for given position in MarkMatrix. 
		/// </summary>
		protected int GetMark(int x, int y)
		{
			return MarkMatrix[x + OffsetX, y + OffsetY];
		}

		protected void GuardBounds(FloodBounds bounds, int startX, int startY)
		{
			int xResultSize = MarkMatrix.GetLength(0);
			int yResultSize = MarkMatrix.GetLength(1);
			if (xResultSize < bounds.SizeX || yResultSize < bounds.SizeY)
				throw new ArgumentException($"MarkMatrix size ({xResultSize}, {yResultSize}) " +
				                            $"is smaller than bounds size ({bounds.SizeX}, {bounds.SizeY})");

			if(!bounds.Contains(startX, startY))
			{
				throw new ArgumentException($"start position ({startX}, {startY}) is not contained in given bounds " +
				                            $"(minX {bounds.MinX}, minY {bounds.MinY}, sizeX {bounds.SizeX}, sizeY {bounds.SizeY}).");
			}
		}
	}
}