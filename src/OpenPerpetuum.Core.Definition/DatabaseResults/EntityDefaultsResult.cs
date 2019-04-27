using OpenPerpetuum.Core.DataServices.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OpenPerpetuum.Core.Definition.DatabaseResults
{
    internal class EntityDefaultsDbData
    {
        public int Definition { get; set; }
        public string DefinitionName { get; set; }
        public int Quantity { get; set; }
        public long AttributeFlags { get; set; }
        public long CategoryFlags { get; set; }
        public string OptionsGenxy { get; set; }
        public string Note { get; set; }
        public bool Enabled { get; set; }
        public double Volume { get; set; }
        public double Mass { get; set; }
        public bool Hidden { get; set; }
        public double Health { get; set; }
        public string DescriptionToken { get; set; }
        public bool Purchasable { get; set; }
        public int TierType { get; set; }
        public int TierLevel { get; set; }
    }

    internal class EntityDefaultsResult : DatabaseResult
    {
        public ReadOnlyCollection<EntityDefaultsDbData> EntityDefaults => ((ResultSet<EntityDefaultsDbData>)Results[0])?.Data ?? new List<EntityDefaultsDbData>().AsReadOnly();

        public EntityDefaultsResult()
        {
            Results.Add(0, new ResultSet<EntityDefaultsDbData>(0));
        }
    }
}
