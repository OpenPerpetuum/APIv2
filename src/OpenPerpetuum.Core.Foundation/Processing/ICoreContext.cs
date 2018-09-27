namespace OpenPerpetuum.Core.Foundation.Processing
{
	public interface ICoreContext
	{
		ICommandProcessor CommandProcessor { get; }
		IQueryProcessor QueryProcessor { get; }
		IIdGeneratorService IdGenerator { get; }
		IGenericContext GenericContext { get; }
	}
}
