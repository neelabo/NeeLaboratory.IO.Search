namespace NeeLaboratory.IO.Search
{
    public class FuzzyStringCache : StringCache
    {
        protected override string Convert(string s)
        {
            return StringUtils.ToNormalizedWord(s, true);
        }
    }
}
