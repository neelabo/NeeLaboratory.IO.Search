using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeeLaboratory.IO.Search
{
    public class SearchOptionMap
    {
        private Dictionary<string, SearchOptionBase> _map;

        public SearchOptionMap()
        {
            _map = new();
            Add(SearchConjunction.And);
            Add(SearchConjunction.Or);
            Add(SearchConjunction.Not);

            Add(SearchPropertyProfiles.TextPropertyProfile);
            Add(SearchPropertyProfiles.DatePropertyProfile);

            Add(SearchOperatorProfiles.ExactSearchOperationProfile);
            Add(SearchOperatorProfiles.WordSearchOperationProfile);
            Add(SearchOperatorProfiles.FuzzySearchOperationProfile);
            Add(SearchOperatorProfiles.RegularExpressionSearchOperationProfile);
            Add(SearchOperatorProfiles.RegularExpressionIgnoreSearchOperationProfile);
            Add(SearchOperatorProfiles.GraterThanSearchOperationProfile);
            Add(SearchOperatorProfiles.LessThanSearchOperationProfile);
        }

        public SearchOptionBase this[string key]
        {
            get { return _map[key]; }
            set { _map[key] = value; }
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out SearchOptionBase value)
        {
            return _map.TryGetValue(key, out value);
        }


        public void Add(SearchConjunction conjunction)
        {
            var option = new ConjunctionSearchOption("/c." + conjunction.ToString().ToLower(), conjunction);
            _map.Add(option.Name, option);
        }

        public void Add(SearchPropertyProfile profile)
        {
            var option = new PropertySearchOption("/p." + profile.Name, profile);
            _map.Add(option.Name, option);
        }

        public void Add(SearchOperatorProfile profile)
        {
            var option = new OperationSearchOption("/m." + profile.Name, profile);
            _map.Add(option.Name, option);
        }
    }

}
