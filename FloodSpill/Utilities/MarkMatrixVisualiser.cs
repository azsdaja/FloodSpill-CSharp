using System.Text;

namespace FloodSpill.Utilities
{
	public static class MarkMatrixVisualiser
	{
		/// <summary>Creates a string which, printed to console, will visualize given markMatrix as a matrix of characters.
		/// Bottom-left corner is 0,0 position. X grows to right, Y grows to top.</summary>
		/// <remarks>Character representation:<br/><br/>
		/// For negative numbers: '-'.<br/>
		/// For single-digit numbers: '0'-'9'.<br/>
		/// For some following numbers: 'a'-'z'.<br/>
		/// For some following numbers: 'A'-'Z'.<br/>
		/// For bigger numbers: '+'. <br/>
		/// For int.MaxValue: '#'</remarks>
		public static string Visualise(int[,] markMatrix)
		{
			if (markMatrix == null)
			{
				return "null";
			}

			StringBuilder result = new StringBuilder();
			result.AppendLine(
				$"Mark matrix of size {markMatrix.GetLength(0)}, {markMatrix.GetLength(1)}.");
			for (int y = markMatrix.GetLength(1) - 1; y >= 0; y--)
			{
				for (int x = 0; x < markMatrix.GetLength(0); x++)
				{
					int mark = markMatrix[x, y];
					char markCharacter = MarkToChar(mark);
					result.Append(markCharacter);
				}
				result.AppendLine();
			}

			return result.ToString();
		}

		private static char MarkToChar(int mark)
		{
			if (mark < 0)
			{
				return '-';
			}
			if (mark == int.MaxValue)
			{
				return '#';
			}
			if (mark <= 9)
			{
				return mark.ToString()[0];
			}

			const int digitsCount = 10;
			int alphabetLength = 'z' - 'a' + 1;
			int distanceFromSmallA = mark - digitsCount;
			if (distanceFromSmallA < alphabetLength)
			{
				return (char)('a' + distanceFromSmallA);
			}

			int distanceFromBigA = mark - digitsCount - alphabetLength;
			if (distanceFromBigA < alphabetLength)
			{
				return (char)('A' + distanceFromBigA);
			}

			return '+';
		}
	}
}