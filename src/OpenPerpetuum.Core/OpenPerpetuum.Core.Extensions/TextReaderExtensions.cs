using System.Collections.Generic;
using System.IO;

namespace OpenPerpetuum.Core.Extensions
{
	public static class TextReaderExtensions
	{
		public static IEnumerable<string> ReadByDelimiter(this TextReader reader, char delimiter)
		{
			List<char> chars = new List<char>();
			while (reader.Peek() >= 0)
			{
				char c = (char)reader.Read();

				if (c == delimiter)
				{
					yield return new string(chars.ToArray());
					chars.Clear();
					continue;
				}

				chars.Add(c);
			}
		}

		public static string ReadUntilDelimiter(this TextReader reader, char delimiter)
		{
			List<char> chars = new List<char>();
			while (reader.Peek() >= 0)
			{
				char c = (char)reader.Read();

				if (c == delimiter)
					break;

				chars.Add(c);
			}

			return new string(chars.ToArray());
		}
	}
}
