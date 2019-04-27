using OpenPerpetuum.Core.Killboard.DataModel;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Killboard
{
    public class KillboardDataResultModel
    {
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }
        public int ResultsReturned { get; set; }
        public ReadOnlyCollection<KillDataGenxy> KillData { get; set; }
    }
}
