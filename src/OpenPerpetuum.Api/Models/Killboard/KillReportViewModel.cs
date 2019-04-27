using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Models.Killboard
{
    public class KillReportViewModel
    {
        public string ZoneName { get; set; }

        public BasicCharacterViewModel Victim { get; set; }

        public ReadOnlyCollection<KillAttackerViewModel> Attackers { get; set; }
    }
}
