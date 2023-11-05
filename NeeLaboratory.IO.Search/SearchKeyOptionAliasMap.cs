using System.Collections;
using System.Collections.Generic;

namespace NeeLaboratory.IO.Search
{
    public class SearchKeyOptionAliasMap : IEnumerable<KeyValuePair<string, List<string>>>
    {
        private readonly Dictionary<string, List<string>> _map = new();

        public SearchKeyOptionAliasMap()
        {
        }


        public List<string> this[string key]
        {
            get { return _map[key]; }
            set { _map[key] = value; }
        }

        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(string key, List<string> value)
        {
            _map[key] = value;
        }

        public List<string> Decode(string s)
        {
            if (_map.TryGetValue(s, out var options))
            {
                return options;
            }
            else
            {
                return new List<string> { s };
            }
        }


        public void AddRange(SearchKeyOptionAliasMap options)
        {
            foreach (var option in options)
            {
                _map[option.Key] = option.Value;
            }
        }
    }
}
