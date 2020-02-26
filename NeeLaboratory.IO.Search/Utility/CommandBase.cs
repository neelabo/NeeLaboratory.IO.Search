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
    /// コマンド実行結果
    /// </summary>
    public enum CommandResult
    {
        None,
        Completed,
        Canceled,
    }


    /// <summary>
    /// コマンドインターフェイス
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
    }


    /// <summary>
    /// コマンド基底
    /// キャンセル、終了待機対応
    /// </summary>
    public abstract class CommandBase : ICommand, IDisposable
    {
        // キャンセルトークン
        private CancellationToken _cancellationToken;
        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
            set { _cancellationToken = value; }
        }

        // コマンド終了通知
        private ManualResetEventSlim _complete = new ManualResetEventSlim(false);

        // コマンド実行結果
        private CommandResult _result;
        public CommandResult Result
        {
            get { return _result; }
            set { _result = value; _complete.Set(); }
        }

        // キャンセル可能フラグ
        public bool CanBeCanceled => _cancellationToken.CanBeCanceled;

        /// <summary>
        /// constructor
        /// </summary>
        public CommandBase()
        {
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        public CommandBase(CancellationToken token)
        {
            _cancellationToken = token;
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            if (_complete.IsSet) return;

            // cancel ?
            if (_cancellationToken.IsCancellationRequested)
            {
                Result = CommandResult.Canceled;
                return;
            }

            // execute
            try
            {
                await ExecuteAsync(_cancellationToken);
                Result = CommandResult.Completed;
            }
            catch (OperationCanceledException)
            {
                Result = CommandResult.Canceled;
                Debug.WriteLine($"command {this}: canceled.");
                OnCanceled();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"command {this}: excepted!!");
                OnException(e);
                throw;
            }
        }

        /// <summary>
        /// コマンド終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync()
        {
            await Task.Run(() => _complete.Wait());
        }

        static int _serial;

        /// <summary>
        /// コマンド終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            var serial = _serial++;

            await Task.Run(() => _complete.Wait(token), token);
        }


        /// <summary>
        /// コマンド実行(abstract)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task ExecuteAsync(CancellationToken token);


        /// <summary>
        /// コマンドキャンセル時
        /// </summary>
        protected virtual void OnCanceled()
        {
        }

        /// <summary>
        /// コマンド例外時
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnException(Exception e)
        {
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_complete != null)
                    {
                        _complete.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
