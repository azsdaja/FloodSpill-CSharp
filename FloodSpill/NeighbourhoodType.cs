namespace FloodSpill
{
	public enum NeighbourhoodType
	{
		/// <summary>
		/// Indicates that the flood can spread towards diagonal and non-diagonal directions.
		/// </summary>
		Eight,

		/// <summary>
		/// Indicates that the flood can spread only towards non-diagonal directions.
		/// </summary>
		Four

		// todo: Six could also be implemented for hexagonal grids
	}
}