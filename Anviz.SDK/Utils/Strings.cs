using System.Text.RegularExpressions;

namespace Anviz.SDK.Utils
{
    static class Strings
    {
        public static string RemoveNonPrintableChars(string input)
        {
            // This pattern matches any character that is not a printable ASCII character
            string pattern = @"[^\x20-\x7E]";
            return Regex.Replace(input, pattern, string.Empty);
        }

    }
}
