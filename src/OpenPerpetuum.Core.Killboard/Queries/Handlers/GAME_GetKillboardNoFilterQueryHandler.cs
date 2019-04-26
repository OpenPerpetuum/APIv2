using Microsoft.Extensions.Options;
using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Genxy;
using OpenPerpetuum.Core.Killboard.DatabaseResults;
using OpenPerpetuum.Core.Killboard.DataModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Killboard.Queries.Handlers
{
    internal class GAME_GetKillboardNoFilterQueryHandler : IQueryHandler<GAME_GetKillboardNoFilterQuery, KillboardDataModel>
    {
        private readonly IDatabaseProvider dataContext;
        private readonly IGenxyReader genxyReader;

        public GAME_GetKillboardNoFilterQueryHandler(IGenxyReader genxyReader, IDataContext dataContext)
        {
            this.genxyReader = genxyReader;
            this.dataContext = dataContext.GetDataContext("Game");
        }

        public KillboardDataModel Handle(GAME_GetKillboardNoFilterQuery query)
        {
            IResult<KillboardResult> result = dataContext.ExecuteProcedure<KillboardResult>(
                "API.GetKillReportsNoFilter",
                new DbParameters
                {
                    { "Page", query.Page },
                    { "ResultsPerPage", query.ResultsPerPage}
                });

            // Always call this after the query
            result.ValidateResult();

            var killList = new List<KillDataGenxy>();

            foreach(KillboardData data in result.Data.Kills)
            {
                if (!string.IsNullOrWhiteSpace(data.GenxyData))
                    killList.Add(genxyReader.Deserialise<KillDataGenxy>(data.GenxyData));
            }

            return new KillboardDataModel
            {
                KillData = killList.AsReadOnly(),
                Page = query.Page,
                ResultsPerPage = query.ResultsPerPage
            };
        }
    }
}
