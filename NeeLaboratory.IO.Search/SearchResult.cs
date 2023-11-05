using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索結果
    /// </summary>
    public class SearchResult : ISearchResult
    {
        public SearchResult(string keyword, SearchDescription option, IEnumerable<Node>? items) : this(keyword, option, items, null)
        {
        }

        public SearchResult(string keyword, SearchDescription option, IEnumerable<Node>? items, Exception? exception)
        {
            Keyword = keyword;
            SearchDescription = option;
            Items = new ObservableCollection<NodeContent>(items?.Select(e => e.Content) ?? Array.Empty<NodeContent>());
            Exception = exception;
        }


        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// 検索オプション
        /// </summary>
        public SearchDescription SearchDescription { get; private set; }

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
