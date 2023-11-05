namespace NeeLaboratory.IO.Search
{
    public class FuzzyStringCache : StringCache
    {
        protected override string Convert(string s)
        {
            return SearchStringTools.ToNormalizedWord(s, true);
        }
    }
}
