using System;
using NeeLaboratory.IO.Search.FileNode;

namespace NeeLaboratory.IO.Search.FileSearch
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
        /// constructor
        /// </summary>
        /// <param name="action"></param>
        /// <param name="content"></param>
        public SearchResultChangedEventArgs(NodeChangedAction action, NodeContent content)
        {
            Action = action;
            Content = content;
        }
    }

    public class SearchResultRenamedEventArgs : SearchResultChangedEventArgs
    {
        public SearchResultRenamedEventArgs(NodeChangedAction action, NodeContent content, string oldPath) : base(action, content)
        {
            if (action != NodeChangedAction.Rename) throw new InvalidOperationException("action must be Rename");
            OldPath = oldPath;
        }

        /// <summary>
        /// リネーム時の旧パス
        /// </summary>
        public string OldPath { get; set; }
    }
}
