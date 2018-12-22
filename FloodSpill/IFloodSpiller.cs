namespace FloodSpill
{
	public interface IFloodSpiller
	{
		/// <summary>
		/// Spreads among positions that satisfy given conditions and marks them with growing numbers.<br/>
		/// Important definitions: <br/>
		/// VISITING A SPREADING POSITION means operating on a position just taken from the queue. <br/>
		/// PROCESSING A NEIGHBOUR means operating on a position reached while spreading from VISITED SPREADING POSITION. A neighbour may be added
		/// to the queue of positions to visit.<br/><br/>
		/// Neighbours by default are marked with numbers bigger by 1 than mark numbers of VISITED POSITION they are reached from.<br/>
		/// </summary>
		/// 
		/// <param name="markMatrix">A matrix that will be initialized with int.MaxValues and then used for storing positions marks. </param>
		/// <param name="parameters">Algorithm parameters.</param>
		/// <returns>True if execution has been stopped by meeting a stop condition. False if ran out of elements in queue.</returns>
		bool SpillFlood(FloodParameters parameters, int[,] markMatrix);
	}
}