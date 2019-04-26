using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Controllers
{
    [Route("api/[controller]"), Authorize(Policy = Scopes.ExternalKillboard)]
    [ApiController]
    public class KillboardController : ApiControllerBase
    {
        public KillboardController(ICoreContext coreContext) : base(coreContext)
        { }

        [HttpGet()]
        public async Task<IActionResult> GetKillboard()
        {
            // This just stops the compiler warning about not use async stuff in an async method
            return await Task.Run(() => NotFound());
        }
    }
}
