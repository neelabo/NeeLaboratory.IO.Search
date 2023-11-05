using System;

namespace NeeLaboratory.IO.Search.FileNode
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
        /// コンストラクタ
        /// </summary>
        /// <param name="action"></param>
        /// <param name="node"></param>
        public NodeChangedEventArgs(NodeChangedAction action, Node node)
        {
            Action = action;
            Node = node;
        }
    }

    public class NodeRenamedEventArgs : NodeChangedEventArgs
    {
        public NodeRenamedEventArgs(NodeChangedAction action, Node node, string oldPath) : base(action, node)
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
