using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Killboard;
using OpenPerpetuum.Core.Killboard.Queries;
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

        [HttpGet]
        public async Task<IActionResult> GetKillboard([FromQuery]int? page, [FromQuery]int? resultsPerPage)
        {
            if (!page.HasValue)
                page = 0;
            if (!resultsPerPage.HasValue)
                resultsPerPage = 10;

            KillboardDataModel kbData = QueryProcessor.Process(new GAME_GetKillboardNoFilterQuery
            {
                Page = page.Value,
                ResultsPerPage = resultsPerPage.Value
            });

            return await Task.Run(() => Ok(kbData));
        }
    }
}
