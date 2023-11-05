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

        private static SearchContext CreateContext()
        {
            var context = new SearchContext();
            context.AddProfile(new DateSearchProfile());
            context.AddProfile(new NodeSearchProfile());
            return context;
        }

        private void UpdateProperties()
        {
            // allow folder
            PreKeys = AllowFolder ? new() : new() { new SearchKey(SearchConjunction.And, SearchFilterProfiles.Equal, NodeSearchPropertyProfiles.IsDirectory, "false") };

            // pushpin
            PostKeys = new() { new SearchKey(SearchConjunction.Or, SearchFilterProfiles.Equal, NodeSearchPropertyProfiles.IsPinned, "true") };
        }

        /// <summary>
        /// 検索
        /// </summary>
        public IEnumerable<Node> Search(string keyword, IEnumerable<Node> entries, CancellationToken token)
        {
            return base.Search(keyword, entries, token).Cast<Node>().OrderByDescending(e => e.IsPushPin);
        }
    }


    public class DateSearchProfile : SearchProfile
    {
        public DateSearchProfile()
        {
            Options.Add(DateSearchPropertyProfiles.Date);

            Alias.Add("/since", new() { "/p.date", "/m.ge" });
            Alias.Add("/until", new() { "/p.date", "/m.le" });
        }
    }

    public class NodeSearchProfile : SearchProfile
    {
        public NodeSearchProfile()
        {
            Options.Add(NodeSearchPropertyProfiles.IsDirectory);
            Options.Add(NodeSearchPropertyProfiles.IsPinned);
        }
    }

    public static class NodeSearchPropertyProfiles
    {
        public static SearchPropertyProfile IsDirectory { get; } = new SearchPropertyProfile("isdir", BooleanSearchValue.Default);
        public static SearchPropertyProfile IsPinned { get; } = new SearchPropertyProfile("ispinned", BooleanSearchValue.Default);
    }
}
