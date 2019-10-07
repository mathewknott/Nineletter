using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NineLetter.Web.Models;
using NineLetter.Web.Models.NineLetter;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        private readonly IHostingEnvironment _hosting;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsAccessor"></param>
        /// <param name="fileProvider"></param>
        /// <param name="memoryCache"></param>
        /// <param name="hosting"></param>
        public HomeController(IOptions<NineLetterConfig> optionsAccessor, IFileProvider fileProvider, IMemoryCache memoryCache, IHostingEnvironment hosting)
        {
            _optionsAccessor = optionsAccessor;
            _fileProvider = fileProvider;
            _memoryCache = memoryCache;
            _hosting = hosting;
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
            var patterns = Patterns().ToList();

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
                            LongestWord = pattern.Words.Last().Word
                        }
                    });
                }
            }

            var patternInputResult = patterns.FirstOrDefault(x => x.Pattern == patternInput);

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
                        LongestWord = patternInputResult.Words.Last().Word
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
        public IEnumerable<Result> Patterns()
        {
            var cacheKey = "Patterns";
            IEnumerable<Result> patterns;

            if (_memoryCache.TryGetValue(cacheKey, out patterns))
            {
                return (IEnumerable<Result>)_memoryCache.Get(cacheKey);
            }

            var r = new List<Result>();

            var patternList = PatternGenerator.GetPatterns(_optionsAccessor.Value.PatternsToGenerate);

            foreach (var pattern in patternList)
            {
                var wordResults = TextUtility.ProcessTextFile(_fileProvider.GetFileInfo(_optionsAccessor.Value.FileLocation).PhysicalPath, pattern.Pattern, _optionsAccessor.Value.MinLettersLength, _optionsAccessor.Value.IgnoreProperNouns, pattern.Pattern[4]);

                var results = new Result
                {
                    Pattern = pattern.Pattern
                };

                var enumerable = wordResults as IList<WordResult> ?? wordResults.ToList();

                if (enumerable.Any())
                {
                    results.Words = enumerable;
                    results.PossibleWords = enumerable.Count;
                    results.LongestWord = enumerable.Last()?.Word;
                }
                r.Add(results);
            }

            var l = r.ToList().OrderByDescending(p => p.PossibleWords).ToList();

            // keep item in cache as long as it is requested at least
            // once every 5 minutes...
            // but in any case make sure to refresh it every hour

            // store in the cache
            _memoryCache.Set(cacheKey, l, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5))
              .SetAbsoluteExpiration(TimeSpan.FromHours(1)));

            return l;
        }

        /// <summary>
        /// Returns all available patterns
        /// </summary>
        [ResponseCache(Duration = 3660, VaryByHeader = "User-Agent")]
        [Route("api/pj")]
        [HttpGet]
        public IActionResult Pj()
        {
            var tempList = JsonConvert.SerializeObject(Patterns());
            return tempList == null ? null : new ContentResult { Content = tempList, ContentType = "application/json" };
        }

        /// <summary>
        /// Validates a pattern.
        /// </summary>
        /// <param name="patternInput"></param>
        [HttpPost]
        [Route("api/validate/{patternInput}")]
        public IActionResult Validate(string patternInput)
        {
            if (patternInput == "" || patternInput.Length != 9)
            {
                ModelState.AddModelError("ValidatePattern", "The pattern cannot be validated.");
                var errorList = JsonConvert.SerializeObject(ModelState.Values.Where(x => x.Errors.Count > 0), Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Json(new { success = "fail", errorList });
            }

            var item = Patterns().FirstOrDefault(x => x.Pattern.Equals(patternInput, StringComparison.CurrentCultureIgnoreCase));

            var tempList = JsonConvert.SerializeObject(item);
            return tempList == null ? null : new ContentResult { Content = tempList, ContentType = "application/json" };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patternInput"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/validatepattern/")]
        public IActionResult ValidatePattern(string patternInput)
        {
            if (string.IsNullOrEmpty(patternInput) || patternInput.Length != 9)
            {
                ModelState.AddModelError("ValidatePattern", "The pattern cannot be validated.");
                var errorList = JsonConvert.SerializeObject(ModelState.Values.Where(x => x.Errors.Count > 0), Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Json(new { success = "fail", errorList });
            }

            var item = Patterns().FirstOrDefault(x => x.Pattern.Equals(patternInput, StringComparison.CurrentCultureIgnoreCase));

            if (item == null)
            {
                ModelState.AddModelError("ValidatePattern", "The pattern cannot be found.");
                var errorList = JsonConvert.SerializeObject(ModelState.Values.Where(x => x.Errors.Count > 0), Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Json(new { success = "fail", errorList });
            }

            return Json(patternInput);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public IActionResult Error(int? statusCode = null)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            ViewBag.ErrorUrl = feature?.OriginalPath;
            
            if (statusCode.HasValue)
            {
                if (statusCode == 404 || statusCode == 500)
                {
                    ViewBag.StatusCode = statusCode + " Error";

                    var viewName = statusCode + ".cshtml";
                    return View("~/Views/Shared/ErrorPages/" + viewName);
                }
            }
            return View("~/Views/Shared/ErrorPages/error.cshtml");
        }
    }
}