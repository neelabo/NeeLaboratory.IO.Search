namespace NeeLaboratory.IO.Search
{
    public enum SearchConjunction
    {
        /// <summary>
        /// AND接続
        /// </summary>
        And,

        /// <summary>
        /// OR接続
        /// </summary>
        Or,

        /// <summary>
        /// NOT接続
        /// </summary>
        Not,
    }

    public enum SearchPattern
    {
        /// <summary>
        /// 完全一致 (m0)
        /// </summary>
        Exact,
        
        /// <summary>
        /// 単語一致 (m1)
        /// </summary>
        Word,

        /// <summary>
        /// あいまい一致 (m2)
        /// </summary>
        Standard,

        /// <summary>
        /// 正規表現
        /// </summary>
        RegularExpression,

        /// <summary>
        /// 正規表現 (IgnoreCase)
        /// </summary>
        RegularExpressionIgnoreCase,

        /// <summary>
        /// 日時検索、開始日時
        /// </summary>
        Since,

        /// <summary>
        /// 日時検索、終了日時
        /// </summary>
        Until,

        /// <summary>
        /// 未定義 (解析用)
        /// </summary>
        Undefined = -1,
    }

    /// <summary>
    /// 検索キー
    /// </summary>
    public class SearchKey
    {
        public SearchKey(string word)
        {
            Word = word;
        }

        public SearchKey(string word, SearchConjunction conjunction, SearchPattern pattern) : this(word)
        {
            Conjunction = conjunction;
            Pattern = pattern;
        }


        /// <summary>
        /// 検索単語
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// 接続詞
        /// </summary>
        public SearchConjunction Conjunction { get; set; }

        /// <summary>
        /// 適応パターン
        /// </summary>
        public SearchPattern Pattern { get; set; } = SearchPattern.Standard;



        public SearchKey Clone()
        {
            return (SearchKey)this.MemberwiseClone();
        }

        public override bool Equals(object? other)
        {
            if (other is SearchKey target)
            {
                return this.Word == target.Word
                    && this.Conjunction == target.Conjunction
                    && this.Pattern == target.Pattern;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Conjunction},{Pattern},\"{Word}\"";
        }
    }
}
