using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.IO.Search
{
    public class SearchContext
    {
        private SearchValueCache _cache;


        public SearchContext() : this(new SearchValueCache())
        {
        }

        public SearchContext(SearchValueCache cache)
        {
            _cache = cache;
            AddProfile(new DefaultSearchProfile());
        }


        public SearchKeyOptionCollection KeyOptions { get; } = new SearchKeyOptionCollection();
        public SearchKeyAliasCollection KeyAlias { get; } = new SearchKeyAliasCollection();

        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        public WordStringCache WordStringCache => _cache.WordStringCache;


        public SearchContext AddProfile(SearchProfile profile)
        {
            KeyOptions.AddRange(profile.Options);
            KeyAlias.AddRange(profile.Alias);
            return this;
        }
    }
}
