// Copyright (c) 2015-2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索結果変更通知イベントデータ
    /// </summary>
    public class SearchResultChangedEventArgs : EventArgs
    {
        /// <summary>
        /// イベント種類
        /// </summary>
        public NodeChangedAction Action { get; set; }

        /// <summary>
        /// 変更ノード
        /// </summary>
        public NodeContent Content { get; set; }

        /// <summary>
        /// リネーム時の旧パス
        /// </summary>
        public string OldPath { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="action"></param>
        /// <param name="content"></param>
        public SearchResultChangedEventArgs(NodeChangedAction action, NodeContent content)
        {
            this.Action = action;
            this.Content = content;
        }
    }
}
