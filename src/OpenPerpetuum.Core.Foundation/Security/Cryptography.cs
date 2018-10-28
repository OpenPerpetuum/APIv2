using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OpenPerpetuum.Core.Foundation.Security
{
	public static class Cryptography
	{
		/// <summary>
		/// Used for creating small readable crypto-tokens
		/// </summary>
		/// <param name="minimumLength"></param>
		/// <param name="tokenHashAlphabet"></param>
		/// <returns></returns>
		public static string CreateEncodedHashId(int minimumLength, string tokenHashAlphabet)
		{
			var seed = new Random(BitConverter.ToInt32(CreateRandomBytes(4), 0)).Next(0, int.MaxValue);
			var salt = CreateRandomBytes(32);

			string saltString = Convert.ToBase64String(salt);

			var hashId = new HashidsNet.Hashids(salt: saltString, minHashLength: minimumLength, alphabet: tokenHashAlphabet);
			var hash = hashId.Encode(seed);

			return hash;
		}
		public static byte[] CreatePasswordForStorage(string password)
		{
			byte[] salt = CreateRandomBytes(32);
			byte[] encryptedPassword = CreatePasswordForStorage(password, salt);

			return encryptedPassword;
		}

		public static byte[] CreatePasswordForStorage(string password, byte[] salt)
		{
			byte[] hashedPassword = HashPassword(password, salt);
			byte[] encryptedPassword = ReadyPasswordForStorage(salt, hashedPassword);

			return encryptedPassword;
		}

		public static byte[] CreateRandomBytes(int numberOfBytes = 32)
		{
			var randomGenerator = RandomNumberGenerator.Create();

			byte[] cryptoBytes = new byte[numberOfBytes];

			randomGenerator.GetBytes(cryptoBytes);

			return cryptoBytes;
		}

		public static byte[] HashPassword(string password, byte[] salt, int numberOfIterations = 50000)
		{
			byte[] hashedPassword =
				KeyDerivation.Pbkdf2(
					password,
					salt,
					KeyDerivationPrf.HMACSHA512,
					numberOfIterations,
					32);

			return hashedPassword;
		}

		public static byte[] ReadyPasswordForStorage(byte[] salt, byte[] hashedPassword)
		{
			var encryptedPassword = new byte[hashedPassword.Length + salt.Length];

			Array.Copy(salt, encryptedPassword, salt.Length);
			Array.Copy(hashedPassword, 0, encryptedPassword, salt.Length, hashedPassword.Length);

			return encryptedPassword;
		}
	}
}
