using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NineLetter.Web.Interfaces;
using NineLetter.Web.Models;

namespace NineLetter.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IOptions<NineLetterConfig> _optionsAccessor;
        private readonly IFileProvider _fileProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly INineLetterService _nineLetterService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsAccessor"></param>
        /// <param name="fileProvider"></param>
        /// <param name="memoryCache"></param>
        /// <param name="nineLetterService"></param>
        public HomeController(IOptions<NineLetterConfig> optionsAccessor, IFileProvider fileProvider, IMemoryCache memoryCache, INineLetterService nineLetterService)
        {
            _optionsAccessor = optionsAccessor;
            _fileProvider = fileProvider;
            _memoryCache = memoryCache;
            _nineLetterService = nineLetterService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patternInput"></param>
        /// <returns></returns>
        [Route("{patternInput:length(9)?}")]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index(string patternInput)
        {
            var patterns = Patterns().Result.ToList();

            if (string.IsNullOrEmpty(patternInput) || patternInput.Length != 9)
            {
                var pattern = patterns.FirstOrDefault(x => x.Words.Any());

                if (pattern != null)
                {
                    return View("Index", new BasePage
                    {
                        Result = new Result
                        {
                            Patterns = patterns,
                            Pattern = pattern.Pattern,
                            PossibleWords = pattern.PossibleWords,
                            Words = pattern.Words,
                            LongestWord = pattern.Words.Last()
                        }
                    });
                }
            }

            var patternInputResult = patterns.FirstOrDefault(x => x.Pattern.Equals(patternInput, StringComparison.CurrentCultureIgnoreCase));

            if (patternInputResult != null)
            {
                return View("Index", new BasePage
                {
                    Result = new Result
                    {
                        Patterns = patterns,
                        Pattern = patternInputResult.Pattern,
                        PossibleWords = patternInputResult.PossibleWords,
                        Words = patternInputResult.Words,
                        LongestWord = patternInputResult.Words.Last()
                    }
                });
            }

            return View("Index", new BasePage
            {
                Result = new Result
                {
                    Patterns = patterns,
                    Pattern = patternInput,
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Result>> PatternList()
        {
            if (_memoryCache.TryGetValue("Patterns", out IEnumerable<Result> _))
            {
                return (IEnumerable<Result>)_memoryCache.Get("Patterns");
            }

            var r = new List<Result>();

            var patternList = await _nineLetterService.GetPatterns(_optionsAccessor.Value.PatternsToGenerate);

            foreach (var pattern in patternList)
            {
                var wordResults = _nineLetterService.ProcessTextFile(
                    _fileProvider.GetFileInfo(_optionsAccessor.Value.FileLocation).PhysicalPath, 
                    pattern.Pattern, _optionsAccessor.Value.MinLettersLength,
                    _optionsAccessor.Value.IgnoreProperNouns,
                    pattern.Pattern[4])?.ToList();

                if (wordResults != null && wordResults.Any())
                {
                    var results = new Result
                    {
                        Pattern = pattern.Pattern,
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
        /// Returns all available patterns
        /// </summary>
        [Route("Patterns")]
        [HttpGet]
        public async Task<IEnumerable<Result>> Patterns()
        {
            return await PatternList();
        }

        /// <summary>
        /// Validates a pattern.
        /// </summary>
        /// <param name="patternInput"></param>
        [HttpGet]
        [Route("validate/ValidatePattern/{patternInput:length(9)?}")]
        public Result Validate(string patternInput)
        {
            if (patternInput == "" || patternInput.Length != 9)
            {
                return new Result();
            }

            var result = PatternList().Result.SingleOrDefault(x =>
                x.Pattern.Equals(patternInput, StringComparison.CurrentCultureIgnoreCase));

            return result ?? new Result();
        }
    }
}