using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Genxy;
using OpenPerpetuum.Core.Killboard.DatabaseResults;
using OpenPerpetuum.Core.Killboard.DataModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenPerpetuum.Core.Killboard.Queries.Handlers
{
    class GAME_GetTodaysKillReportsQueryHandler : IQueryHandler<GAME_GetTodaysKillReportsQuery, KillboardDataResultModel>
    {
        private readonly IDatabaseProvider db;
        private readonly IGenxyReader reader;

        public GAME_GetTodaysKillReportsQueryHandler(IDataContext dbContext, IGenxyReader reader)
        {
            db = dbContext.GetDataContext("Game");
            this.reader = reader;
        }

        public KillboardDataResultModel Handle(GAME_GetTodaysKillReportsQuery query)
        {
            IResult<KillboardResult> result = db.ExecuteProcedure<KillboardResult>(
                "API.GetTodaysKillReports",
                new DbParameters
                {
                    { "Page", query.Page },
                    { "ResultsPerPage", query.ResultsPerPage },
                    { "TodaysDate", query.TodayDate }
                });

            result.ValidateResult();

            ReadOnlyCollection<KillDataGenxy> output = result.Data.Kills
                .Where(kd => !string.IsNullOrWhiteSpace(kd.GenxyData))
                .Select(kd => reader.Deserialise<KillDataGenxy>(kd.GenxyData))
                .ToList()
                .AsReadOnly();

            return new KillboardDataResultModel
            {
                KillData = output,
                Page = query.Page,
                ResultsPerPage = query.ResultsPerPage,
                ResultsReturned = output.Count
            };
        }
    }
}
