using System.Linq;

namespace MusCat.Utils
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
        /// Quite fast method that capitalizes any string
        /// (also handles nulls transforming them to string.Empty)
        /// </summary>
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            var chars = s.ToLower().ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }
    }
}
