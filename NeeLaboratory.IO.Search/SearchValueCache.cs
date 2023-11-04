namespace NeeLaboratory.IO.Search
{
    public class SearchValueCache
    {
        public static SearchValueCache Default { get; } = new();

        public FuzzyStringCache FuzzyStringCache { get; } = new FuzzyStringCache();
        public WordStringCache WordStringCache { get; } = new WordStringCache();
    }

}
