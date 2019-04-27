using System.Collections.ObjectModel;

namespace OpenPerpetuum.Api.Models.Killboard
{
    public class GetKillboardResponseViewModel
    {
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }
        public int ResultsReturned { get; set; }

        public ReadOnlyCollection<KillReportViewModel> KillReports { get; set; }
    }
}
