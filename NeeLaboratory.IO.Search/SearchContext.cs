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


        public SearchKeyOptionCollection KeyOptions { get; } = new SearchKeyOptionCollection();
        public SearchKeyAliasCollection KeyAlias { get; } = new SearchKeyAliasCollection();

        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        public WordStringCache WordStringCache => _cache.WordStringCache;


        public void AddProfile(SearchProfile profile)
        {
            KeyOptions.AddRange(profile.Options);
            KeyAlias.AddRange(profile.Alias);
        }
    }
}
