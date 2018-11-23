using OpenPerpetuum.Core.Foundation.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenPerpetuum.Core.Genxy
{
	internal sealed class InternalReader
	{
		private static readonly Lazy<InternalReader> instance = new Lazy<InternalReader>(() => new InternalReader());
		private static readonly Lazy<NumberFormatInfo> numberFormat = new Lazy<NumberFormatInfo>(() => { var ni = (NumberFormatInfo)CultureInfo.InstalledUICulture.NumberFormat.Clone(); ni.NumberDecimalSeparator = "."; return ni; });

		private InternalReader()
		{ }

		internal static InternalReader Instance
		{
			get
			{
				return instance.Value;
			}
		}

		internal string ReadEscapedString(TextReader reader)
		{
			var v = ReadValueAsString(reader);
			return ParseEscapedString(v);
		}

		internal string[] ReadEscapedStringArray(TextReader reader)
		{
			return ReadValueAsArray(reader, ParseEscapedString);
		}

		internal int ReadInt(TextReader reader)
		{
			var valueString = ReadValueAsString(reader);
			return ParseInt(valueString);
		}

		internal int[] ReadIntArray(TextReader reader)
		{
			return ReadValueAsArray(reader, ParseInt);
		}

		internal byte[] ReadByteArray(TextReader reader)
		{
			var v = ReadValueAsString(reader);

			int len = v.Length / 2;
			byte[] r = new byte[len];

			for (var i = 0; i < len; i++)
			{
				r[i] = ParseHexNumber(v.Substring(i * 2, 2), (s, num) => byte.Parse(num, NumberStyles.HexNumber, null));
			}

			return r;
		}

		internal long ReadLong(TextReader reader)
		{
			var v = ReadValueAsString(reader);
			return ParseLong(v);
		}

		internal long[] ReadLongArray(TextReader reader)
		{
			return ReadValueAsArray(reader, ParseLong);
		}

		internal int ReadDecimal(TextReader reader)
		{
			var v = ReadValueAsString(reader);
			return int.Parse(v);
		}

		internal int[] ReadDecimalArray(TextReader reader)
		{
			return ReadValueAsArray(reader, int.Parse);
		}

		internal long ReadDecimalLong(TextReader reader)
		{
			var v = ReadValueAsString(reader);
			return long.Parse(v);
		}

		internal double ReadFloat(TextReader reader)
		{
			var v = ReadValueAsString(reader);
			return double.Parse(v, numberFormat.Value);
		}

		internal double ReadFloatBytes(TextReader reader)
		{
			var v = ReadValueAsString(reader);

			var b = new byte[4];
			for (var i = 0; i < 4; i++)
				b[i] = Convert.ToByte(v.Substring(i * 2, 2), 16);

			return BitConverter.ToSingle(b, 0);
		}

		internal double ReadDoubleBytes(TextReader reader)
		{
			var v = ReadValueAsString(reader);

			var b = new byte[8];
			for (var i = 0; i < 8; i++)
				b[i] = Convert.ToByte(v.Substring(i * 2, 2), 16);

			return BitConverter.ToDouble(b, 0);
		}

		internal DateTime ReadDate(TextReader reader)
		{
			int[] n = ReadValueAsArray(reader, int.Parse, '.');
			return new DateTime(n[0], n[1], n[2], n[3], n[4], n[5]);
		}

		internal Color ReadColor(TextReader reader)
		{
			int[] n = ReadValueAsArray(reader, ParseInt, '.');
			return Color.FromArgb(n[3], n[0], n[1], n[2]);
		}

		//internal Area ReadArea(TextReader reader)
		//{
		//	var v = ReadValueAsString(reader);
		//	return ParseArea(v);
		//}

		//internal Area[] ReadAreaArray(TextReader reader)
		//{
		//	return ReadValueAsArray(ParseArea);
		//}

		internal Point ReadPoint(TextReader reader)
		{
			int[] n = ReadValueAsArray(reader, ParseInt, '.');
			return new Point(n[0], n[1]);
		}

		internal Position ReadPosition(TextReader reader)
		{
			var v = ReadValueAsString(reader);
			return ParsePosition(v);
		}

		internal Position[] ReadPositionArray(TextReader reader)
		{
			return ReadValueAsArray(reader, ParsePosition);
		}

		private static Position ParsePosition(string positionString)
		{
			int[] n = ParseValueAsArray(positionString, ParseInt, '.');
			return new Position(n[0], n[1], n[2]);
		}

		//private static Area ParseArea(string areaString)
		//{
		//	var n = ParseValueAsArray(areaString, ParseInt, '.');
		//	return new Area(n[0], n[1], n[2], n[3]);
		//}

		private static T ParseHexNumber<T>(string hexNumber, Func<int /* sign */, string, T> parser)
		{
			var sign = 1;
			if (hexNumber[0] == '-')
			{
				hexNumber = hexNumber.Substring(1);
				sign = -1;
			}

			return parser(sign, hexNumber);
		}

		private static int ParseInt(string valueString)
		{
			return ParseHexNumber(valueString, (s, num) => int.Parse(num, NumberStyles.HexNumber, null) * s);
		}

		private static long ParseLong(string longString)
		{
			return ParseHexNumber(longString, (s, num) => long.Parse(num, NumberStyles.HexNumber, null) * s);
		}

		private static string ParseEscapedString(string s)
		{
			var result = new StringBuilder();

			int Decode(char d)
			{
				return (d & 0xf) + ((d & 0x40) >> 3) + ((d & 0x40) >> 6);
			}

			var i = 0;
			while (i < s.Length)
			{
				var c = s[i++];
				if (c == '\\')
				{
					var hc = Decode(s[i++]);
					var lc = Decode(s[i++]);
					c = (char)(hc << 4 | lc);
				}
				result.Append(c);
			}

			return result.ToString();
		}

		private T[] ReadValueAsArray<T>(TextReader reader, Func<string, T> valueAction, char separator = ',')
		{
			var v = ReadValueAsString(reader);
			return ParseValueAsArray(v, valueAction, separator);
		}

		private static T[] ParseValueAsArray<T>(string value, Func<string, T> valueAction, char separator)
		{
			if (string.IsNullOrEmpty(value))
				return new T[0];

			var stringArray = value.Split(separator);
			var result = new T[stringArray.Length];
			for (var i = 0; i < stringArray.Length; i++)
			{
				result[i] = valueAction(stringArray[i]);
			}
			return result;
		}

		internal string ReadValueAsString(TextReader reader)
		{
			var sb = new StringBuilder();
			char currentChar;
			while ( (currentChar = (char)reader.Peek()) > -1)
			{
				if (currentChar == '#' || currentChar == '|' || currentChar == ']') break;

				sb.Append((char)reader.Read());
			}

			return sb.ToString();
		}
	}
}
