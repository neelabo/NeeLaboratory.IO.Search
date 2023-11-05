using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeeLaboratory.IO.Search
{
    public class SearchKeyOptionMap
    {
        private Dictionary<string, SearchKeyOption> _map;

        public SearchKeyOptionMap()
        {
            _map = new();
            Add(SearchConjunction.And);
            Add(SearchConjunction.Or);
            Add(SearchConjunction.Not);

            // TODO: 基本文字列検索セット以外は拡張セットとして外部から追加するように

            Add(SearchPropertyProfiles.Text);
            Add(SearchPropertyProfiles.Date);
            Add(SearchPropertyProfiles.IsDirectory);
            Add(SearchPropertyProfiles.IsPinned);

            Add(SearchFilterProfiles.Exact);
            Add(SearchFilterProfiles.Word);
            Add(SearchFilterProfiles.Fuzzy);
            Add(SearchFilterProfiles.RegularExpression);
            Add(SearchFilterProfiles.RegularExpressionIgnoreCase);

            Add(SearchFilterProfiles.LessThan);
            Add(SearchFilterProfiles.LessThanEqual);
            Add(SearchFilterProfiles.Equal);
            Add(SearchFilterProfiles.NotEqual);
            Add(SearchFilterProfiles.GreaterThanEqual);
            Add(SearchFilterProfiles.GreaterThan);
        }

        public SearchKeyOption this[string key]
        {
            get { return _map[key]; }
            set { _map[key] = value; }
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out SearchKeyOption value)
        {
            return _map.TryGetValue(key, out value);
        }


        public void Add(SearchConjunction conjunction)
        {
            var option = new ConjunctionSearchKeyOption("/c." + conjunction.ToString().ToLower(), conjunction);
            _map.Add(option.Name, option);
        }

        public void Add(SearchPropertyProfile profile)
        {
            var option = new PropertySearchKeyOption("/p." + profile.Name, profile);
            _map.Add(option.Name, option);
        }

        public void Add(SearchFilterProfile profile)
        {
            var option = new FilterSearchKeyOption("/m." + profile.Name, profile);
            _map.Add(option.Name, option);
        }
    }

}
