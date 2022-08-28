using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search.Utility
{
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// WaitHandle待ちのタスク化。
        /// </summary>
        /// <example>
        /// await ManualResetEventSlim.WaitHandle.AsTask();
        /// </example>
        /// <remarks>
        /// https://docs.microsoft.com/ja-jp/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types
        /// </remarks>
        public static Task AsTask(this WaitHandle waitHandle)
        {
            if (waitHandle == null) throw new ArgumentNullException(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, delegate { tcs.TrySetResult(true); }, null, -1, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
            return t;
        }
    }

}
