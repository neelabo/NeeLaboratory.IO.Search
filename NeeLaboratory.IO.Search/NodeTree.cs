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
        private static Utility.Logger Logger => Development.Logger;


        /// <summary>
        /// ノード環境
        /// </summary>
        private readonly SearchContext _context;

        /// <summary>
        /// ファイル変更監視
        /// </summary>
        private FileSystemWatcher? _fileSystemWatcher;



        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="path"></param>
        public NodeTree(SearchArea area, SearchContext ctx)
        {
            _context = ctx;

            Path = area.Path;
            IncludeSubdirectories = area.IncludeSubdirectories;
            IsDarty = true;

            InitializeWatcher(area.IncludeSubdirectories);
        }



        /// <summary>
        /// ファイル変更イベント
        /// </summary>
        internal event EventHandler<NodeTreeFileSystemEventArgs>? FileSystemChanged;


        /// <summary>
        /// パス
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// サブフォルダーも含める
        /// </summary>
        public bool IncludeSubdirectories { get; private set; }

        /// <summary>
        /// 基準ノード
        /// </summary>
        public Node? Root { get; private set; }

        /// <summary>
        /// 更新必要フラグ
        /// </summary>
        public bool IsDarty { get; private set; }

        /// <summary>
        /// 他のNodeTreeの子
        /// </summary>
        public bool IsChild => Root is not null && Root.Parent != null;



        /// <summary>
        /// ノード収拾
        /// </summary>
        /// <param name="token"></param>
        public void Collect(CancellationToken token)
        {
            ThrowIfDisposed();

            token.ThrowIfCancellationRequested();

            if (!IsDarty)
            {
                _context.TotalCount += NodeCount();
                return;
            }
            IsDarty = false;

            // フォルダ監視開始
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }

            // node
            Root = Node.Collect(Path, null, IncludeSubdirectories ? -1 : 1, _context, token);
            DumpTree(false);
        }

        /// <summary>
        /// 開発用：ツリー表示
        /// </summary>
        [Conditional("DEBUG")]
        public void DumpTree(bool verbose)
        {
            if (Root is null)
            {
                Logger.Trace($"---- {Path}: Invalid.");
            }
            else
            {
                Logger.Trace($"---- {Path}: IncludeSubdirectories={IncludeSubdirectories}, IsChild={IsChild}");

                if (verbose)
                {
                    Root.Dump();
                }
            }
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
        public Node? AddNode(string path, CancellationToken token)
        {
            ThrowIfDisposed();

            var node = Root?.Add(path, _context, token);
            Logger.Trace($"Add: {node?.Path}");
            ////DumpTree();
            return node;
        }

        /// <summary>
        /// ノードの削除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node? RemoveNode(string path)
        {
            ThrowIfDisposed();

            var node = Root?.Remove(path, _context);
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
        public Node? Rename(string oldPath, string newPath)
        {
            ThrowIfDisposed();

            var node = Root?.Search(oldPath, _context, CancellationToken.None);
            if (node != null)
            {
                Logger.Trace($"Rename: {oldPath} -> {newPath}");
                // 場所の変更は認めない
                if (node.Parent?.Path != System.IO.Path.GetDirectoryName(newPath))
                {
                    throw new ArgumentException("Directory can not be changed");
                }

                node.Rename(System.IO.Path.GetFileName(newPath));
                ////DumpTree();
            }
            else
            {
                // もしノードになければ追加
                // NOTE: 大文字小文字変換でFileSystemWatcherからDeleteが呼ばれるため、この状態になることがありうる
                node = Root?.Add(newPath, _context, CancellationToken.None);
                if (node != null)
                {
                    Logger.Trace($"Rename: Add {newPath}");
                }
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
            ThrowIfDisposed();

            var node = Root?.Search(path, _context, CancellationToken.None);
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
        private void InitializeWatcher(bool includeSubdirectories)
        {
            try
            {
                _fileSystemWatcher = new FileSystemWatcher
                {
                    Path = Path,
                    IncludeSubdirectories = includeSubdirectories,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size
                };
                _fileSystemWatcher.Created += Watcher_Changed;
                _fileSystemWatcher.Deleted += Watcher_Changed;
                _fileSystemWatcher.Renamed += Watcher_Renamed;
                _fileSystemWatcher.Changed += Watcher_Changed;
            }
            catch (Exception e)
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
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        /// <summary>
        /// ファイル変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(object? sender, FileSystemEventArgs e)
        {
            if (_disposedValue) return;

            ////Logger.Trace($"Watcher: {e.ChangeType}: {e.Name}");
            FileSystemChanged?.Invoke(sender, new NodeTreeFileSystemEventArgs(Path, e));
        }

        private void Watcher_Renamed(object? sender, RenamedEventArgs e)
        {
            if (_disposedValue) return;

            ////Logger.Trace($"Watcher: {e.ChangeType}: {e.OldName} -> {e.Name}");
            FileSystemChanged?.Invoke(sender, new NodeTreeFileSystemEventArgs(Path, e));
        }



        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    TerminateWatcher();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
