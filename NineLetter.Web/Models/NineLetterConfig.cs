namespace NineLetter.Web.Models
{
    public class NineLetterConfig
    {
        public int MinLettersLength { get; set; }
        public int MinWordsInSolution { get; set; }
        public bool IgnoreProperNouns { get; set; }
        public string FileLocation { get; set; }
        public int PatternsToGenerate { get; set; }
        public int MinWordLength { get; set; }
        public int DataSource { get; set; }
    }
}
