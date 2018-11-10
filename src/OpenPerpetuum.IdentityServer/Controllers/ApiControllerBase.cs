using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.IdentityServer.Controllers
{
	public class ApiControllerBase : Controller
	{
		private readonly ICoreContext CoreContext;

		protected ICommandProcessor CommandProcessor => CoreContext.CommandProcessor;
		protected IQueryProcessor QueryProcessor => CoreContext.QueryProcessor;
		protected IGenericContext GenericContext => CoreContext.GenericContext;
		protected IIdGeneratorService IdGenerator => CoreContext.IdGenerator;

		public ApiControllerBase(ICoreContext coreContext)
		{
			CoreContext = coreContext;
		}
	}
}
