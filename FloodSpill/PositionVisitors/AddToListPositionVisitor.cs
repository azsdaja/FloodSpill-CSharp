using System.Collections.Generic;

namespace FloodSpill.PositionVisitors
{
	public class AddToListPositionVisitor : BasePositionVisitor
	{
		public IList<Position> Result { get; } = new List<Position>();

		public override void Visit(int x, int y)
		{
			Result.Add(new Position(x, y));
		}
	}
}