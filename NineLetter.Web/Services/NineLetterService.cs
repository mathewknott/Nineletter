using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NineLetter.Web.Interfaces;
using NineLetter.Web.Models;

namespace NineLetter.Web.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class NineLetterService : INineLetterService
    {
        private readonly IOptions<NineLetterConfig> _optionsAccessor;
        private readonly IFileProvider _fileProvider;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsAccessor"></param>
        /// <param name="fileProvider"></param>
        /// <param name="memoryCache"></param>
        public NineLetterService(IOptions<NineLetterConfig> optionsAccessor, IFileProvider fileProvider, IMemoryCache memoryCache)
        {
            _optionsAccessor = optionsAccessor;
            _fileProvider = fileProvider;
            _memoryCache = memoryCache;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Result>> GetPatternResult()
        {
            if (_memoryCache.TryGetValue("Patterns", out IEnumerable<Result> _))
            {
                return (IEnumerable<Result>)_memoryCache.Get("Patterns");
            }

            var r = new List<Result>();

            var patternList = await GetPatterns(_optionsAccessor.Value.PatternsToGenerate);

            foreach (var pattern in patternList)
            {
                var wordResults = ProcessTextFile(
                    _fileProvider.GetFileInfo(_optionsAccessor.Value.FileLocation).PhysicalPath,
                    pattern, _optionsAccessor.Value.MinLettersLength,
                    _optionsAccessor.Value.IgnoreProperNouns,
                    pattern[4])?.ToList();

                if (wordResults != null && wordResults.Any())
                {
                    var results = new Result
                    {
                        Pattern = pattern,
                        Words = wordResults,
                        PossibleWords = wordResults.Count,
                        LongestWord = wordResults.Last()
                    };

                    r.Add(results);
                }
            }

            var l = r.OrderByDescending(p => p.PossibleWords).ToList();

            // keep item in cache as long as it is requested at least
            // once every 5 minutes...
            // but in any case make sure to refresh it every hour

            // store in the cache
            _memoryCache.Set("Patterns", l, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)));

            return l;
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
        private IEnumerable<string> ProcessTextFile(string fileLocation, string pattern, int ignoreLessThan, bool ignoreProperNouns, char midChar)
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

            return from s in result
                orderby s.Length
                select s;
        }

        private readonly Random _rnd = new Random();

        /// <summary>
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetPatterns(int number)
        {
            var list = new List<string>();

            for (var i = 0; i < number; i++)
            {
                list.Add(GeneratePattern());
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
        
        private static bool IsResult(string word1, string word2)
        {
            var oLength = word1.Length;

            if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2))
            {
                return false;
            }

            foreach (var character in word2)
            {
                var ix = word1.IndexOf(character);

                if (ix >= 0)
                {
                    word1 = word1.Remove(ix, 1);
                }
                else
                {
                    return false;
                }
            }

            return oLength == word1.Length + word2.Length;
        }
    }
}