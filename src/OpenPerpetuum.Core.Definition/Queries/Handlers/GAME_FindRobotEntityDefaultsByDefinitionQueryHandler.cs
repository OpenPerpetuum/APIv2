using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Definition.DatabaseResults;
using OpenPerpetuum.Core.Definition.DataModel;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Genxy;
using System;
using System.Linq;

namespace OpenPerpetuum.Core.Definition.Queries.Handlers
{
    internal class GAME_FindRobotEntityDefaultsByDefinitionQueryHandler : IQueryHandler<GAME_FindRobotEntityDefaultsByDefinitionQuery, BasicRobotDataModel>
    {
        private readonly IDatabaseProvider dataContext;
        private readonly IGenxyReader genxyReader;

        public GAME_FindRobotEntityDefaultsByDefinitionQueryHandler(IGenxyReader genxyReader, IDataContext dataContext)
        {
            this.genxyReader = genxyReader;
            this.dataContext = dataContext.GetDataContext("Game");
        }
        public BasicRobotDataModel Handle(GAME_FindRobotEntityDefaultsByDefinitionQuery query)
        {
            var result = dataContext.ExecuteProcedure<EntityDefaultsResult>(
                "API.GetEntityDefaultsByDefinition",
                new DbParameters
                {
                    { "Definition", query.DefinitionId }
                });

            result.ValidateResult();

            if (!result.Data.EntityDefaults.Any()) return default(BasicRobotDataModel);
            if (result.Data.EntityDefaults.Count > 1)
                throw new InvalidOperationException($"Expected 1 result but got {result.Data.EntityDefaults.Count}");

            var entity = result.Data.EntityDefaults.Single();
            BasicRobotDataModel robotDataModel = new BasicRobotDataModel
            {
                Definition = entity.Definition,
                DefinitionName = entity.DefinitionName,
                DescriptionToken = entity.DescriptionToken
            };
            // genxyReader.Deserialise<RobotOptionsGenxy>(result.Data.EntityDefaults[0].OptionsGenxy); Need to re-think this because killers can be things other than bots.

            return robotDataModel;
        }
    }
}
