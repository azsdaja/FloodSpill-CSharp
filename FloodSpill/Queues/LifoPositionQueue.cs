using System.Collections.Generic;
using System.Linq;

namespace FloodSpill.Queues
{
	public class LifoPositionQueue : PositionQueueAdapter
	{
		private readonly Stack<Position> _stack;

		public LifoPositionQueue()
		{
			_stack = new Stack<Position>(32);
		}

		public override bool Any()
		{
			return _stack.Count > 0; // note: using _stack.Any() causes an allocation of new Stack.Enumerator each time!
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