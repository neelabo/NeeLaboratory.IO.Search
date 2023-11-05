using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeeLaboratory.IO.Search.FileNode;

namespace NeeLaboratory.IO.Search.FileSearch
{
    /// <summary>
    /// 検索結果
    /// </summary>
    public class SearchResult : ISearchResult
    {
        public SearchResult(string keyword, IEnumerable<Node>? items) : this(keyword, items, null)
        {
        }

        public SearchResult(string keyword, IEnumerable<Node>? items, Exception? exception)
        {
            Keyword = keyword;
            Items = new ObservableCollection<NodeContent>(items?.Select(e => e.Content) ?? Array.Empty<NodeContent>());
            Exception = exception;
        }


        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// 検索結果
        /// </summary>
        public ObservableCollection<NodeContent> Items { get; private set; }

        /// <summary>
        /// 検索失敗時の例外
        /// </summary>
        public Exception? Exception { get; private set; }
    }

}
