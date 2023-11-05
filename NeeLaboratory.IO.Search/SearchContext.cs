using System.Collections.Generic;
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


        public SearchKeyOptionMap KeyOptions { get; } = new SearchKeyOptionMap();
        public SearchKeyOptionAliasMap KeyOptionAlias { get; } = new SearchKeyOptionAliasMap();

        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        public WordStringCache WordStringCache => _cache.WordStringCache;
    }
}
