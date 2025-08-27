using System.Globalization;

namespace Luminance.Helpers
{
    //TODO: Worry about this later.
    public static class InvariantCultureHelper
    {
        public static string ToInvariantString(this decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);

        public static decimal ParseDecimal(string input) =>
            decimal.Parse(input, CultureInfo.InvariantCulture);

        public static bool TryParseDecimal(string input, out decimal result) =>
            decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
