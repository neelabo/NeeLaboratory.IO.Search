using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeLaboratory.IO.Search.FileNode
{
    /// <summary>
    /// NoteTree 専用 Searcher
    /// </summary>
    public class NodeSearcher : Searcher
    {
        private bool _allowFolder = true;


        public NodeSearcher() : base(CreateContext())
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

        public static SearchContext CreateContext()
        {
            var context = new SearchContext();
            context.AddProfile(new DateSearchProfile());
            context.AddProfile(new NodeSearchProfile());
            return context;
        }

        private void UpdateProperties()
        {
            // allow folder
            PreKeys = AllowFolder ? new() : new() { new SearchKey(SearchConjunction.And, SearchFilterProfiles.Equal, ExtraSearchPropertyProfiles.IsDirectory, "false") };

            // pushpin
            PostKeys = new() { new SearchKey(SearchConjunction.Or, SearchFilterProfiles.Equal, ExtraSearchPropertyProfiles.IsPinned, "true") };
        }

        /// <summary>
        /// 検索
        /// </summary>
        public IEnumerable<Node> Search(string keyword, IEnumerable<Node> entries, CancellationToken token)
        {
            return base.Search(keyword, entries, token).Cast<Node>().OrderByDescending(e => e.IsPushPin);
        }
    }



    public class NodeSearchProfile : SearchProfile
    {
        public NodeSearchProfile()
        {
            Options.Add(ExtraSearchPropertyProfiles.IsDirectory);
            Options.Add(ExtraSearchPropertyProfiles.IsPinned);
        }
    }

}
