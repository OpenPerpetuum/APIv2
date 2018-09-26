namespace OpenPerpetuum.Core.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToEnabledString(this bool input)
        {
            return input ? "enabled" : "disabled";
        }
    }
}
