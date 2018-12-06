namespace FloodSpill.Queues
{
	/// <summary>
	/// An abstract queue exposing basic operations for pairs of numbers indicating positions.
	/// </summary>
	public abstract class PositionQueueAdapter
	{
		public abstract bool Any();
		public abstract void Enqueue(int x, int y);
		public abstract void Dequeue(out int x, out int y);
	}
}