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

			// todo: maybe would be faster if wouldn't separate checking and processing
			return ProcessNeighboursAlongLine(lineMinY, lineMaxY, currentX, currentY, markToGive);
		}

		/// <summary>
		/// Moves along given vertical line and marks all positions on its way. Validity check is not needed as the line is supposed to contain
		/// positions that are valid. However, we also check left and right neighbours of positions on the line and if they start a new
		/// streak of valid positions, we add them to the queue.
		/// </summary>
		private bool ProcessNeighboursAlongLine(int lineBeginningY, int lineEndY, int parentX, int parentY, int markToGive)
		{
			bool stopConditionMet = false;
			int lineX = parentX;
			bool leftStreak = false, rightStreak = false;

			bool includeDiagonalDirections = NeighbourhoodType == NeighbourhoodType.Eight;
			if (includeDiagonalDirections)
			{
				stopConditionMet |= ProcessSides(markToGive, lineX, lineBeginningY - 1, ref leftStreak, ref rightStreak);
			}
			for (int yInLine = lineBeginningY; yInLine <= lineEndY; yInLine++)
			{
				if (yInLine != parentY) // if we are at parent (which has already been processed) we skip processing
				{
					// it is the essence of scanline fill to skip enqueueing positions on the line we move along
					stopConditionMet |= ProcessNeighbour(lineX, yInLine, markToGive, shouldEnqueue: false);
				}

				stopConditionMet |= ProcessSides(markToGive, lineX, yInLine, ref leftStreak, ref rightStreak);
			}
			if (includeDiagonalDirections)
			{
				stopConditionMet |= ProcessSides(markToGive, lineX, lineEndY + 1, ref leftStreak, ref rightStreak);
			}
			return stopConditionMet;
		}

		private bool ProcessSides(int markToGive, int lineX, int yInLine, ref bool leftStreak, ref bool rightStreak)
		{
			bool stopConditionMet = false;

			int leftNeighbourX = lineX - 1;
			if (!leftStreak)
			{
				if (IsValidPosition(leftNeighbourX, yInLine))
				{
					stopConditionMet |= ProcessNeighbour(leftNeighbourX, yInLine, markToGive);
					leftStreak = true;
				}
			}
			else
			{
				if (!IsValidPosition(leftNeighbourX, yInLine))
					leftStreak = false;
			}

			int rightNeighbourX = lineX + 1;
			if (!rightStreak)
			{
				if (IsValidPosition(rightNeighbourX, yInLine))
				{
					stopConditionMet |= ProcessNeighbour(rightNeighbourX, yInLine, markToGive);
					rightStreak = true;
				}
			}
			else
			{
				if (!IsValidPosition(rightNeighbourX, yInLine))
					rightStreak = false;
			}

			return stopConditionMet;
		}
	}
}