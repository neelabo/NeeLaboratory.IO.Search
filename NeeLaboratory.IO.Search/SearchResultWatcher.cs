using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class SearchResultWatcher : IDisposable, ISearchResult
    {
        // Logger
        private static Utility.Logger Logger => Development.Logger;

        #region Fields

        /// <summary>
        /// 所属する検索エンジン
        /// </summary>
        private SearchEngine _engine;

        /// <summary>
        /// 監視する検索結果
        /// </summary>
        private SearchResult _result;

        #endregion

        #region Constructors

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

        #endregion

        #region Events

        /// <summary>
        /// 検索結果変更
        /// </summary>
        public event EventHandler<SearchResultChangedEventArgs> SearchResultChanged;

        #endregion

        #region Properties

        #endregion

        #region Methods

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
            if (_engine.Core != null)
            {
                _engine.Core.NodeChanged -= Core_NodeChanged;
            }
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
                    SearchResultChanged?.Invoke(this, new SearchResultChangedEventArgs(NodeChangedAction.Add, item.Content));
                }
            }
            else if (e.Action == NodeChangedAction.Remove)
            {
                var items = _result.Items.Where(item => item.IsRemoved).ToList();
                foreach (var item in items)
                {
                    Logger.Trace($"Remove: {item.Name}");
                    _result.Items.Remove(item);
                    SearchResultChanged?.Invoke(this, new SearchResultChangedEventArgs(NodeChangedAction.Remove, item));
                }
            }
            else if (e.Action == NodeChangedAction.Rename)
            {
                if (_result.Items.Contains(node.Content))
                {
                    SearchResultChanged?.Invoke(this, new SearchResultChangedEventArgs(NodeChangedAction.Rename, node.Content) { OldPath = e.OldPath });
                }
                else
                {
                    var items = _engine.Core.Search(_result.Keyword, _result.SearchOption, new List<Node>() { node });
                    foreach (var item in items)
                    {
                        Logger.Trace($"Add: {item.Name}");
                        _result.Items.Add(item.Content);
                        SearchResultChanged?.Invoke(this, new SearchResultChangedEventArgs(NodeChangedAction.Add, item.Content));
                    }
                }
            }
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // マネージ状態を破棄します (マネージ オブジェクト)。
                    Stop();
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
        }
        #endregion

        #region ISearchResult Support

        /// <summary>
        /// 検索結果項目
        /// </summary>
        public ObservableCollection<NodeContent> Items => _result.Items;

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword => _result.Keyword;

        /// <summary>
        /// 検索オプション
        /// </summary>
        public SearchOption SearchOption => _result.SearchOption;

        #endregion
    }
}
