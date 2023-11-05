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
        public SearchDescription CreateSearchDescription(bool allowFolder)
        {
            var description = new SearchDescription();

            // allow folder
            if (!allowFolder)
            {
                description.PreKeys.Add(new SearchKey(SearchConjunction.And, SearchFilterProfiles.Equal, SearchPropertyProfiles.IsDirectory, "false"));
            }

            // pushpin
            description.PostKeys.Add(new SearchKey(SearchConjunction.Or, SearchFilterProfiles.Equal, SearchPropertyProfiles.IsPinned, "true"));

            return description;
        }

        /// <summary>
        /// 検索
        /// </summary>
        public IEnumerable<Node> Search(string keyword, SearchDescription description, IEnumerable<Node> entries, CancellationToken token)
        {
            return base.Search(keyword, description, entries, token).Cast<Node>().OrderByDescending(e => e.IsPushPin);
        }
    }
}
