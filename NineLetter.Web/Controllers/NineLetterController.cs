using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NineLetter.Web.Interfaces;
using NineLetter.Web.Models;

namespace NineLetter.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>

    public class NineLetterController : ControllerBase
    {
        private readonly INineLetterService _nineLetterService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nineLetterService"></param>
        public NineLetterController(INineLetterService nineLetterService)
        {
            _nineLetterService = nineLetterService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("nineletter/PatternList/")]
        public async Task<IEnumerable<Result>> PatternList()
        {
            return await _nineLetterService.GetPatternResult();
        }
        
        /// <summary>
        /// Validates a pattern.
        /// </summary>
        /// <param name="patternInput"></param>
        [HttpGet]
        [Route("nineletter/{patternInput:length(9)?}")]
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