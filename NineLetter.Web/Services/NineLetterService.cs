using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NineLetter.Web.Interfaces;
using NineLetter.Web.Models;

namespace NineLetter.Web.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class NineLetterService : INineLetterService
    {
        private readonly Random _rnd = new Random();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PatternResult>> GetPatterns(int number)
        {
            var list = new List<PatternResult>();

            for (var i = 0; i < number; i++)
            {
                list.Add(new PatternResult
                {
                    Pattern = GeneratePattern()
                });
            }

            return await Task.FromResult(list);
        }

        private string GeneratePattern()
        {
            var sb = new StringBuilder();
            sb.Append(RandomVowels(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
            sb.Append(RandomVowels(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
            sb.Append(RandomConsonants(_rnd, 1));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <param name="pattern"></param>
        /// <param name="ignoreLessThan"></param>
        /// <param name="ignoreProperNouns"></param>
        /// <param name="midChar"></param>
        /// <returns></returns>
        public IEnumerable<string> ProcessTextFile(string fileLocation, string pattern, int ignoreLessThan, bool ignoreProperNouns, char midChar)
        {
            var result = new List<string>();

            foreach (var word in System.IO.File.ReadLines(fileLocation))
            {
                if (word.Length < ignoreLessThan) { continue; }

                if (!IsResult(pattern.ToUpper(), word.ToUpper())) continue;

                if (CultureInfo.CurrentUICulture.CompareInfo.IndexOf(word, midChar, CompareOptions.IgnoreCase) <= 0)
                {
                    continue;
                }

                var add = true;

                if (ignoreProperNouns)
                {
                    if (char.IsUpper(word[0]))
                    {
                        add = false;
                    }
                }

                if (add)
                {
                    result.Add(word);
                }
            }

            return SortByLength(result);

        }

        private static IEnumerable<string> SortByLength(IEnumerable<string> e)
        {
            var sorted = from s in e
                orderby s.Length
                select s;
            return sorted;
        }
        
        private static bool IsResult(string s1, string s2)
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