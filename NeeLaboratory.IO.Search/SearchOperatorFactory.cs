using System.Collections.Generic;

namespace NeeLaboratory.IO.Search
{
    public class SearchOperatorFactory
    {
        public delegate SearchOperation CreateSearchOperationFunc(SearchValueContext context, string property, string pattern);

        private readonly Dictionary<string, CreateSearchOperationFunc> _map = new();

        public void Add(string key, CreateSearchOperationFunc func)
        {
            _map.Add(key, func);
        }

        public SearchOperation Create(string key, SearchValueContext context, string property, string pattern)
        {
            return _map[key].Invoke(context, property, pattern);
        }
    }
}
