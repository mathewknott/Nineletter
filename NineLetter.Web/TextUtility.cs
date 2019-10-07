using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NineLetter.Web.Models.NineLetter;

namespace NineLetter.Web
{
    internal class TextUtility
    {
        public static IEnumerable<WordResult> ProcessTextFile(string fileLocation, string pattern, int ignoreLessThan, bool ignoreProperNouns, char midChar)
        {
            var result = new List<WordResult>();

            foreach (var word in System.IO.File.ReadLines(fileLocation))
            {
                if (word.Length < ignoreLessThan) { continue; }

                if (!PatternGenerator.IsResult(pattern.ToUpper(), word.ToUpper())) continue;

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
                    result.Add(new WordResult { Word = word });
                }
            }

            return SortByLength(result);

        }
        
        private static IEnumerable<WordResult> SortByLength(IEnumerable<WordResult> e)
        {
            var sorted = from s in e
                         orderby s.Word.Length ascending
                         select s;
            return sorted;
        }
    }
}