using OpenPerpetuum.Core.Killboard.DataModel;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Killboard
{
    public class KillboardDataModel
    {
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }
        public ReadOnlyCollection<KillDataGenxy> KillData { get; set; }
    }
}
