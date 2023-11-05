using System;
using System.Collections.Generic;
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
        public CollectCommandArgs(List<SearchArea> area)
        {
            Area = area;
        }

        /// <summary>
        /// 検索エリアのパス
        /// </summary>
        public List<SearchArea> Area { get; set; }
    }


    /// <summary>
    /// ノード構築コマンド
    /// </summary>
    internal class CollectCommand : CommandBase
    {
        readonly CollectCommandArgs _args;

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



    #region SearchCommand

    /// <summary>
    /// 検索コマンドデータ
    /// </summary>
    internal class SearchExCommandArgs : CommandArgs
    {
        public SearchExCommandArgs(string keyword)
        {
            Keyword = keyword;
        }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword { get; set; }
    }

    /// <summary>
    /// 検索コマンド
    /// </summary>
    internal class SearchCommand : CommandBase
    {
        private readonly SearchExCommandArgs _args;

        public SearchCommand(SearchEngine target, SearchExCommandArgs args) : base(target)
        {
            _args = args;
        }


        /// <summary>
        /// 検索結果
        /// </summary>
        public SearchResult? SearchResult { get; private set; }


        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            SearchResult = _target.Search_Execute(_args, token);
        }
    }

    #endregion

    #region MultiSearchCommand

    /// <summary>
    /// 検索コマンドデータ
    /// </summary>
    internal class MultiSearchExCommandArgs : CommandArgs
    {
        public MultiSearchExCommandArgs(List<string> keywords)
        {
            Keywords = keywords;
        }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public List<string> Keywords { get; set; }
    }

    /// <summary>
    /// 検索コマンド
    /// </summary>
    internal class MultiSearchCommand : CommandBase
    {
        private readonly MultiSearchExCommandArgs _args;

        /// <summary>
        /// 検索結果
        /// </summary>
        public List<SearchResult>? SearchResults { get; private set; }

        public MultiSearchCommand(SearchEngine target, MultiSearchExCommandArgs args) : base(target)
        {
            _args = args;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
            SearchResults = _target.MultiSearch_Execute(_args, token);
        }
    }

    #endregion

    #region WaitCommand

    /// <summary>
    /// 待機コマンド用（何も処理しない）
    /// </summary>
    internal class WaitCommand : CommandBase
    {
        //
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        public WaitCommand(SearchEngine target, CommandArgs args) : base(target)
        {
        }

        //
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();
        }
    }

    #endregion

    #region NodeChangeCommand

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
        public NodeChangeCommandArgs(NodeChangeType changeType, string? root, string path)
        {
            ChangeType = changeType;
            Root = root;
            Path = path;
        }

        public NodeChangeType ChangeType { get; set; }
        public string? Root { get; set; }
        public string Path { get; set; }
    }

    internal class NodeRenameCommandArgs : NodeChangeCommandArgs
    {
        public NodeRenameCommandArgs(NodeChangeType changeType, string? root, string path, string oldPath) : base(changeType, root, path)
        {
            if (changeType != NodeChangeType.Rename) throw new ArgumentException("changeType must be Rename");

            OldPath = oldPath;
        }

        public string OldPath { get; set; }
    }

    /// <summary>
    /// ノード変更コマンド
    /// </summary>
    internal class NodeChangeCommand : CommandBase
    {
        private readonly NodeChangeCommandArgs _args;

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
                var remame = (NodeRenameCommandArgs)_args;
                return $"{nameof(NodeChangeCommand)}: {_args.ChangeType}: {remame.OldPath} -> {_args.Path}";
            }
            else
            {
                return $"{nameof(NodeChangeCommand)}: {_args.ChangeType}: {_args.Path}";
            }
        }
    }

    #endregion
}
