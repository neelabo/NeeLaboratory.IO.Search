// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.IO;

namespace NeeLaboratory.IO.Search
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
            this.NodePath = path;
            this.FileSystemEventArgs = args;
        }
    }
}
