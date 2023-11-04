namespace NeeLaboratory.IO.Search
{
    public class SearchPropertyProfile
    {
        public SearchPropertyProfile(string name, SearchValue defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public SearchValue DefaultValue { get; }

        public SearchValue Parse(string format)
        {
            return DefaultValue.Parse(format);
        }
    }


    public static class SearchPropertyProfiles
    {
        public static SearchPropertyProfile TextPropertyProfile { get; } = new SearchPropertyProfile("text", StringSearchValue.Default);
        public static SearchPropertyProfile DatePropertyProfile { get; } = new SearchPropertyProfile("date", DateTimeSearchValue.Default);

        // TODO: 基本セット以外は外部で定義するように
        public static SearchPropertyProfile IsDirectoryPropertyProfile { get; } = new SearchPropertyProfile("isdir", BooleanSearchValue.Default);
        public static SearchPropertyProfile IsPinnedPropertyProfile { get; } = new SearchPropertyProfile("ispinned", BooleanSearchValue.Default);
    }
}
