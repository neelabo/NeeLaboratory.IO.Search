using System;
using System.Collections.ObjectModel;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索結果インターフェイス
    /// </summary>
    public interface ISearchResult
    {
        /// <summary>
        /// 検索キーワード
        /// </summary>
        string Keyword { get; }

        /// <summary>
        /// 検索オプション
        /// </summary>
        SearchDescription SearchDescription { get; }

        /// <summary>
        /// 検索結果
        /// </summary>
        ObservableCollection<NodeContent> Items { get; }

        /// <summary>
        /// 検索失敗時の例外
        /// </summary>
        Exception? Exception { get; }
    }

}
