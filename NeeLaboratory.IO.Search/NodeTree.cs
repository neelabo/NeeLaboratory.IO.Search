// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// ノード木
    /// １かたまりのノード。この単位で監視する。
    /// </summary>
    public class NodeTree : IDisposable
    {
        private Utility.Logger Logger => Development.Logger;

        #region Fields

        // ファイル変更監視
        private FileSystemWatcher _fileSystemWatcher;

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="path"></param>
        public NodeTree(string path)
        {
            Path = path;
            IsDarty = true;

            InitializeWatcher();
        }

        #endregion

        #region Events

        /// <summary>
        /// ファイル変更イベント
        /// </summary>
        internal event EventHandler<NodeTreeFileSystemEventArgs> FileSystemChanged;

        #endregion

        #region Properties

        /// <summary>
        /// パス
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 基準ノード
        /// </summary>
        public Node Root { get; private set; }

        /// <summary>
        /// 更新必要フラグ
        /// </summary>
        public bool IsDarty { get; private set; }

        #endregion
        
        #region Methods

        /// <summary>
        /// ノード収拾
        /// </summary>
        /// <param name="token"></param>
        public void Collect(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (!IsDarty)
            {
                Node.TotalCount += NodeCount();
                return;
            }
            IsDarty = false;

            // フォルダ監視開始
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }

            // node
            Root = Node.Collect(Path, null, token);
            DumpTree();
        }

        /// <summary>
        /// 開発用：ツリー表示
        /// </summary>
        [Conditional("DEBUG")]
        public void DumpTree()
        {
            Logger.Trace($"---- {Path}");
            ////Root.Dump();
        }

        /// <summary>
        /// ノード数を数える
        /// </summary>
        /// <returns></returns>
        public int NodeCount()
        {
            return Root != null ? Root.AllNodes.Count() : 0;
        }

        /// <summary>
        /// ノードの追加
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node AddNode(string path, CancellationToken token)
        {
            var node = Root?.Add(path, token);
            Logger.Trace($"Add: {node?.Path}");
            ////DumpTree();
            return node;
        }

        /// <summary>
        /// ノードの削除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node RemoveNode(string path)
        {
            var node = Root?.Remove(path);
            Logger.Trace($"Del: {node?.Path}");
            ////DumpTree();
            return node;
        }

        /// <summary>
        /// 名前変更
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public Node Rename(string oldPath, string newPath)
        {
            Logger.Trace($"Rename: {oldPath} -> {newPath}");
            var node = Root?.Search(oldPath, CancellationToken.None);
            if (node != null)
            {
                // 場所の変更は認めない
                if (node.Parent?.Path != System.IO.Path.GetDirectoryName(newPath))
                {
                    throw new ApplicationException("リネームなのに場所が変更されている");
                }

                node.Rename(System.IO.Path.GetFileName(newPath));
                ////DumpTree();
            }

            return node;
        }

        /// <summary>
        /// 情報更新
        /// </summary>
        /// <param name="path"></param>
        /// <returns>更新処理がされたらtrue</returns>
        public bool RefleshNode(string path)
        {
            var node = Root?.Search(path, CancellationToken.None);
            if (node != null)
            {
                node.Reflesh();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ファイル監視初期化
        /// </summary>
        private void InitializeWatcher()
        {
            try
            {
                _fileSystemWatcher = new FileSystemWatcher();
                _fileSystemWatcher.Path = Path;
                _fileSystemWatcher.IncludeSubdirectories = true;
                _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
                _fileSystemWatcher.Created += Watcher_Changed;
                _fileSystemWatcher.Deleted += Watcher_Changed;
                _fileSystemWatcher.Renamed += Watcher_Changed;
                _fileSystemWatcher.Changed += Watcher_Changed;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                TerminateWatcher();
            }
        }

        /// <summary>
        /// ファイル監視終了処理
        /// </summary>
        private void TerminateWatcher()
        {
            FileSystemChanged = null;

            _fileSystemWatcher?.Dispose();
            _fileSystemWatcher = null;
        }

        /// <summary>
        /// ファイル変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileSystemChanged?.Invoke(sender, new NodeTreeFileSystemEventArgs(Path, e));
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            TerminateWatcher();
        }

        #endregion
    }
}
