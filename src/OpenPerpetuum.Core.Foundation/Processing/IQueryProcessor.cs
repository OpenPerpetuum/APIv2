namespace OpenPerpetuum.Core.Foundation.Processing
{
	public interface IQueryProcessor
	{
		TResult Process<TResult>(IQuery<TResult> query);
	}
}
