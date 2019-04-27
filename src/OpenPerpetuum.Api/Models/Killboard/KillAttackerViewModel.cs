namespace OpenPerpetuum.Api.Models.Killboard
{
    public class KillAttackerViewModel
    {
        public BasicCharacterViewModel Attacker { get; set; }
        public double DamageDone { get; set; }
        public bool KillingBlow { get; set; }
        public int JammerTotalCycles { get; set; }
        public int JammerSuccessfulCycles { get; set; }
        public int DemobilizerCycles { get; set; }
        public int SuppressorCycles { get; set; }
        public double Dispersion { get; set; }
    }
}
 