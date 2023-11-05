namespace NeeLaboratory.IO.Search
{
    public class WordStringCache : StringCache
    {
        protected override string Convert(string s)
        {
            return SearchStringTools.ToNormalizedWord(s, false);
        }
    }
}
