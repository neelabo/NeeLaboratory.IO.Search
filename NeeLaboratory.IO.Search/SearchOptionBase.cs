namespace NeeLaboratory.IO.Search
{
    public abstract class SearchOptionBase
    {
        protected SearchOptionBase(string option)
        {
            Name = option;
        }

        public string Name { get; }
    }

    /// <summary>
    /// 接続詞オプション
    /// </summary>
    public class ConjunctionSearchOption : SearchOptionBase
    {
        public ConjunctionSearchOption(string option, SearchConjunction conjunction) : base(option)
        {
            SearchConjunction = conjunction;
        }

        public SearchConjunction SearchConjunction { get; }
    }

    /// <summary>
    /// 検索オプション
    /// </summary>
    public class OperationSearchOption : SearchOptionBase
    {
        public OperationSearchOption(string option, SearchFilterProfile profile) : base(option)
        {
            Profile = profile;
        }

        public SearchFilterProfile Profile { get; }
    }

   /// <summary>
   /// プロパティオプション
   /// </summary>
    public class PropertySearchOption : SearchOptionBase
    {
        public PropertySearchOption(string option, SearchPropertyProfile profile) : base(option)
        {
            Profile = profile;
        }

        public SearchPropertyProfile Profile { get; }
    }
}
