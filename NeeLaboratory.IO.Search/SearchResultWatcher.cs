using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 結果の変更を監視する。
    /// ファイルの状態が変化を検索結果に反映させる
    /// </summary>
    public class SearchResultWatcher : IDisposable
    {
        // Logger
        private static Utility.Logger Logger => Development.Logger;

        /// <summary>
        /// 所属する検索エンジン
        /// </summary>
        private SearchEngine _engine;

        /// <summary>
        /// 監視する検索結果
        /// </summary>
        private SearchResult _result;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="result"></param>
        public SearchResultWatcher(SearchEngine engine, SearchResult result)
        {
            _engine = engine;
            _result = result;
        }

        /// <summary>
        /// 開始
        /// </summary>
        public void Start()
        {
            _engine.Core.NodeChanged += Core_NodeChanged;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _engine.Core.NodeChanged -= Core_NodeChanged;
        }

        /// <summary>
        /// ファイル変化イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Core_NodeChanged(object sender, NodeChangedEventArgs e)
        {
            var node = e.Node;
            if (node == null) return;

            if (e.Action == NodeChangedAction.Add)
            {
                var items = _engine.Core.Search(_result.Keyword, _result.SearchOption, node.AllNodes);
                foreach (var item in items)
                {
                    Logger.Trace($"Add: {item.Name}");
                    _result.Items.Add(item.Content);
                }
            }
            else if (e.Action == NodeChangedAction.Remove)
            {
                var items = _result.Items.Where(item => item.IsRemoved).ToList();
                foreach (var item in items)
                {
                    Logger.Trace($"Remove: {item.Name}");
                    _result.Items.Remove(item);
                }
            }
            else if (e.Action == NodeChangedAction.Rename)
            {
                if (!_result.Items.Contains(node.Content))
                {
                    var items = _engine.Core.Search(_result.Keyword, _result.SearchOption, new List<Node>() { node });
                    foreach (var item in items)
                    {
                        Logger.Trace($"Add: {item.Name}");
                        _result.Items.Add(item.Content);
                    }
                }
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
                    // マネージ状態を破棄します (マネージ オブジェクト)。
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
