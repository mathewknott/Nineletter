using System.Collections.Generic;
using NineLetter.Web.Models;
using System.Threading.Tasks;

namespace NineLetter.Web.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface INineLetterService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        Task<IEnumerable<PatternResult>> GetPatterns(int number);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <param name="pattern"></param>
        /// <param name="ignoreLessThan"></param>
        /// <param name="ignoreProperNouns"></param>
        /// <param name="midChar"></param>
        /// <returns></returns>
        IEnumerable<string> ProcessTextFile(string fileLocation, string pattern, int ignoreLessThan,
            bool ignoreProperNouns, char midChar);
    }
}