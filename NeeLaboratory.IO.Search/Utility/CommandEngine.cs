// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search.Utility
{
    /// <summary>
    /// コマンドエンジン
    /// </summary>
    public class CommandEngine : IDisposable
    {
        /// <summary>
        /// Logger
        /// </summary>
        private Logger _logger;
        public Logger Logger => _logger;

        /// <summary>
        /// ワーカータスクのキャンセルトークン
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 予約コマンド存在通知
        /// </summary>
        private ManualResetEventSlim _ready = new ManualResetEventSlim(false);

        /// <summary>
        /// lock
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// 予約コマンドリスト
        /// </summary>
        protected Queue<ICommand> _queue = new Queue<ICommand>();

        /// <summary>
        /// 実行中コマンド
        /// </summary>
        protected ICommand _command;


        /// <summary>
        /// constructor
        /// </summary>
        public CommandEngine()
        {
            _logger = new Logger(nameof(CommandEngine));
        }


        /// <summary>
        /// コマンド登録
        /// </summary>
        /// <param name="command"></param>
        public virtual void Enqueue(ICommand command)
        {
            lock (_lock)
            {
                if (OnEnqueueing(command))
                {
                    _queue.Enqueue(command);
                    OnEnqueued(command);
                    _ready.Set();
                }
            }
        }

        /// <summary>
        /// Queue登録前の処理
        /// </summary>
        /// <param name="command"></param>
        protected virtual bool OnEnqueueing(ICommand command)
        {
            return true;
        }

        /// <summary>
        /// Queue登録後の処理
        /// </summary>
        protected virtual void OnEnqueued(ICommand command)
        {
            // nop.
        }

        /// <summary>
        /// 現在のコマンド数
        /// </summary>
        public int Count
        {
            get { return _queue.Count + (_command != null ? 1 : 0); }
        }


        /// <summary>
        /// 初期化
        /// ワーカータスク起動
        /// </summary>
        public void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                // TODO: ここでの例外が補足できていない。致命的！
                // システム停止レベル！
                try
                {
                    await WorkerAsync(_cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Logger.TraceEvent(TraceEventType.Critical, 0, $"!!!! EXCEPTION !!!!: {e.Message}\n{e.StackTrace}");
                    Debugger.Break();
                    throw;
                }
            });
        }

        /// <summary>
        /// ワーカータスク終了
        /// </summary>
        public virtual void Dispose()
        {
            lock (_lock)
            {
                // 停止命令発行
                _cancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// ワーカータスク
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task WorkerAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _ready.Wait(token);

                    while (!token.IsCancellationRequested)
                    {
                        lock (_lock)
                        {
                            if (_queue.Count <= 0)
                            {
                                _command = null;
                                _ready.Reset();
                                break;
                            }

                            _command = _queue.Dequeue();
                        }

                        Logger.Trace($"{_command}: start... :rest={_queue.Count}");
                        await _command?.ExecuteAsync();
                        Logger.Trace($"{_command}: done.");
                        if (_command is CommandBase cmd)
                        {
                            Logger.Trace($"{cmd}: result={cmd.Result}");
                        }

                        _command = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _command = null;
            }
        }
    }

}
