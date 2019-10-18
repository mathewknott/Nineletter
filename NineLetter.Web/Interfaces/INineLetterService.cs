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
        /// <returns></returns>
        Task<IEnumerable<Result>> GetPatternResult();
        
    }
}