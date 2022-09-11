using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索エンジン
    /// </summary>
    public class SearchEngine : IDisposable
    {

        public static Utility.Logger Logger => Development.Logger;


        /// <summary>
        /// ノード構築処理のキャンセルトークン
        /// </summary>
        private CancellationTokenSource _resetAreaCancellationTokenSource = new();

        /// <summary>
        /// 検索処理のキャンセルトークン
        /// </summary>
        private CancellationTokenSource _searchCancellationTokenSource = new();

        /// <summary>
        /// 検索コア
        /// </summary>
        private readonly SearchCore _core;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchEngine()
        {
            _core = new SearchCore();
            _core.FileSystemChanged += Core_FileSystemChanged;

            _commandEngine = new SearchCommandEngine();
        }


        /// <summary>
        /// 検索エリア
        /// </summary>
        private ObservableCollection<SearchArea> _searchAreas = new();



        /// <summary>
        /// コア検索エンジン
        /// </summary>
        internal SearchCore Core => _core;

        /// <summary>
        /// ノード環境
        /// </summary>
        public SearchContext Context => _core.Context;

        /// <summary>
        /// 検索エンジン状態
        /// </summary>
        public SearchCommandEngineState State => _commandEngine.State;

        /// <summary>
        /// ノード数(おおよそ)
        /// 通知用
        /// </summary>
        public int NodeCountMaybe => this.Context.TotalCount;

        /// <summary>
        /// ノード数(計測するので重い)
        /// </summary>
        public int NodeCount => _core.NodeCount();

        /// <summary>
        /// コマンドエンジン
        /// </summary>
        private readonly SearchCommandEngine _commandEngine;

        /// <summary>
        /// コマンドエンジン Logger
        /// </summary>
        public Utility.Logger CommandEngineLogger => _commandEngine.Logger;



        [Conditional("DEBUG")]
        public void DumpTree(bool verbose)
        {
            ThrowIfDisposed();
            _core.DumpTree(verbose);
        }

        /// <summary>
        /// 検索エリア設定
        /// </summary>
        /// <param name="areas"></param>
        public void SetSearchAreas(IEnumerable<SearchArea> areas)
        {
            ThrowIfDisposed();

            var value = new ObservableCollection<SearchArea>(areas);

            if (_searchAreas != value)
            {
                if (_searchAreas != null)
                {
                    _searchAreas.CollectionChanged -= Areas_CollectionChanged;
                }
                _searchAreas = value;
                if (_searchAreas != null)
                {
                    _searchAreas.CollectionChanged += Areas_CollectionChanged;
                }
                Collect();
            }
        }

        public void AddSearchAreas(params SearchArea[] areas)
        {
            foreach (var area in areas)
            {
                _searchAreas.Add(area);
            }
            Collect();
        }

        /// <summary>
        /// ファイルシステムの変更をノードに反映させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Core_FileSystemChanged(object? sender, NodeTreeFileSystemEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.FileSystemEventArgs.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    AddNode(e.NodePath, e.FileSystemEventArgs.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    RemoveNode(e.NodePath, e.FileSystemEventArgs.FullPath);
                    break;
                case WatcherChangeTypes.Renamed:
                    var rename = (RenamedEventArgs)e.FileSystemEventArgs;
                    RenameNode(e.NodePath, rename.OldFullPath, rename.FullPath);
                    break;
                case WatcherChangeTypes.Changed:
                    RefleshNode(e.NodePath, e.FileSystemEventArgs.FullPath);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 全てのコマンドの完了待機
        /// </summary>
        public async Task WaitAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            var command = new WaitCommand(this, new CommandArgs());
            _commandEngine.Enqueue(command);

            await command.WaitAsync(token);
        }

        /// <summary>
        /// 検索範囲の変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Areas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    Collect();
                    break;

                default:
                    throw new NotImplementedException("not support yet.");
            }
        }

        /// <summary>
        /// ノード構築
        /// </summary>
        private void Collect()
        {
            ThrowIfDisposed();

            // one command only.
            _resetAreaCancellationTokenSource.Cancel();
            _resetAreaCancellationTokenSource.Dispose();
            _resetAreaCancellationTokenSource = new CancellationTokenSource();

            var command = new CollectCommand(this, new CollectCommandArgs(_searchAreas.ToList()));
            _commandEngine.Enqueue(command, _resetAreaCancellationTokenSource.Token);
        }

        //
        internal void Collect_Execute(CollectCommandArgs args, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _core.Collect(args.Area, token);
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public async Task<SearchResult> SearchAsync(string keyword, SearchOption option)
        {
            ThrowIfDisposed();

            // one command only.
            _searchCancellationTokenSource.Cancel();
            _searchCancellationTokenSource.Dispose();
            _searchCancellationTokenSource = new CancellationTokenSource();

            return await SearchAsync(keyword, option, _searchCancellationTokenSource.Token);
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<SearchResult> SearchAsync(string keyword, SearchOption option, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            var command = new SearchCommand(this, new SearchExCommandArgs(keyword, option.Clone()));
            _commandEngine.Enqueue(command, token);

            await command.WaitAsync(token);
            return command.SearchResult ?? throw new InvalidOperationException("SearchResult must not be null");
        }

        //
        internal SearchResult Search_Execute(SearchExCommandArgs args, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                return new SearchResult(args.Keyword, args.Option, _core.Search(args.Keyword, args.Option, token));
            }
            catch (Exception ex)
            {
                return new SearchResult(args.Keyword, args.Option, null, ex);
            }
        }

        /// <summary>
        /// 検索キャンセル
        /// </summary>
        public void CancelSearch()
        {
            ThrowIfDisposed();

            _searchCancellationTokenSource.Cancel();
        }


        /// <summary>
        /// マルチ検索
        /// </summary>

        public async Task<List<SearchResult>> MultiSearchAsync(IEnumerable<string> keywords, SearchOption option)
        {
            // one command only.
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            return await MultiSearchAsync(keywords, option, _searchCancellationTokenSource.Token);
        }

        public async Task<List<SearchResult>> MultiSearchAsync(IEnumerable<string> keywords, SearchOption option, CancellationToken token)
        {
            if (_commandEngine == null)
            {
                return keywords.Select(e => new SearchResult(e, option, new ObservableCollection<NodeContent>())).ToList();
            }

            var command = new MultiSearchCommand(this, new MultiSearchExCommandArgs(keywords.ToList(), option.Clone()));
            _commandEngine.Enqueue(command, token);

            await command.WaitAsync(token);
            return command.SearchResults ?? throw new InvalidOperationException("SearchResults must not be null");
        }

        internal List<SearchResult> MultiSearch_Execute(MultiSearchExCommandArgs args, CancellationToken token)
        {
            var units = args.Keywords.Select(e => new MultiSearchUnit(e, args.Option)).ToList();

            Parallel.ForEach(units, unit =>
            {
                try
                {
                    unit.Result = new SearchResult(unit.Keyword, unit.Option, _core.Search(unit.Keyword, unit.Option, token));
                }
                catch (Exception ex)
                {
                    unit.Result = new SearchResult(unit.Keyword, unit.Option, null, ex);
                }
            });

            return units
                .Select(e => e.Result ?? throw new InvalidOperationException("Result must not be null"))
                .ToList();
        }

        private class MultiSearchUnit
        {
            public MultiSearchUnit(string keyword, SearchOption option)
            {
                Keyword = keyword;
                Option = option;
            }

            public string Keyword { get; set; }
            public SearchOption Option { get; set; }
            public SearchResult? Result { get; set; }
        }


        /// <summary>
        /// ノード情報更新
        /// 反映は非同期に行われる
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        public void Reflesh(string path)
        {
            ThrowIfDisposed();

            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs(NodeChangeType.Reflesh, null, path));
            _commandEngine.Enqueue(command);
        }


        /// <summary>
        /// 名前変更
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void Rename(string src, string dst)
        {
            ThrowIfDisposed();

            var command = new NodeChangeCommand(this, new NodeRenameCommandArgs(NodeChangeType.Rename, null, dst, src));
            _commandEngine.Enqueue(command);
        }


        /// <summary>
        /// 内部コマンド用：ノード追加
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal void AddNode(string root, string path)
        {
            ThrowIfDisposed();

            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs(NodeChangeType.Add, root, path));
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード削除
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal void RemoveNode(string root, string path)
        {
            ThrowIfDisposed();

            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs(NodeChangeType.Remove, root, path));
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード名前変更
        /// </summary>
        /// <param name="root"></param>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        internal void RenameNode(string root, string oldPath, string newPath)
        {
            ThrowIfDisposed();

            var command = new NodeChangeCommand(this, new NodeRenameCommandArgs(NodeChangeType.Rename, root, newPath, oldPath));
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード情報更新
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal void RefleshNode(string root, string path)
        {
            ThrowIfDisposed();

            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs(NodeChangeType.Reflesh, root, path));
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード変更
        /// </summary>
        /// <param name="args"></param>
        internal void NodeChange_Execute(NodeChangeCommandArgs args, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            switch (args.ChangeType)
            {
                case NodeChangeType.Add:
                    _core.AddPath(args.Root, args.Path, token);
                    break;
                case NodeChangeType.Remove:
                    _core.RemovePath(args.Root, args.Path);
                    break;
                case NodeChangeType.Rename:
                    var rename = (NodeRenameCommandArgs)args;
                    _core.RenamePath(rename.Root, rename.OldPath, rename.Path);
                    break;
                case NodeChangeType.Reflesh:
                    _core.RefleshIndex(args.Root, args.Path);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        public bool IsDisposed()
        {
            return _disposedValue;
        }

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
                    _searchAreas.CollectionChanged -= Areas_CollectionChanged;

                    _resetAreaCancellationTokenSource.Cancel();
                    _resetAreaCancellationTokenSource.Dispose();

                    _searchCancellationTokenSource.Cancel();
                    _searchCancellationTokenSource.Dispose();

                    _commandEngine.Dispose();

                    _core.FileSystemChanged -= Core_FileSystemChanged;
                    _core.Dispose();
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}
