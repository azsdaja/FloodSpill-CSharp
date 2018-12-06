namespace FloodSpill.Utilities
{
	/// <summary>
	/// A predicate with two parameters.
	/// </summary>
	public delegate bool Predicate<in TFirst, in TSecond>(TFirst first, TSecond second);
}