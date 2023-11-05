using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// NoteTree 専用 Searcher
    /// </summary>
    public class NodeSearcher : Searcher
    {
        private bool _allowFolder;


        public NodeSearcher()
        {
            UpdateProperties();
        }


        public bool AllowFolder
        {
            get { return _allowFolder; }
            set
            {
                if (_allowFolder != value)
                {
                    _allowFolder = value;
                    UpdateProperties();
                }
            }
        }


        private void UpdateProperties()
        {
            // allow folder
            PreKeys = AllowFolder ? new() : new() { new SearchKey(SearchConjunction.And, SearchFilterProfiles.Equal, SearchPropertyProfiles.IsDirectory, "false") };

            // pushpin
            PostKeys = new() { new SearchKey(SearchConjunction.Or, SearchFilterProfiles.Equal, SearchPropertyProfiles.IsPinned, "true") };
        }

        /// <summary>
        /// 検索
        /// </summary>
        public IEnumerable<Node> Search(string keyword, IEnumerable<Node> entries, CancellationToken token)
        {
            return base.Search(keyword, entries, token).Cast<Node>().OrderByDescending(e => e.IsPushPin);
        }
    }
}
