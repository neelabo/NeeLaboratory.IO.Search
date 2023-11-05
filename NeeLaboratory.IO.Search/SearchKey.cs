namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索キー
    /// </summary>
    public class SearchKey
    {
        public SearchKey(SearchConjunction conjunction, SearchFilterProfile filter, SearchPropertyProfile property, string format)
        {
            Conjunction = conjunction;
            Filter = filter;
            Property = property;
            Format = format;
        }


        /// <summary>
        /// 接続詞
        /// </summary>
        public SearchConjunction Conjunction { get; set; }

        /// <summary>
        /// 検索フィルター
        /// </summary>
        public SearchFilterProfile Filter { get; set; }

        /// <summary>
        /// 検索対象プロパティ
        /// </summary>
        public SearchPropertyProfile Property { get; set; }

        /// <summary>
        /// 検索フォーマット
        /// </summary>
        public string Format { get; set; }


        public SearchKey Clone()
        {
            return (SearchKey)this.MemberwiseClone();
        }

        public override bool Equals(object? other)
        {
            if (other is SearchKey target)
            {
                return this.Format == target.Format
                    && this.Conjunction == target.Conjunction
                    && this.Filter == target.Filter
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
            return $"{Conjunction},{Filter},\"{Format}\",{Property}";
        }
    }


}
