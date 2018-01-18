using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// コマンドデータ基底
    /// </summary>
    internal class CommandArgs
    {
    }

    /// <summary>
    /// コマンド基底
    /// </summary>
    internal class CommandBase : Utility.CommandBase
    {
        /// <summary>
        /// 所属する検索エンジン
        /// </summary>
        protected SearchEngine _target;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="target"></param>
        public CommandBase(SearchEngine target)
        {
            _target = target;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="target"></param>
        /// <param name="token"></param>
        public CommandBase(SearchEngine target, CancellationToken token) : base(token)
        {
            _target = target;
        }

        /// <summary>
        /// コマンド実行(基底)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// ノード構築コマンドデータ
    /// </summary>
    internal class CollectCommandArgs : CommandArgs
    {
        /// <summary>
        /// 検索エリアのパス
        /// </summary>
        public string[] Area { get; set; }
    }

    /// <summary>
    /// ノード構築コマンド
    /// </summary>
    internal class CollectCommand : CommandBase
    {
        CollectCommandArgs _args;

        //
        public CollectCommand(SearchEngine target, CollectCommandArgs args) : base(target)
        {
            _args = args;
        }

        //
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            _target.Collect_Execute(_args, token);
        }
    }


    /// <summary>
    /// 検索コマンドデータ
    /// </summary>
    internal class SearchExCommandArgs : CommandArgs
    {
        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 検索オプション
        /// </summary>
        public SearchOption Option { get; set; }
    }

    /// <summary>
    /// 検索コマンド
    /// </summary>
    internal class SearchCommand : CommandBase
    {
        private SearchExCommandArgs _args;

        /// <summary>
        /// 検索結果
        /// </summary>
        public SearchResult SearchResult { get; private set; }

        //
        public SearchCommand(SearchEngine target, SearchExCommandArgs args) : base(target)
        {
            _args = args;
        }

        //
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            SearchResult = _target.Search_Execute(_args, token);
        }
    }

    /// <summary>
    /// 待機コマンド用（何も処理しない）
    /// </summary>
    internal class WaitCommand : CommandBase
    {
        //
        public WaitCommand(SearchEngine target, CommandArgs args) : base(target)
        {
        }

        //
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
        }
    }


    /// <summary>
    /// ノード変更の種類
    /// </summary>
    internal enum NodeChangeType
    {
        Add,
        Remove,
        Rename,
        Reflesh,
    }

    /// <summary>
    /// ノード変更コマンドデータ 
    /// </summary>
    internal class NodeChangeCommandArgs : CommandArgs
    {
        public NodeChangeType ChangeType { get; set; }
        public string Root { get; set; }
        public string Path { get; set; }
        public string OldPath { get; set; }
    }

    /// <summary>
    /// ノード変更コマンド
    /// </summary>
    internal class NodeChangeCommand : CommandBase
    {
        private NodeChangeCommandArgs _args;

        //
        public NodeChangeCommand(SearchEngine target, NodeChangeCommandArgs args) : base(target)
        {
            _args = args;
        }

        //
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            _target.NodeChange_Execute(_args, token);
        }

        //
        public override string ToString()
        {
            if (_args.ChangeType == NodeChangeType.Rename)
            {
                return $"{nameof(NodeChangeCommand)}: {_args.ChangeType}: {_args.OldPath} -> {_args.Path}";
            }
            else
            {
                return $"{nameof(NodeChangeCommand)}: {_args.ChangeType}: {_args.Path}";
            }
        }
    }


 
    /// <summary>
    /// コマンドエンジン状態
    /// </summary>
    public enum SearchEngineState
    {
        /// <summary>
        /// 処理なし
        /// </summary>
        Idle,

        /// <summary>
        /// 収拾中
        /// </summary>
        Collect,

        /// <summary>
        /// 検索中
        /// </summary>
        Search,

        /// <summary>
        /// その他処理中
        /// </summary>
        Etc,
    }


    /// <summary>
    /// コマンドエンジン
    /// </summary>
    internal class SerarchCommandEngine : Utility.CommandEngine
    {
        /// <summary>
        /// 状態取得
        /// </summary>
        public SearchEngineState State
        {
            get
            {
                var current = _command;
                if (current == null && !_queue.Any())
                    return SearchEngineState.Idle;
                else if (current is CollectCommand)
                    return SearchEngineState.Collect;
                else if (current is SearchCommand)
                    return SearchEngineState.Search;
                else
                    return SearchEngineState.Etc;
            }
        }

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        internal void Enqueue(CommandBase command, CancellationToken token)
        {
            command.CancellationToken = token;
            Enqueue(command);
        }
    }
}
