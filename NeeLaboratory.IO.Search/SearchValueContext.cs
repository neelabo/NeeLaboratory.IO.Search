using System.Linq;
using System.Runtime.CompilerServices;

namespace NeeLaboratory.IO.Search
{
    public class SearchValueContext
    {
        public static SearchValueContext Default { get; } = new();


        private SearchValueCache _cache;


        public SearchValueContext() : this(SearchValueCache.Default)
        {
        }

        public SearchValueContext(SearchValueCache cache)
        {
            _cache = cache;
        }

        public SearchOptionMap Options { get; } = new SearchOptionMap();
        public SearchOptionAliasMap OptionAlias { get; } = new SearchOptionAliasMap();

        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        public WordStringCache WordStringCache => _cache.WordStringCache;
    }
}
