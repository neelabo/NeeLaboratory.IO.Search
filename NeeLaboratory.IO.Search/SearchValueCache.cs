namespace NeeLaboratory.IO.Search
{
    public class SearchValueCache
    {
        public SearchValueCache() : this(new FuzzyStringCache(), new WordStringCache())
        {
        }

        public SearchValueCache(FuzzyStringCache fuzzyStringCache, WordStringCache wordStringCache)
        {
            FuzzyStringCache = fuzzyStringCache;
            WordStringCache = wordStringCache;
        }

        public FuzzyStringCache FuzzyStringCache { get; } = new FuzzyStringCache();
        public WordStringCache WordStringCache { get; } = new WordStringCache();
    }

}
