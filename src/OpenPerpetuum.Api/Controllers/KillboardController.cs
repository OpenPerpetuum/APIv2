using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Api.Models;
using OpenPerpetuum.Api.Models.Killboard;
using OpenPerpetuum.Core.Definition.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Killboard;
using OpenPerpetuum.Core.Killboard.DataModel;
using OpenPerpetuum.Core.Killboard.Queries;
using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Controllers
{
    [Route("api/[controller]"), Authorize(Policy = Scopes.ExternalKillboard)]
    [ApiController]
    public class KillboardController : ApiControllerBase
    {
        public KillboardController(ICoreContext coreContext) : base(coreContext)
        { }

        [HttpGet("today")]
        public async Task<IActionResult> GetKillsToday([FromQuery]int? page, [FromQuery]int? resultsPerPage)
        {
            if (!page.HasValue)
                page = 0;
            if (!resultsPerPage.HasValue)
                resultsPerPage = 10;
            if (resultsPerPage.Value > 100)
                resultsPerPage = 100;

            KillboardDataResultModel kbData = QueryProcessor.Process(new GAME_GetTodaysKillReportsQuery
            {
                Page = page.Value,
                ResultsPerPage = resultsPerPage.Value,
                TodayDate = GenericContext.CurrentDateTime
            });

            var robotList = kbData.KillData
                .Select(kd => kd.Victim.Robot)
                .Union(kbData.KillData.SelectMany(kd => kd.Attackers, (kd, kda) => kda.Attacker.Robot))
                .OrderBy(id => id)
                .Distinct()
                .Select(id => QueryProcessor.Process(new GAME_FindRobotEntityDefaultsByDefinitionQuery { DefinitionId = id }))
                .Select(dm => new BasicRobotViewModel
                {
                    DefinitionName = dm.DefinitionName,
                    DescriptionToken = dm.DescriptionToken,
                    RobotId = dm.Definition
                })
                .ToList()
                .AsReadOnly();

            GetKillboardResponseViewModel viewModel = new GetKillboardResponseViewModel
            {
                Page = kbData.Page,
                ResultsPerPage = kbData.ResultsPerPage,
                ResultsReturned = kbData.ResultsReturned,
                KillReports = kbData.KillData.Select(kd => CreateKillReportViewModel(kd, robotList)).ToList().AsReadOnly()
            };

            return await Task.Run(() => Ok(viewModel));
        }

        [HttpGet]
        public async Task<IActionResult> GetKillboard([FromQuery]int? page, [FromQuery]int? resultsPerPage)
        {
            if (!page.HasValue)
                page = 0;
            if (!resultsPerPage.HasValue)
                resultsPerPage = 10;
            if (resultsPerPage.Value > 100)
                resultsPerPage = 100;

            KillboardDataResultModel kbData = QueryProcessor.Process(new GAME_GetKillboardNoFilterQuery
            {
                Page = page.Value,
                ResultsPerPage = resultsPerPage.Value
            });
            var robotList = kbData.KillData
                .Select(kd => kd.Victim.Robot)
                .Union(kbData.KillData.SelectMany(kd => kd.Attackers, (kd, kda) => kda.Attacker.Robot))
                .OrderBy(id => id)
                .Distinct()
                .Select(id => QueryProcessor.Process(new GAME_FindRobotEntityDefaultsByDefinitionQuery { DefinitionId = id }))
                .Select(dm => new BasicRobotViewModel
                {
                    DefinitionName = dm.DefinitionName,
                    DescriptionToken = dm.DescriptionToken,
                    RobotId = dm.Definition
                })
                .ToList()
                .AsReadOnly();

            GetKillboardResponseViewModel viewModel = new GetKillboardResponseViewModel
            {
                Page = kbData.Page,
                ResultsPerPage = kbData.ResultsPerPage,
                ResultsReturned = kbData.ResultsReturned,
                KillReports = kbData.KillData.Select(kd => CreateKillReportViewModel(kd, robotList)).ToList().AsReadOnly()
            };

            return await Task.Run(() => Ok(viewModel));
        }

        private KillReportViewModel CreateKillReportViewModel(KillDataGenxy killData, ReadOnlyCollection<BasicRobotViewModel> robots)
        {
            var krvm = new KillReportViewModel
            {
                Victim = new BasicCharacterViewModel
                {
                    CharacterId = killData.Victim.CharacterId,
                    Corporation = killData.Victim.Corporation,
                    Location = killData.Victim.Position,
                    Name = killData.Victim.Nick,
                    Robot = robots.SingleOrDefault(r => r.RobotId == killData.Victim.Robot) ?? new BasicRobotViewModel
                    {
                        DefinitionName = "def_unknown_bot",
                        DescriptionToken = "def_unknown_bot_desc",
                        RobotId = killData.Victim.Robot
                    }
                },
                Attackers = killData.Attackers.Select(a => new KillAttackerViewModel
                {
                    Attacker = new BasicCharacterViewModel
                    {
                        CharacterId = a.Attacker.CharacterId,
                        Corporation = a.Attacker.Corporation,
                        Location = a.Attacker.Position,
                        Name = a.Attacker.Nick,
                        Robot = robots.SingleOrDefault(r => r.RobotId == a.Attacker.Robot)
                    },
                    DamageDone = a.DamageDone,
                    DemobilizerCycles = a.Demobilizer,
                    Dispersion = a.Dispersion,
                    JammerSuccessfulCycles = a.Jammer,
                    JammerTotalCycles = a.JammerTotal,
                    KillingBlow = a.KillingBlow > 0,
                    SuppressorCycles = a.Suppressor
                }).ToList().AsReadOnly(),
                ZoneName = QueryProcessor.Process(new GAME_FindZoneByZoneIdQuery { ZoneId = killData.ZoneId })?.Name ?? "Unknown zone"
            };

            return krvm;
        }
    }
}
