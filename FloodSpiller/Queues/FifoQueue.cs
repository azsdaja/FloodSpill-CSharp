using System.Collections.Generic;
using System.Linq;

namespace FloodSpiller.Queues
{
	public class FifoQueue : PositionQueueAdapter
	{
		private readonly Queue<Position> _queue;

		public FifoQueue()
		{
			_queue = new Queue<Position>(32);
		}

		public override bool Any()
		{
			return _queue.Any();
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