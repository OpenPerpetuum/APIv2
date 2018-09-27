namespace OpenPerpetuum.Core.DataServices.Database.Interfaces
{
	public interface IResult
	{
		bool Success { get; }
		string ErrorMessage { get; }
		int ErrorCode { get; }
	}
	
	public interface IResult<out TData> : IResult
	{
		TData Data { get; }
		void ValidateResult();
	}
}
