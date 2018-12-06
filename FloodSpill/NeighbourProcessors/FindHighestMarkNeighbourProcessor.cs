namespace FloodSpill.NeighbourProcessors
{
	/// <summary>
	/// Keeps track of highest mark value and its position during flood spilling.
	/// </summary>
	public class FindHighestMarkNeighbourProcessor : BaseNeighbourProcessor
	{
		public int HighestMark { get; private set; }

		public Position PositionWithHighestMark { get; private set; } = Position.MinValue;

		public override void Process(int x, int y, int mark)
		{
			if (mark > HighestMark)
			{
				HighestMark = mark;
				PositionWithHighestMark = new Position(x, y);
			}
		}
	}
}