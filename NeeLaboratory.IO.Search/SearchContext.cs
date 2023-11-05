using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.IO.Search
{
    public class SearchContext
    {
        public static SearchContext Default { get; } = new();


        private SearchValueCache _cache;


        public SearchContext() : this(SearchValueCache.Default)
        {
            AddProfile(new DefaultSearchProfile());
        }

        public SearchContext(SearchValueCache cache)
        {
            _cache = cache;
        }


        public SearchKeyOptionMap KeyOptions { get; } = new SearchKeyOptionMap();
        public SearchKeyOptionAliasMap KeyOptionAlias { get; } = new SearchKeyOptionAliasMap();

        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        public WordStringCache WordStringCache => _cache.WordStringCache;


        public void AddProfile(SearchProfile profile)
        {
            KeyOptions.AddRange(profile.Options);
            KeyOptionAlias.AddRange(profile.Alias);
        }
    }
}
