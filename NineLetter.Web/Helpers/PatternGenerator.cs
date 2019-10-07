using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NineLetter.Web.Models.NineLetter;

namespace NineLetter.Web
{
    static class PatternGenerator
    {
        private static readonly Random Rnd = new Random();

        public static IEnumerable<PatternResult> GetPatterns(int number)
        {
            var list = new List<PatternResult>();

            for (var i = 0; i < number; i++)
            {
                list.Add(new PatternResult { Pattern = GeneratePattern() });
            }

            return list;
        }

        private static string GeneratePattern()
        {
            var sb = new StringBuilder();
            sb.Append(RandomVowels(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            sb.Append(RandomVowels(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            sb.Append(RandomConsonants(Rnd, 1));
            return sb.ToString();
        }

        private static char[] RandomConsonants(Random random, int length)
        {
            const string chars = "BCDFGHJKLMNPQRSTVWXYZ";
            return Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray();
        }

        private static char[] RandomVowels(Random random, int length)
        {
            const string chars = "AEIOU";
            return Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray();
        }

        public static bool IsResult(string s1, string s2)
        {
            var oLength = s1.Length;

            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return false;
            }

            foreach (var c in s2)
            {
                var ix = s1.IndexOf(c);
                if (ix >= 0)
                {
                    s1 = s1.Remove(ix, 1);
                }
                else
                {
                    return false;
                }
            }

            return oLength == s1.Length + s2.Length;
        }
    }
}