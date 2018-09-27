using OpenPerpetuum.Core.DataServices.Database.Interfaces;

namespace OpenPerpetuum.Core.DataServices.Database
{
	public class Result : IResult
	{
		public Result(int errorCode = 0, string errorMessage = "")
		{
			ErrorCode = errorCode;
			ErrorMessage = errorMessage;
			Success = (errorCode == 0) && string.IsNullOrEmpty(errorMessage);
		}

		public int ErrorCode { get; }
		public string ErrorMessage { get; }
		public bool Success { get; }

		public override string ToString()
		{
			return Success ? "Success" : $"Error: {ErrorCode}({ErrorMessage})";
		}
	}

	public class Result<TResultData> : Result, IResult<TResultData>
	{
		public Result(TResultData data, int errorCode = 0, string errorMessage = "")
			: base(errorCode, errorMessage)
		{
			Data = data;
		}

		public TResultData Data
		{ get; }

		public virtual void ValidateResult()
		{
			if (!Success)
				throw new DatabaseException((DatabaseErrorType)ErrorCode, $"Database error: {ErrorCode} ({ErrorMessage})");

			if (Data == null)
				throw new DatabaseException(DatabaseErrorType.UnspecifiedFatalError, "Error loading database result container. Check DatabaseProvider class");
		}
	}
}
