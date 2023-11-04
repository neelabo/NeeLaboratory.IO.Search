namespace NeeLaboratory.IO.Search
{
    public class SearchOperatorProfile
    {
        public delegate SearchOperation CreateSearchOperationFunc(SearchPropertyProfile property, string format);

        public SearchOperatorProfile(string name, CreateSearchOperationFunc createFunc)
        {
            Name = name;
            CreateFunc = createFunc;
        }

        public string Name { get; }

        public CreateSearchOperationFunc CreateFunc { get; }
    }


    public static class SearchOperatorProfiles
    {
        public static SearchOperatorProfile TrueSearchOperationProfile { get; } = new SearchOperatorProfile("true", (property, format) => new TrueSearchOperation(property, format));
        public static SearchOperatorProfile ExactSearchOperationProfile { get; } = new SearchOperatorProfile("exact", (property, format) => new ExactSearchOperation(property, format));
        public static SearchOperatorProfile WordSearchOperationProfile { get; } = new SearchOperatorProfile("word", (property, format) => new WordSearchOperation(property, format));
        public static SearchOperatorProfile FuzzySearchOperationProfile { get; } = new SearchOperatorProfile("fuzzy", (property, format) => new FuzzySearchOperation(property, format));
        public static SearchOperatorProfile RegularExpressionSearchOperationProfile { get; } = new SearchOperatorProfile("re", (property, format) => new RegularExpressionSearchOperation(property, format));
        public static SearchOperatorProfile RegularExpressionIgnoreSearchOperationProfile { get; } = new SearchOperatorProfile("ire", (property, format) => new RegularExpressionIgnoreSearchOperation(property, format));
        public static SearchOperatorProfile GraterThanSearchOperationProfile { get; } = new SearchOperatorProfile("gt", (property, format) => new GraterThanSearchOperation(property, format));
        public static SearchOperatorProfile LessThanSearchOperationProfile { get; } = new SearchOperatorProfile("lt", (property, format) => new LessThanSearchOperation(property, format));
    }

}
