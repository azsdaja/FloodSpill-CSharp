using System.Collections.Generic;
using System.Linq;

namespace FloodSpiller.Queues
{
	public class LifoQueue : PositionQueueAdapter
	{
		private readonly Stack<Position> _stack;

		public LifoQueue()
		{
			_stack = new Stack<Position>(32);
		}

		public override bool Any()
		{
			return _stack.Any();
		}

		public override void Enqueue(int x, int y)
		{
			var toEnqueue = new Position(x, y);
			_stack.Push(toEnqueue);
		}

		public override void Dequeue(out int x, out int y)
		{
			Position dequeued = _stack.Pop();
			x = dequeued.X;
			y = dequeued.Y;
		}
	}
}