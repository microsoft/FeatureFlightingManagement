using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Extensions to string
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces ann non-ascii characters from a string
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="replacement">Replacement stirng</param>
        /// <returns>Stirng with non-ascii characters removed</returns>
        /// <remarks>https://stackoverflow.com/questions/123336/how-can-you-strip-non-ascii-characters-from-a-string-in-c</remarks>
        public static string RemoveNonAscii(this string source, string replacement = "")
        {   
            string asciiRangeExpr = "\u0020-\u007E"; // https://www.asciitable.com/
            string nonAsciiRangeExpr = $"^{asciiRangeExpr}";
            return Regex.Replace(source, $@"[{nonAsciiRangeExpr}]", replacement);
        }

        /// <summary>
        /// Removes all characters except ; \ -
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="replacement">Replacement character</param>
        /// <returns>String without special characters</returns>
        public static string RemoveSpecialCharacters(this string source, string replacement = "")
        {
            string characterRangeExpr = @"a-zA-Z0-9\\:_\- ";
            string specialCharacterRangeExpre = $"^{characterRangeExpr}";
            return Regex.Replace(source, $@"[{specialCharacterRangeExpre}]", replacement);
        }
    }
}
