using System.Linq;

namespace MusCat.Services
{
    static class ParserStringUtils
    {
        /// <summary>
        /// Extension method that leaves only digits in the string (without regex)
        /// </summary>
        public static string Digits(this string s)
        {
            return new string(s.Where(char.IsDigit)
                               .Select(c => c)
                               .ToArray());
        }

        /// <summary>
        /// Quite fast extension method that capitalizes any string
        /// (also handles nulls transforming them to string.Empty)
        /// </summary>
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            var chars = s.ToLower().ToCharArray();
            for (var i = 0; i < s.Length; i++)
            {
                if (char.IsLetter(s[i]))
                {
                    chars[i] = char.ToUpper(s[i]);
                    break;
                }
            }
            return new string(chars);
        }

        /// <summary>
        /// Extension method detects if space at position 'idx' is between letters (including '\'', '-' and '&')
        /// </summary>
        public static bool SpaceSurroundedByLetters(this string s, int idx)
        {
            if (s[idx] != ' ')
            {
                return false;
            }

            if ((char.IsLetterOrDigit(s[idx - 1]) || LetterSymbols.Contains(s[idx - 1])) &&
                (char.IsLetterOrDigit(s[idx + 1]) || LetterSymbols.Contains(s[idx + 1])))
            {
                return false;
            }

            return true;
        }

        private static readonly char[] LetterSymbols = {'\'', '"', '-', '&'};
    }
}
