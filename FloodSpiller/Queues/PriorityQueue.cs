using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using FloodSpiller.Utilities;

namespace FloodSpiller.Queues
{
	public class PriorityQueue : PositionQueueAdapter
	{
		private readonly IntervalHeap<Position> _intervalHeap;

		public PriorityQueue(IComparer<Position> comparer = null)
		{
			_intervalHeap = new IntervalHeap<Position>(comparer);
		}

		public PriorityQueue(Func<Position, Position, int> comparer) : this(new FunctionalComparer<Position>(comparer))
		{
		}

		public override bool Any()
		{
			return _intervalHeap.Count > 0; // note: using _intervalHeap.Any() would cause an allocation of new IntervalHeap.Enumerator each time!
		}

		public override void Enqueue(int x, int y)
		{
			var toEnqueue = new Position(x, y);
			_intervalHeap.Add(toEnqueue);
		}

		public override void Dequeue(out int x, out int y)
		{
			Position dequeued = _intervalHeap.FindMin();
			x = dequeued.X;
			y = dequeued.Y;
			_intervalHeap.DeleteMin();
		}
	}
}