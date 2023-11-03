namespace NeeLaboratory.IO.Search
{

    public class SearchValueCache
    {
        public static SearchValueCache Default { get; } = new();

        public FuzzyStringCache FuzzyStringCache { get; } = new FuzzyStringCache();
        public WordStringCache WordStringCache { get; } = new WordStringCache();
    }

    public class SearchValueContext
    {
        public static SearchValueContext Default { get; } = new();

        private SearchValueCache _cache;
        private SearchPropertyMap _searchPropertyMap = new();
        private SearchOperatorFactory _operatorFactory = new();


        // TODO: Default生成は別で行うべきだろう。依存が酷い。
        public SearchValueContext() : this(SearchValueCache.Default)
        {
            _searchPropertyMap.Add(new SearchPropertyProfile(StringSearchValue.DefaultPropertyName, StringSearchValue.Default));
            _searchPropertyMap.Add(new SearchPropertyProfile(DateTimeSearchValue.DefaultPropertyName, DateTimeSearchValue.Default));

            _operatorFactory.Add(SearchPattern.Standard.ToString(), FuzzySearchOperation.Create);
            _operatorFactory.Add(SearchPattern.Word.ToString(), WordSearchOperation.Create);
            _operatorFactory.Add(SearchPattern.Exact.ToString(), ExactSearchOperation.Create);
            _operatorFactory.Add(SearchPattern.RegularExpression.ToString(), RegularExpressionSearchOperation.Create);
            _operatorFactory.Add(SearchPattern.RegularExpressionIgnoreCase.ToString(), RegularExpressionIgnoreSearchOperation.Create);

            _operatorFactory.Add(SearchPattern.Since.ToString(), GraterThanSearchOperation.Create);
            _operatorFactory.Add(SearchPattern.Until.ToString(), LessThanSearchOperation.Create);
        }

        public SearchValueContext(SearchValueCache cache)
        {
            _cache = cache;
        }


        public FuzzyStringCache FuzzyStringCache => _cache.FuzzyStringCache;
        
        public WordStringCache WordStringCache => _cache.WordStringCache;


        public SearchOperation CreateSearchOperation(string filterName, string propertyName, string format)
        {
            return _operatorFactory.Create(filterName, this, propertyName, format);
        }

        public SearchValue CreateSearchValue(string propertyName, string format)
        {
            return _searchPropertyMap[propertyName].Parse(format);
        }
    }

}
