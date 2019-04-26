using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Genxy;

namespace OpenPerpetuum.Api.Controllers
{
    [Route("api/[controller]")]
	public class DiagnosticsController : ApiControllerBase
	{
		public DiagnosticsController(ICoreContext coreContext) : base(coreContext)
		{
		}
	}
}
