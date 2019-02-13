// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    #region CommandBase

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

    #endregion

    #region CollectCommand

    /// <summary>
    /// ノード構築コマンドデータ
    /// </summary>
    internal class CollectCommandArgs : CommandArgs
    {
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

    #endregion

    #region SearchCommand

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

    #endregion

    #region WaitCommand

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

    #endregion
}
