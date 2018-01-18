﻿// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        SearchOption SearchOption { get; }

        /// <summary>
        /// 検索結果
        /// </summary>
        ObservableCollection<NodeContent> Items { get; }
    }

}
