namespace NeeLaboratory.IO.Search
{
    public class SearchFilterProfile
    {
        public delegate SearchFilter CreateSearchOperationFunc(SearchPropertyProfile property, string? propertyParameter, string format);

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
        public static SearchFilterProfile True { get; } = new SearchFilterProfile("true", (property, parameter, format) => new TrueSearchFilter(property, parameter, format));
        public static SearchFilterProfile Exact { get; } = new SearchFilterProfile("exact", (property, parameter, format) => new ExactSearchFilter(property, parameter, format));
        public static SearchFilterProfile Word { get; } = new SearchFilterProfile("word", (property, parameter, format) => new WordSearchFilter(property, parameter, format));
        public static SearchFilterProfile Fuzzy { get; } = new SearchFilterProfile("fuzzy", (property, parameter, format) => new FuzzySearchFilter(property, parameter, format));
        public static SearchFilterProfile RegularExpression { get; } = new SearchFilterProfile("re", (property, parameter, format) => new RegularExpressionSearchFilter(property, parameter, format));
        public static SearchFilterProfile RegularExpressionIgnoreCase { get; } = new SearchFilterProfile("ire", (property, parameter, format) => new RegularExpressionIgnoreCaseSearchFilter(property, parameter, format));

        public static SearchFilterProfile LessThan { get; } = new SearchFilterProfile("lt", (property, parameter, format) => new LessThanSearchFilter(property, parameter, format));
        public static SearchFilterProfile LessThanEqual { get; } = new SearchFilterProfile("le", (property, parameter, format) => new LessThanEqualSearchFilter(property, parameter, format));
        public static SearchFilterProfile Equal { get; } = new SearchFilterProfile("eq", (property, parameter, format) => new EqualSearchFilter(property, parameter, format));
        public static SearchFilterProfile NotEqual { get; } = new SearchFilterProfile("ne", (property, parameter, format) => new NotEqualSearchFilter(property, parameter, format));
        public static SearchFilterProfile GreaterThanEqual { get; } = new SearchFilterProfile("ge", (property, parameter, format) => new GreaterThanEqualSearchFilter(property, parameter, format));
        public static SearchFilterProfile GreaterThan { get; } = new SearchFilterProfile("gt", (property, parameter, format) => new GreaterThanSearchFilter(property, parameter, format));
    }

}
