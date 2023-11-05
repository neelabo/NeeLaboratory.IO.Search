using System.Linq;
using System.Threading;

namespace NeeLaboratory.IO.Search.FileSearch
{

    /// <summary>
    /// コマンドエンジン
    /// </summary>
    internal class SearchCommandEngine : Utility.CommandEngine
    {
        /// <summary>
        /// 状態取得
        /// </summary>
        public SearchCommandEngineState State
        {
            get
            {
                var current = _command;
                if (current == null && !_queue.Any())
                    return SearchCommandEngineState.Idle;
                else if (current is CollectCommand)
                    return SearchCommandEngineState.Collect;
                else if (current is SearchCommand)
                    return SearchCommandEngineState.Search;
                else
                    return SearchCommandEngineState.Etc;
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
