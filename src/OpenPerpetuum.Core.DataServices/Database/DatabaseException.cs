using System;

namespace OpenPerpetuum.Core.DataServices.Database
{
	/// <summary>
	/// Wrapper around Exception that allows the use of a SQL error code enum for better usability
	/// </summary>
	public class DatabaseException : Exception
	{
		public DatabaseErrorType ErrorType
		{
			get;
		}

		public DatabaseException(DatabaseErrorType errorType) : this(errorType, string.Empty)
		{ }

		public DatabaseException(DatabaseErrorType errorType, string message) : this(errorType, message, null)
		{ }

		public DatabaseException(DatabaseErrorType errorType, string message, Exception innerException) : base(message, innerException)
		{
			ErrorType = errorType;
		}
	}
}
