// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="option"></param>
        /// <param name="items"></param>
        public SearchResult(string keyword, SearchOption option, ObservableCollection<NodeContent> items)
        {
            Keyword = keyword;
            SearchOption = option;
            Items = items;
        }

        #endregion

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

        #endregion
    }

}
