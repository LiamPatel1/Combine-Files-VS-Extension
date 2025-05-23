using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CombineFilesVSExtension
{
    internal static class WildcardMatcher
    {
        private static string WildcardToRegex(string pattern)
        {
  
            return "^" + Regex.Escape(pattern)
                              .Replace("\\*", ".*")  // \* becomes .*
                              .Replace("\\?", ".")   // \? becomes .
                              + "$";
        }

        public static bool Matches(string pattern, string text)
        {
            if (string.IsNullOrEmpty(pattern)) return string.IsNullOrEmpty(text); // Match if both are null/empty
            if (pattern == "*") return true;

            // Use '/' for path separators
            string normalizedPattern = pattern.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
            string normalizedText = text.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

            bool isPathPattern = normalizedPattern.Contains("/");

            string textToMatchAgainstPattern;
            if (isPathPattern)
            {
                // If it's a path pattern, use the full text
                textToMatchAgainstPattern = normalizedText;
            }
            else
            {
                // If it's a filename pattern, extract the filename from the text
                textToMatchAgainstPattern = Path.GetFileName(normalizedText);
            }

            try
            {
                return Regex.IsMatch(textToMatchAgainstPattern, WildcardToRegex(normalizedPattern), RegexOptions.IgnoreCase);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WildcardMatcher] Regex error for pattern '{pattern}': {ex.Message}");
                return false;
            }
        }
    }
}