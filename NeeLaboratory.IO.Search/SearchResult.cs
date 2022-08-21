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
        public SearchResult(string keyword, SearchOption option, ObservableCollection<NodeContent>? items)
        {
            Keyword = keyword;
            SearchOption = option;
            Items = items ?? new ObservableCollection<NodeContent>();
        }

        public SearchResult(string keyword, SearchOption searchOption, ObservableCollection<NodeContent>? items, Exception exception) : this(keyword, searchOption, items)
        {
            Exception = exception;
        }


        #region ISearchResult Support

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// 検索オプション
        /// </summary>
        public SearchOption SearchOption { get; private set; }

        /// <summary>
        /// 検索結果
        /// </summary>
        public ObservableCollection<NodeContent> Items { get; private set; }

        /// <summary>
        /// 検索失敗時の例外
        /// </summary>
        public Exception? Exception { get; private set; }

        #endregion
    }

}
