using System.Collections.Generic;

namespace FloodSpill.Queues
{
	public class FifoPositionQueue : PositionQueueAdapter
	{
		private readonly Queue<Position> _queue;

		public FifoPositionQueue()
		{
			_queue = new Queue<Position>(32);
		}

		public override bool Any()
		{
			return _queue.Count > 0; // note: using _queue.Any() would cause an allocation of new Queue.Enumerator each time!
		}

		public override void Enqueue(int x, int y)
		{
			var toEnqueue = new Position(x, y);
			_queue.Enqueue(toEnqueue);
		}

		public override void Dequeue(out int x, out int y)
		{
			Position dequeued = _queue.Dequeue();
			x = dequeued.X;
			y = dequeued.Y;
		}
	}
}