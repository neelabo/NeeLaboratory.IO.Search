using System.Collections.Generic;

namespace NeeLaboratory.IO.Search
{
    public class SearchOptionAliasMap
    {
        private Dictionary<string, List<string>> _map;

        public SearchOptionAliasMap()
        {
            _map = new Dictionary<string, List<string>>()
            {
                ["/and"] = new() { "/c.and" },
                ["/or"] = new() { "/c.or" },
                ["/not"] = new() { "/c.not" },
                ["/re"] = new() { "/m.re" },
                ["/ire"] = new() { "/m.ire" },
                ["/m0"] = new() { "/m.exact" },
                ["/exact"] = new() { "/m.exact" },
                ["/m1"] = new() { "/m.word" },
                ["/word"] = new() { "/m.word" },
                ["/m2"] = new() { "/m.fuzzy" },
                ["/since"] = new() { "/p.date", "/m.gt" },
                ["/until"] = new() { "/p.date", "/m.lt" },

                ["/lt"] = new() { "m.lt" },
                ["/le"] = new() { "m.le" },
                ["/eq"] = new() { "m.eq" },
                ["/ne"] = new() { "m.ne" },
                ["/ge"] = new() { "m.ge" },
                ["/gt"] = new() { "m.gte" },
            };
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
    }
}
