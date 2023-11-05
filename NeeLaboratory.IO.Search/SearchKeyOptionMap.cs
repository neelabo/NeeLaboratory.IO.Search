using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeeLaboratory.IO.Search
{
    public class SearchKeyOptionMap : IEnumerable<KeyValuePair<string, SearchKeyOption>>
    {
        private readonly Dictionary<string, SearchKeyOption> _map = new();

        public SearchKeyOptionMap()
        {
        }

        public SearchKeyOption this[string key]
        {
            get { return _map[key]; }
            set { _map[key] = value; }
        }

        public IEnumerator<KeyValuePair<string, SearchKeyOption>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public void AddRange(SearchKeyOptionMap options)
        {
            foreach(var option in options)
            {
                _map[option.Key] = option.Value;
            }
        }

    }

}
