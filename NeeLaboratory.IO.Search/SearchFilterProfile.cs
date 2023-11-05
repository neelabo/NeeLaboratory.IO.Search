namespace NeeLaboratory.IO.Search
{
    public class SearchFilterProfile
    {
        public delegate SearchFilter CreateSearchOperationFunc(SearchPropertyProfile property, string format);

        public SearchFilterProfile(string name, CreateSearchOperationFunc createFunc)
        {
            Name = name;
            CreateFunc = createFunc;
        }

        public string Name { get; }

        public CreateSearchOperationFunc CreateFunc { get; }
    }


    public static class SearchFilterProfiles
    {
        public static SearchFilterProfile True { get; } = new SearchFilterProfile("true", (property, format) => new TrueSearchFilter(property, format));
        public static SearchFilterProfile Exact { get; } = new SearchFilterProfile("exact", (property, format) => new ExactSearchFilter(property, format));
        public static SearchFilterProfile Word { get; } = new SearchFilterProfile("word", (property, format) => new WordSearchFilter(property, format));
        public static SearchFilterProfile Fuzzy { get; } = new SearchFilterProfile("fuzzy", (property, format) => new FuzzySearchFilter(property, format));
        public static SearchFilterProfile RegularExpression { get; } = new SearchFilterProfile("re", (property, format) => new RegularExpressionSearchFilter(property, format));
        public static SearchFilterProfile RegularExpressionIgnoreCase { get; } = new SearchFilterProfile("ire", (property, format) => new RegularExpressionIgnoreCaseSearchFilter(property, format));

        public static SearchFilterProfile LessThan { get; } = new SearchFilterProfile("lt", (property, format) => new LessThanSearchFilter(property, format));
        public static SearchFilterProfile LessThanEqual { get; } = new SearchFilterProfile("le", (property, format) => new LessThanEqualSearchFilter(property, format));
        public static SearchFilterProfile Equal { get; } = new SearchFilterProfile("eq", (property, format) => new EqualSearchFilter(property, format));
        public static SearchFilterProfile NotEqual { get; } = new SearchFilterProfile("ne", (property, format) => new NotEqualSearchFilter(property, format));
        public static SearchFilterProfile GreaterThanEqual { get; } = new SearchFilterProfile("ge", (property, format) => new GreaterThanEqualSearchFilter(property, format));
        public static SearchFilterProfile GreaterThan { get; } = new SearchFilterProfile("gt", (property, format) => new GreaterThanSearchFilter(property, format));
    }

}
