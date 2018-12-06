namespace FloodSpill
{
	public class FloodBounds
	{
		public FloodBounds(int sizeX, int sizeY)
		{
			MinX = 0;
			MaxX = sizeX - 1;
			MinY = 0;
			MaxY = sizeY - 1;
		}

		public FloodBounds(int minX, int minY, int sizeX, int sizeY)
		{
			MinX = minX;
			MaxX = minX + sizeX - 1;
			MinY = minY;
			MaxY = MinY + sizeY - 1;
		}

		public int MinX { get; }
		public int MaxX { get; }
		public int MinY { get; }
		public int MaxY { get; }

		public int SizeX => MaxX - MinX + 1;
		public int SizeY => MaxY - MinY + 1;

		public bool Contains(int startX, int startY)
		{
			return startX >= MinX && startX <= MaxX && startY >= MinY && startY <= MaxY;
		}
	}
}