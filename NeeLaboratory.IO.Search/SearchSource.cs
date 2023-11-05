using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索ファイルをインデックス化
    /// </summary>
    public class SearchSource : IDisposable
    {
        /// <summary>
        /// ノード環境
        /// </summary>
        private readonly NodeContext _context = new();

        /// <summary>
        /// ノード群
        /// </summary>
        private Dictionary<string, NodeTree> _fileIndexDirectory;


        public SearchSource()
        {
            _fileIndexDirectory = new Dictionary<string, NodeTree>();
        }


        /// <summary>
        /// ファイルシステム変更イベント
        /// </summary>
        internal event EventHandler<NodeTreeFileSystemEventArgs>? FileSystemChanged;

        /// <summary>
        /// ノード変更イベント
        /// </summary>
        public event EventHandler<NodeChangedEventArgs>? NodeChanged;



        /// <summary>
        /// ノード環境
        /// </summary>
        public NodeContext Context => _context;

        /// <summary>
        /// すべてのNodeを走査
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> AllNodes
        {
            get
            {
                ThrowIfDisposed();

                foreach (var nodeTree in _fileIndexDirectory.Values)
                {
                    if (nodeTree.Root != null && !nodeTree.IsChild)
                    {
                        foreach (var node in nodeTree.Root.AllChildren)
                            yield return node;
                    }
                }
            }
        }


        /// <summary>
        /// 検索フォルダのインデックス化
        /// </summary>
        /// <param name="areas">検索フォルダ群</param>
        public void Collect(List<SearchArea> areas, CancellationToken token)
        {
            ThrowIfDisposed();

            token.ThrowIfCancellationRequested();

            Debug.WriteLine($"Search: Index Collect: ...");

            if (areas.Count == 0) return;

            var roots = new List<SearchArea>();

            foreach (var area in areas)
            {
                // 重複の除外。再帰フラグはONを優先。
                var member = roots.FirstOrDefault(e => e.Path == area.Path);
                if (member != null)
                {
                    member.IncludeSubdirectories = member.IncludeSubdirectories | area.IncludeSubdirectories;
                    continue;
                }

                // 他のパスに含まれるなら除外
                if (areas.Any(e => area != e && e.IncludeSubdirectories && area.Path.StartsWith(e.Path.TrimEnd('\\') + "\\")))
                {
                    continue;
                }

                roots.Add(area);

            }

            CollectCore(roots, token);

            Debug.WriteLine($"Search: Index Collect: {_context.TotalCount}");
        }

        /// <summary>
        /// 検索フォルダのインデックス化
        /// 更新分のみ
        /// </summary>
        private void CollectCore(List<SearchArea> areas, CancellationToken token)
        {
            ThrowIfDisposed();

            token.ThrowIfCancellationRequested();

            var newDictionary = new Dictionary<string, NodeTree>();

            foreach (var area in areas)
            {
                _fileIndexDirectory.TryGetValue(area.Path, out NodeTree? nodeTree);
                if (nodeTree is null || nodeTree.IncludeSubdirectories != area.IncludeSubdirectories)
                {
                    nodeTree = new NodeTree(area, _context);
                    nodeTree.FileSystemChanged += (s, e) => FileSystemChanged?.Invoke(s, e);
                }
                newDictionary.Add(area.Path, nodeTree);
            }

            _context.TotalCount = 0;

            var options = new ParallelOptions() { CancellationToken = token };
            Parallel.ForEach(newDictionary.Values, options, nodeTree =>
            {
                nodeTree.Collect(options.CancellationToken);
            });

            // 再登録されなかったパスの後処理を行う
            foreach (var nodeTree in _fileIndexDirectory.Values)
            {
                if (!newDictionary.ContainsValue(nodeTree))
                {
                    nodeTree.Dispose();
                }
            }

            // ノードの統合
            foreach (var nodeTree in newDictionary.Values)
            {
                if (!nodeTree.IncludeSubdirectories)
                {
                    var children = nodeTree.Root?.Children;
                    if (children is null) continue;

                    foreach (var node in children.Where(e => e.IsDirectory).ToList())
                    {
                        if (newDictionary.TryGetValue(node.Path, out NodeTree? sub))
                        {
                            if (sub.Root is null) throw new InvalidOperationException("sub.Root must not be null");
                            if (sub.Root.Parent != null) throw new InvalidOperationException("Incorrectly configured SearchArea");
                            children.Remove(node);
                            sub.Root.Parent = nodeTree.Root;
                            children.Add(sub.Root);
                        }
                    }
                }
            }

            _fileIndexDirectory = newDictionary;
            System.GC.Collect();
        }


        /// <summary>
        /// 開発用：ツリー表示
        /// </summary>
        [Conditional("DEBUG")]
        public void DumpTree(bool verbose)
        {
            foreach (var nodeTree in _fileIndexDirectory.Values)
            {
                if (nodeTree.IsChild)
                {
                    nodeTree.DumpTree(false);
                }
                else
                {
                    nodeTree.DumpTree(verbose);
                }
            }
        }

        /// <summary>
        /// ノード数取得
        /// </summary>
        /// <returns></returns>
        public int NodeCount()
        {
            return _fileIndexDirectory.Sum(e => e.Value.IsChild ? 0 : e.Value.NodeCount());
        }

        /// <summary>
        /// インデックス追加
        /// </summary>
        /// <param name="root">検索フォルダ</param>
        /// <param name="paths">追加パス</param>
        public Node? AddPath(string? root, string path, CancellationToken token)
        {
            ThrowIfDisposed();

            root = root ?? GetRoot(path);
            if (root is null || !_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].AddNode(path, token);
            if (node is null) return null;

            NodeChanged?.Invoke(this, new NodeChangedEventArgs(NodeChangedAction.Add, node));
            return node;
        }

        /// <summary>
        /// インデックス削除
        /// </summary>
        /// <param name="root">検索フォルダ</param>
        /// <param name="path">削除パス</param>
        public Node? RemovePath(string? root, string path)
        {
            ThrowIfDisposed();

            root = root ?? GetRoot(path);
            if (root is null || !_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].RemoveNode(path);
            if (node is null) return null;

            NodeChanged?.Invoke(this, new NodeChangedEventArgs(NodeChangedAction.Remove, node));

            return node;
        }

        /// <summary>
        /// リネーム
        /// </summary>
        /// <param name="root">所属するツリー。nullの場合は oldFileName から推定する</param>
        /// <param name="oldFileName"></param>
        /// <param name="newFilename"></param>
        public Node? RenamePath(string? root, string oldFileName, string newFileName)
        {
            ThrowIfDisposed();

            root = root ?? GetRoot(oldFileName);
            if (root == null || !_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].Rename(oldFileName, newFileName);
            if (node != null)
            {
                NodeChanged?.Invoke(this, new NodeRenamedEventArgs(NodeChangedAction.Rename, node, oldFileName));
            }

            return node;
        }

        /// <summary>
        /// インデックスの情報更新
        /// </summary>
        /// <param name="root">所属するツリー。nullの場合はパスから推定する</param>
        /// <param name="path">変更するインデックスのパス</param>
        public void RefreshIndex(string? root, string path)
        {
            ThrowIfDisposed();

            root = root ?? GetRoot(path);
            if (root == null || !_fileIndexDirectory.ContainsKey(root))
            {
                return;
            }

            _fileIndexDirectory[root].RefreshNode(path);
        }

        /// <summary>
        /// パスが所属するRoot名を返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string? GetRoot(string path)
        {
            if (path == null) return null;
            return _fileIndexDirectory.Keys.FirstOrDefault(e => path.StartsWith(e.TrimEnd('\\') + "\\"));
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
                    foreach (var pair in _fileIndexDirectory)
                    {
                        pair.Value.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
