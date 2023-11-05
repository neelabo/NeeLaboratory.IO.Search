using System.Linq;
using System.Runtime.CompilerServices;

namespace NeeLaboratory.IO.Search
{
    public class SearchContext
    {
        public static SearchContext Default { get; } = new();


        private SearchValueCache _cache;


        public SearchContext() : this(SearchValueCache.Default)
        {
        }

        public SearchContext(SearchValueCache cache)
        {
            _cache = cache;
        }

        public SearchOptionMap Options { get; } = new SearchOptionMap();
        public SearchOptionAliasMap OptionAlias { get; } = new SearchOptionAliasMap();

        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        public WordStringCache WordStringCache => _cache.WordStringCache;
    }
}
