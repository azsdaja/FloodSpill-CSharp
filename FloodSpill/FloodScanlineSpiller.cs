using System.Runtime.CompilerServices;

namespace FloodSpill
{
	/// <summary>
	/// Flood filling using scanline algorithm, where we try to process whole vertical lines as neigbhours (to save time on memory access) 
	/// and not add all processed neighbours to the queue.
	/// Inspired by: https://web.archive.org/web/20171220215816/http://lodev.org:80/cgtutor/floodfill.html
	/// </summary>
	public class FloodScanlineSpiller : FloodSpiller
	{
		protected override bool SpreadToNeighbours(int currentX, int currentY, int markToGive)
		{
			int lineMinY = currentY;
			while (IsValidPosition(currentX, lineMinY - 1))
			{
				lineMinY -= 1;
			}
			
			int lineMaxY = currentY;
			while (IsValidPosition(currentX, lineMaxY + 1))
			{
				lineMaxY += 1;
			}

			// now lineMinY and lineMaxY define the longest vertical line containing current position 
			// which contains positions that are valid to process.

			return ProcessNeighboursAlongLine(currentX, currentY, lineMinY, lineMaxY, markToGive);
		}

		/// <summary>
		/// Moves along given vertical line and marks all positions on its way. Validity check for main line is not needed because it 
		/// is supposed to contain positions that are valid. However, we check left and right neighbours of positions on the line 
		/// and if they start a new streak of valid positions, we add them to the queue.
		/// </summary>
		private bool ProcessNeighboursAlongLine(int parentX, int parentY, int lineMinY, int lineMaxY, int markToGive)
		{
			bool stopConditionMet = false;
			int mainLineX = parentX;

			// first we process neighbours on the main line. We omit yInLine equal to parentY, because it already has been processed before
			for (int yInLine = lineMinY; yInLine < parentY; yInLine++)
				if(IsValidPosition(mainLineX, yInLine))
					stopConditionMet |= ProcessNeighbour(mainLineX, yInLine, markToGive, shouldEnqueue: false);
			for (int yInLine = parentY + 1; yInLine <= lineMaxY; yInLine++)
				if (IsValidPosition(mainLineX, yInLine))
					stopConditionMet |= ProcessNeighbour(mainLineX, yInLine, markToGive, shouldEnqueue: false);

			// now we'll follow the sidelines and process positions on them which start new streaks of valid positions

			bool includeDiagonalDirections = NeighbourhoodType == NeighbourhoodType.Eight;
			// if we want to include diagonal neighbourhood, we just need to check sides for a range bigger by 1 at both ends
			int sideLineMinY = includeDiagonalDirections ? lineMinY - 1 : lineMinY;
			int sideLineMaxY = includeDiagonalDirections ? lineMaxY + 1 : lineMaxY;

			bool leftStreakActive = false, rightStreakActive = false;
			for (int yInLine = sideLineMinY; yInLine <= sideLineMaxY; yInLine++)
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, mainLineX -1, yInLine, ref leftStreakActive);
			for (int yInLine = sideLineMinY; yInLine <= sideLineMaxY; yInLine++)
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, mainLineX +1, yInLine, ref rightStreakActive);
			
			return stopConditionMet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool CheckStreakAndProcessSide(int markToGive, int sideLineX, int yInLine, ref bool streakActive)
		{
			bool stopConditionMet = false;

			if (!streakActive)
			{
				if (IsValidPosition(sideLineX, yInLine))
				{
					stopConditionMet |= ProcessNeighbour(sideLineX, yInLine, markToGive);
					streakActive = true;
				}
			}
			else
			{
				if (!IsValidPosition(sideLineX, yInLine))
					streakActive = false;
			}
			
			return stopConditionMet;
		}
	}
}