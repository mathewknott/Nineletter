using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NineLetter.Web.Interfaces;
using NineLetter.Web.Models;

namespace NineLetter.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        private readonly INineLetterService _nineLetterService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nineLetterService"></param>
        public HomeController(INineLetterService nineLetterService)
        {
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
            var patterns = _nineLetterService.GetPatternResult().Result.ToList();

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
    }
}