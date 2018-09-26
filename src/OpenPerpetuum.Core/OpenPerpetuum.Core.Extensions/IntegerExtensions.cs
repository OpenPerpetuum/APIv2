namespace OpenPerpetuum.Core.Extensions
{
    public static class IntegerExtensions
    {
        public static int MegaBytes(this int number)
        {
            return number * 1024 * 1024;
        }
    }
}
