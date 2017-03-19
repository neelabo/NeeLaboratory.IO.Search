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
        /// 検索エリア
        /// </summary>
        private ObservableCollection<string> _searchAreas;
        public ObservableCollection<string> SearchAreas
        {
            get => _searchAreas;
            set
            {
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
        }

        /// <summary>
        /// 検索エリア設定
        /// </summary>
        /// <param name="areas"></param>
        public void SetSearchAreas(IEnumerable<string> areas)
        {
            this.SearchAreas = new ObservableCollection<string>(areas);
        }

        /// <summary>
        /// コア検索エンジン
        /// </summary>
        private SearchCore _core;
        internal SearchCore Core => _core;

        /// <summary>
        /// 検索エンジン状態
        /// </summary>
        public SearchEngineState State => _commandEngine.State;

        /// <summary>
        /// ノード数(おおよそ)
        /// 通知用
        /// </summary>
        public int NodeCountMaybe => Node.TotalCount;

        /// <summary>
        /// コマンドエンジン
        /// </summary>
        private SerarchCommandEngine _commandEngine;

        /// <summary>
        /// コマンドエンジン Logger
        /// </summary>
        public Utility.Logger CommandEngineLogger => _commandEngine.Logger;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchEngine()
        {
            this.SearchAreas = new ObservableCollection<string>();

            _core = new SearchCore();
            _core.FileSystemChanged += Core_FileSystemChanged;
        }

        
        /// <summary>
        /// ファイルシステムの変更をノードに反映させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Core_FileSystemChanged(object sender, NodeTreeFileSystemEventArgs e)
        {
            switch (e.FileSystemEventArgs.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    AddNode(e.NodePath, e.FileSystemEventArgs.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    RemoveNode(e.NodePath, e.FileSystemEventArgs.FullPath);
                    break;
                case WatcherChangeTypes.Renamed:
                    var rename = e.FileSystemEventArgs as RenamedEventArgs;
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
        /// 開始
        /// </summary>
        public void Start()
        {
            _commandEngine = new SerarchCommandEngine();
            _commandEngine.Initialize();

            if (_searchAreas != null) Collect();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _commandEngine.Dispose();
            _commandEngine = null;
        }

        /// <summary>
        /// 全てのコマンドの完了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync()
        {
            if (_commandEngine == null) throw new InvalidOperationException("engine stopped.");

            var command = new WaitCommand(this, null);
            _commandEngine.Enqueue(command);

            await command.WaitAsync();
        }


        /// <summary>
        /// 検索範囲の変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Areas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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
        /// ノード構築処理のキャンセルトークン
        /// </summary>
        private CancellationTokenSource _resetAreaCancellationTokenSource;


        /// <summary>
        /// ノード構築
        /// </summary>
        private void Collect()
        {
            if (_commandEngine == null) return;

            // one command only.
            _resetAreaCancellationTokenSource?.Cancel();
            _resetAreaCancellationTokenSource = new CancellationTokenSource();

            var command = new CollectCommand(this, new CollectCommandArgs() { Area = _searchAreas?.ToArray() });
            _commandEngine.Enqueue(command, _resetAreaCancellationTokenSource.Token);
        }

        //
        internal void Collect_Execute(CollectCommandArgs args, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // TODO: 設計不十分のため、現状ではコマンド実行中のキャンセル不可
            _core.Collect(args.Area, CancellationToken.None);
        }


        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public async Task<SearchResult> SearchAsync(string keyword, SearchOption option)
        {
            return await SearchAsync(keyword, option, CancellationToken.None);
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
            if (_commandEngine == null) throw new InvalidOperationException("engine stopped.");

            var command = new SearchCommand(this, new SearchExCommandArgs() { Keyword = keyword, Option = option.Clone() });
            _commandEngine.Enqueue(command, token);

            await command.WaitAsync(token);
            return command.SearchResult;
        }

        //
        internal SearchResult Search_Execute(SearchExCommandArgs args)
        {
            return new SearchResult(args.Keyword, args.Option, _core.Search(args.Keyword, args.Option));
        }



        /// <summary>
        /// 内部コマンド用：ノード追加
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal void AddNode(string root, string path)
        {
            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs()
            {
                ChangeType = NodeChangeType.Add,
                Root = root,
                Path = path
            });
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード削除
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal void RemoveNode(string root, string path)
        {
            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs()
            {
                ChangeType = NodeChangeType.Remove,
                Root = root,
                Path = path
            });
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
            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs()
            {
                ChangeType = NodeChangeType.Rename,
                Root = root,
                OldPath = oldPath,
                Path = newPath
            });
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード情報更新
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal void RefleshNode(string root, string path)
        {
            var command = new NodeChangeCommand(this, new NodeChangeCommandArgs()
            {
                ChangeType = NodeChangeType.Reflesh,
                Root = root,
                Path = path
            });
            _commandEngine.Enqueue(command);
        }

        /// <summary>
        /// 内部コマンド用：ノード変更
        /// </summary>
        /// <param name="args"></param>
        internal void NodeChange_Execute(NodeChangeCommandArgs args)
        {
            switch (args.ChangeType)
            {
                case NodeChangeType.Add:
                    _core.AddPath(args.Root, args.Path);
                    break;
                case NodeChangeType.Remove:
                    _core.RemovePath(args.Root, args.Path);
                    break;
                case NodeChangeType.Rename:
                    _core.RenamePath(args.Root, args.OldPath, args.Path);
                    break;
                case NodeChangeType.Reflesh:
                    _core.RefleshIndex(args.Root, args.Path);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
        }
        #endregion
    }

}
