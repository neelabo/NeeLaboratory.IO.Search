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
    internal class CommandEngine : IDisposable
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger _logger;
        public Logger Logger => _logger;

        /// <summary>
        /// ワーカータスクのキャンセルトークン
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// 予約コマンド存在通知
        /// </summary>
        private readonly ManualResetEventSlim _ready = new(false);

        /// <summary>
        /// lock
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 予約コマンドリスト
        /// </summary>
        protected Queue<ICommand> _queue = new();

        /// <summary>
        /// 実行中コマンド
        /// </summary>
        protected ICommand? _command;



        public CommandEngine()
        {
            _logger = new Logger(nameof(CommandEngine));

            var thread = new Thread(Worker)
            {
                Name = GetType().FullName,
                IsBackground = true
            };
            thread.Start();
        }



        /// <summary>
        /// コマンド登録
        /// </summary>
        /// <param name="command"></param>
        public virtual void Enqueue(ICommand command)
        {
            ThrowIfDisposed();

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
        /// ワーカー
        /// </summary>
        private void Worker()
        {
            try
            {
                WorkerAsync(_cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Logger.TraceEvent(TraceEventType.Critical, 0, $"!!!! EXCEPTION !!!!: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// ワーカータスク
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private void WorkerAsync(CancellationToken token)
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
                        _command?.ExecuteAsync().Wait(token);
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
                    lock (_lock)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();

                        _ready.Dispose();
                    }
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
