using OpenPerpetuum.Core.DataServices.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Killboard.DatabaseResults
{
    internal class KillboardData
    {
        public Guid KillId { get; set; }
        public DateTime Date { get; set; }
        public string GenxyData { get; set; }
    }
    internal class KillboardResult : DatabaseResult
    {
        public ReadOnlyCollection<KillboardData> Kills => ((ResultSet<KillboardData>)Results[0])?.Data ?? new List<KillboardData>().AsReadOnly();

        public KillboardResult()
        {
            Results.Add(0, new ResultSet<KillboardData>(0));
        }
    }
}
