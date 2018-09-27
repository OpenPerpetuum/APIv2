using System;

namespace OpenPerpetuum.Core.DataServices
{
	/// <summary>
	/// Required to build up user defined table types for submission to MS SQL
	/// When used with a decimal, ensure to set the DecimalPrecision and DecimalScale
	/// properties or you may find that numbers are truncated
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class DataItemAttribute : Attribute
	{
		public DataItemAttribute(int ordinal)
		{
			Ordinal = ordinal;
		}

		public int Ordinal { get; }

		/// <summary>
		/// Maximum number of characters in this string. Set to -1 if the column is declared as "max"
		/// </summary>
		public long MaxLength
		{
			get; set;
		}

		/// <summary>
		/// Total number of digits in the decimal.
		/// e.g. 20.1234 has a precision of 6
		/// </summary>
		public byte DecimalPrecision
		{
			get; set;
		}

		/// <summary>
		/// The number of digits after the decimal point
		/// e.g. 20.1234 has a scale of 4
		/// </summary>
		public byte DecimalScale
		{
			get; set;
		}
	}
}
