using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Core.DataServices.Context
{
	public class CoreContext : ICoreContext
	{
		public CoreContext(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor, IGenericContext genericContext, IIdGeneratorService idGenerator)
		{
			CommandProcessor = commandProcessor;
			GenericContext = genericContext;
			QueryProcessor = queryProcessor;
			IdGenerator = idGenerator;
		}

		public ICommandProcessor CommandProcessor
		{
			get;
		}

		public IGenericContext GenericContext
		{
			get;
		}

		public IQueryProcessor QueryProcessor
		{
			get;
		}

		public IIdGeneratorService IdGenerator
		{
			get;
		}
	}
}
