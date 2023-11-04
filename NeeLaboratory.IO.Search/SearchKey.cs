namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索キー
    /// </summary>
    public class SearchKey
    {
        public SearchKey(string word, SearchConjunction conjunction, SearchOperatorProfile pattern, SearchPropertyProfile property)
        {
            Word = word;
            Conjunction = conjunction;
            Pattern = pattern;
            Property = property;
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
        public SearchOperatorProfile Pattern { get; set; }

        /// <summary>
        /// 検索対象プロパティ
        /// </summary>
        public SearchPropertyProfile Property { get; set; }



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
                    && this.Pattern == target.Pattern
                    && this.Property == target.Property;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Conjunction},{Pattern},\"{Word}\",{Property}";
        }
    }


}
