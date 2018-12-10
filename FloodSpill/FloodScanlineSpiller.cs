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

			return ProcessNeighboursAlongLine(lineMinY, lineMaxY, currentX, currentY, markToGive);
		}

		/// <summary>
		/// Moves along given vertical line and marks all positions on its way. Validity check for main line is not needed because it 
		/// it was already performed in SpreadToNeighbours(). However, we check left and right neighbours of positions on the line 
		/// and if they start a new streak of valid positions, we add them to the queue.
		/// </summary>
		/// <remarks>Performance notes: joining together the operations of checking conditions in SpreadToNeighbours 
		/// and processing in here doesn't help much. Neither is getting rid of "yInLine != parentY" condigtion by using 
		/// two loops instead of one. Also it doesn't make it faster to avoid unnecessary checks for bounds-related conditions.</remarks>
		private bool ProcessNeighboursAlongLine(int lineBeginningY, int lineEndY, int parentX, int parentY, int markToGive)
		{
			bool stopConditionMet = false;
			int mainLineX = parentX;
			int leftLineX = mainLineX - 1;
			int rightLineX = mainLineX + 1;
			bool leftStreakActive = false, rightStreakActive = false;
			bool includeDiagonalDirections = NeighbourhoodType == NeighbourhoodType.Eight;

			if (includeDiagonalDirections)
			{
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, leftLineX, lineBeginningY - 1, ref leftStreakActive);
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, rightLineX, lineBeginningY - 1, ref rightStreakActive);
			}

			for (int yInLine = lineBeginningY; yInLine <= lineEndY; yInLine++)
			{
				if (yInLine != parentY) // if we are at parent (which has already been processed) we skip processing
				{
					// it is the essence of scanline fill to skip enqueueing positions on the line we move along
					stopConditionMet |= ProcessNeighbour(mainLineX, yInLine, markToGive, shouldEnqueue: false);
				}

				stopConditionMet |= CheckStreakAndProcessSide(markToGive, leftLineX, yInLine, ref leftStreakActive);
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, rightLineX, yInLine, ref rightStreakActive);
			}

			if (includeDiagonalDirections)
			{
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, leftLineX, lineEndY + 1, ref leftStreakActive);
				stopConditionMet |= CheckStreakAndProcessSide(markToGive, rightLineX, lineEndY + 1, ref rightStreakActive);
			}

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