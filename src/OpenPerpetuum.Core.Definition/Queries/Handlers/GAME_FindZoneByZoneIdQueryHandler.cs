using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Definition.DatabaseResults;
using OpenPerpetuum.Core.Definition.DataModel;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.Types;
using System;
using System.Linq;

namespace OpenPerpetuum.Core.Definition.Queries.Handlers
{
    internal class GAME_FindZoneByZoneIdQueryHandler : IQueryHandler<GAME_FindZoneByZoneIdQuery, ZoneDataModel>
    {
        private readonly IDatabaseProvider db;

        public GAME_FindZoneByZoneIdQueryHandler(IDataContext dataContext)
        {
            db = dataContext.GetDataContext("Game");
        }

        public ZoneDataModel Handle(GAME_FindZoneByZoneIdQuery query)
        {
            var result = db.ExecuteProcedure<ZoneResult>(
                "API.FindZoneByZoneId",
                new DbParameters
                {
                    { "ZoneId", query.ZoneId }
                });
            result.ValidateResult();

            if (!result.Data.Zones.Any())
                return null;

            if (result.Data.Zones.Count > 1)
                throw new InvalidOperationException($"Expect 1 result but got {result.Data.Zones.Count} results");

            var zone = result.Data.Zones.Single();

            return new ZoneDataModel
            {
                Active = zone.Active,
                CanTerraform = zone.Terraformable,
                Description = zone.Description,
                Enabled = zone.Enabled,
                Fertility = zone.Fertility,
                Height = zone.Height,
                Host = zone.Host,
                Id = zone.Id,
                Ip = zone.ZoneIp,
                IsInstance = zone.IsInstance,
                MaxBases = zone.MaxDockingBases,
                Name = zone.Name,
                Note = zone.Note,
                PlantAltitudeScale = zone.PlantAltitudeScale,
                PlantRuleSet = zone.PlantRuleSet,
                Plugin = zone.ZonePlugin,
                Port = zone.ZonePort,
                Position = new Position(zone.X, zone.Y, 0),
                Protected = zone.Protected,
                RaceId = zone.RaceId,
                Sleeping = zone.Sleeping,
                SparkCost = zone.SparkCost,
                SpawnId = zone.SpawnId,
                Type = zone.ZoneType,
                Width = zone.Width
            };
        }
    }
}
