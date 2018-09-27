namespace OpenPerpetuum.Core.Foundation.Processing
{
	public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
	{
		TResult Handle(TQuery query);
	}
}
