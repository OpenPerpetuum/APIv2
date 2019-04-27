using OpenPerpetuum.Core.Foundation.Types;

namespace OpenPerpetuum.Core.Definition.DataModel
{
    public class ZoneDataModel
    {
        public int Id { get; set; }
        public Position Position { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public int Fertility { get; set; }
        public string Plugin { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool IsInstance { get; set; }
        public bool Enabled { get; set; }
        public int SpawnId { get; set; }
        public int PlantRuleSet { get; set; }
        public bool Protected { get; set; }
        public int RaceId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool CanTerraform { get; set; }
        public int Type { get; set; } // Would be nice if we had a descriptive enum for this
        public int SparkCost { get; set; }
        public int MaxBases { get; set; }
        public bool Sleeping { get; set; }
        public double PlantAltitudeScale { get; set; }
        public string Host { get; set; }
        public bool Active { get; set; }
    }
}
