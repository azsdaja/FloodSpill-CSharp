using System;

namespace FloodSpill
{
	public struct Position : IEquatable<Position>
	{
		public int X { get; }

		public int Y { get; }

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Position MinValue = new Position(Int32.MinValue, Int32.MinValue);

		public override string ToString()
		{
			return $"({X}, {Y})";
		}

		public static bool operator ==(Position lhs, Position rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator !=(Position lhs, Position rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object other)
		{
			if (!(other is Position))
				return false;
			return Equals((Position)other);
		}

		public bool Equals(Position other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() << 2;
		}

		public static double Distance(Position first, Position other)
		{
			int deltaX = first.X - other.X;
			int deltaY = first.Y - other.Y;
			return Math.Sqrt(deltaX*deltaX + deltaY*deltaY);
		}
	}
}
