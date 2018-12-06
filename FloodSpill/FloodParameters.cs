using System;
using FloodSpill.Queues;
using FloodSpill.Utilities;

namespace FloodSpill
{
	/// <summary>
	/// A container for parameters of <see cref="FloodSpiller"/>.
	/// </summary>
	/// <remarks>
	/// No delegates in here need to be assigned. The algorithm should just ignore them if they are null.
	/// </remarks>
	public class FloodParameters
	{
		public FloodParameters(PositionQueueAdapter positionsToVisitQueue, int startX, int startY) 
		{
			PositionsToVisitQueue = positionsToVisitQueue;
			StartX = startX;
			StartY = startY;
		}

		public FloodParameters(int startX, int startY) : this(new FifoPositionQueue(), startX, startY)
		{
			
		}

		/// <summary>
		/// Abstract queue for storing positions to visit.
		/// </summary>
		public PositionQueueAdapter PositionsToVisitQueue { get; set; }

		/// <summary>
		/// Position coordinate X to start the flood from.
		/// </summary>
		public int StartX { get; set; }

		/// <summary>
		/// Position coordinate Y to start the flood from.
		/// </summary>
		public int StartY { get; set; }

		/// <summary>
		/// Indicates if we should include diagonal neighbourhood when getting neighbours of a position.
		/// </summary>
		public NeighbourhoodType NeighbourhoodType { get; set; }

		/// <summary>
		/// If set to true, before running the main loop, starting position will be processed like if it was someone's neighbour 
		/// reached by flood with mark 0.<br/>
		/// Otherwise (if it's valid position) it will be just marked with 0 and added to queue 
		/// (without using NeighbourProcessor and NeighbourStopCondition).
		/// </summary>
		public bool ProcessStartAsFirstNeighbour { get; set; }

		/// <summary>
		/// Optional bounds limiting the algorithm spread.
		/// </summary>
		public FloodBounds BoundsRestriction { get; set; }

		/// <summary>
		/// An optional condition that a position has to meet in order to be flooded.
		/// </summary>
		public Predicate<int, int> Qualifier { get; set; }
		
		/// <summary>
		/// Does any necessary work on currently visited position.
		/// </summary>
		public Action<int, int> SpreadingPositionVisitor { get; set; }

		/// <summary>
		/// If matched for currently visited position, the algorithm will stop. Performed after executing SpreadingPositionVisitor.
		/// </summary>
		public Predicate<int, int> SpreadingPositionStopCondition { get; set; }

		/// <summary>
		/// Does any necessary work on currently processed neighbour.
		/// </summary>
		public Action<int, int, int> NeighbourProcessor { get; set; }

		/// <summary>
		/// If matched for a neighbour, the algorithm will stop. Performed after executing NeighbourProcessor.
		/// </summary>
		public Predicate<int, int> NeighbourStopCondition { get; set; }
	}
}