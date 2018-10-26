using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenPerpetuum.Core.Foundation.Security
{
	public static class LegacyPassword
	{
		public static byte[] ToLegacySha1(this string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;

			using (SHA1Managed hasher = new SHA1Managed())
			{
				Span<byte> output = new byte[20];

				if (!hasher.TryComputeHash(Encoding.UTF8.GetBytes(input), output, out int bytesWritten))
					return null;
				else
					return output.ToArray();
			}
		}

		public static string ToLegacyShaString(this string input)
		{
			byte[] hash = input.ToLegacySha1();

			if (hash == null)
				return string.Empty;

			return string.Join(string.Empty, hash.Select(c => c.ToString("x2")).ToArray());
		}
	}
}
