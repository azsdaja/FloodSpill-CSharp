using FloodSpill.Queues;
using FloodSpill.Utilities;
using System;
using System.Runtime.CompilerServices;

namespace FloodSpill
{
	/// <summary>
	/// Realisation of flood-spill algorithm. For instructions and examples go to https://github.com/azsdaja/FloodSpill-CSharp
	/// </summary>
	public class FloodSpiller : IFloodSpiller
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
		/// Neighbours by are with numbers having some relation to the number at spreading position - for example they can be bigger by 1 
		/// than mark numbers of the position they are reached from.<br/>
		/// </summary>
		/// 
		/// <remarks>Note that positions are marked before actually processing them, so if the algorithm stops with positions waiting in queue,
		/// they will be remain marked even though they haven't been processed yet.</remarks>
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
			if (PositionsToVisit.Any())
			{
				throw new ArgumentException("Provided PositionsToVisitQueue was not empty at the beginning of flood spilling.", nameof(parameters));
			}
			FloodBounds boundsRestriction = parameters.BoundsRestriction
				?? new FloodBounds(markMatrix.GetLength(0), markMatrix.GetLength(1));
			GuardBounds(boundsRestriction, StartX, StartY);
			OffsetX = -boundsRestriction.MinX;
			OffsetY = -boundsRestriction.MinY;
			MinX = boundsRestriction.MinX;
			MinY = boundsRestriction.MinY;
			MaxX = boundsRestriction.MaxX;
			MaxY = boundsRestriction.MaxY;

			InitializeMarkMatrix();
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
				int markToGive = CalculateMark(currentX, currentY);
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

		/// <summary>
		/// Returns mark number that will be given to positions reached from given spreading position.
		/// </summary>
		protected virtual int CalculateMark(int spreadingPositionX, int spreadingPositionY)
		{
			return 1 + GetMark(spreadingPositionX, spreadingPositionY);
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
		// [MethodImpl(MethodImplOptions.AggressiveInlining)] doesn't seem to help
		protected bool IsValidPosition(int x, int y)
		{
			return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY
			       && GetMark(x, y) == int.MaxValue
			       && (Qualifier == null || Qualifier(x, y));
		}

		/// <returns>True if the stop condition was met. Otherwise false.</returns>
		// [MethodImpl(MethodImplOptions.AggressiveInlining)] doesn't seem to help
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
		/// Gets mark for given position in MarkMatrix. 
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int GetMark(int x, int y)
		{
			return MarkMatrix[x + OffsetX, y + OffsetY];
		}

		/// <summary>
		/// Sets given position in MarkMatrix to given mark.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetMark(int x, int y, int mark)
		{
			MarkMatrix[x + OffsetX, y + OffsetY] = mark;
		}

		protected virtual void InitializeMarkMatrix()
		{
			for (int x = 0; x < MarkMatrix.GetLength(0); x++)
			{
				for (int y = 0; y < MarkMatrix.GetLength(1); y++)
				{
					MarkMatrix[x, y] = int.MaxValue;
				}
			} // takes ~5 ms for 2000x2000
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