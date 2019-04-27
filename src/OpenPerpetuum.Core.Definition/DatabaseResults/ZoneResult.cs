using OpenPerpetuum.Core.DataServices.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OpenPerpetuum.Core.Definition.DatabaseResults
{
    internal class ZoneResultModel
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public int Fertility { get; set; }
        public string ZonePlugin { get; set; }
        public string ZoneIp { get; set; }
        public int ZonePort { get; set; }
        public bool IsInstance { get; set; }
        public bool Enabled { get; set; }
        public int SpawnId { get; set; }
        public int PlantRuleSet { get; set; }
        public bool Protected { get; set; }
        public int RaceId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Terraformable { get; set; }
        public int ZoneType { get; set; } // Would be nice if we had a descriptive enum for this
        public int SparkCost { get; set; }
        public int MaxDockingBases { get; set; }
        public bool Sleeping { get; set; }
        public double PlantAltitudeScale { get; set; }
        public string Host { get; set; }
        public bool Active { get; set; }
    }
    internal class ZoneResult : DatabaseResult
    {
        public ReadOnlyCollection<ZoneResultModel> Zones => ((ResultSet<ZoneResultModel>)Results[0])?.Data ?? new List<ZoneResultModel>().AsReadOnly();

        public ZoneResult()
        {
            Results.Add(0, new ResultSet<ZoneResultModel>(0));
        }
    }
}
