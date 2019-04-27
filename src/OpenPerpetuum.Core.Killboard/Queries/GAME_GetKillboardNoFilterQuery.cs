using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Core.Killboard.Queries
{
    public class GAME_GetKillboardNoFilterQuery : IQuery<KillboardDataResultModel>
    {
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }
    }
}
