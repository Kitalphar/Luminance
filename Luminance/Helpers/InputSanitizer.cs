using System.Text.RegularExpressions;

namespace Luminance.Helpers
{
    public static class InputSanitizer
    {
        private static readonly Regex UsernameRegex = new(@"^[a-z0-9_-]{1,30}$", RegexOptions.Compiled);
        public static (bool IsValid, string Result, string? Error) SanitizeUsername(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, "", "ERR_USERNAME_ISNULL(201)");

            if (input.Length > 30)
                return (false, "", "ERR_USERNAME_IS_LONG(202)");

            string sanitizedText = input.Trim().ToLowerInvariant();

            if (!UsernameRegex.IsMatch(sanitizedText))
                return (false, "", "ERR_USERNAME_HAS_OPERATORS(203)");

            return (true, sanitizedText, null);
        }

        public static (bool IsValid, string Result, string? Error) SanitizePassword(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, "", "ERR_PW_IS_NULL(210)");

            if (input.Length < 8)
                return (false, "", "ERR_PW_IS_SHORT(211)");

            if (input.Length > 30)
                return (false, "", "ERR_PW_IS_LONG(212)");

            return (true, input, null);
        }
    }
}
