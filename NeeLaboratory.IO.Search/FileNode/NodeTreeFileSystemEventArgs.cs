using System.IO;

namespace NeeLaboratory.IO.Search.FileNode
{
    /// <summary>
    /// ファイル監視による変更イベンのトデータ
    /// </summary>
    internal class NodeTreeFileSystemEventArgs
    {
        /// <summary>
        /// 所属ノード
        /// </summary>
        public string NodePath { get; private set; }

        /// <summary>
        /// ファイル変更イベントのデータ
        /// </summary>
        public FileSystemEventArgs FileSystemEventArgs { get; private set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        public NodeTreeFileSystemEventArgs(string path, FileSystemEventArgs args)
        {
            NodePath = path;
            FileSystemEventArgs = args;
        }
    }
}
