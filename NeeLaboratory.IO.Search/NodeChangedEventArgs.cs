// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// ノード変更イベントデータ
    /// </summary>
    public class NodeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// イベント種類
        /// </summary>
        public NodeChangedAction Action { get; set; }

        /// <summary>
        /// 変更ノード
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// リネーム時の旧パス
        /// </summary>
        public string OldPath { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action"></param>
        /// <param name="node"></param>
        public NodeChangedEventArgs(NodeChangedAction action, Node node)
        {
            this.Action = action;
            this.Node = node;
        }
    }
}
