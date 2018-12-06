using System.Collections.Generic;

namespace FloodSpill.NeighbourProcessors
{
	/// <summary>
	/// Keeps track of all processed neighbours during flood spilling.
	/// </summary>
	public class AddToListNeighbourProcessor : BaseNeighbourProcessor
	{
		public IList<Position> Result { get; } = new List<Position>();

		public override void Process(int x, int y, int mark)
		{
			Result.Add(new Position(x, y));
		}
	}
}