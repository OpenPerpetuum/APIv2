using OpenPerpetuum.Core.Foundation.Processing;
using System;

namespace OpenPerpetuum.Core.Killboard.Queries
{
    public class GAME_GetTodaysKillReportsQuery : IQuery<KillboardDataResultModel>
    {
        public DateTimeOffset TodayDate { get; set; }
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }
    }
}
