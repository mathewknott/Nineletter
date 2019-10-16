using System.Collections.Generic;

namespace NineLetter.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 
        /// </summary>
        public Result()
        {
            Words = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Result> Patterns { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Words { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PossibleWords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LongestWord { get; set; }
    }
}