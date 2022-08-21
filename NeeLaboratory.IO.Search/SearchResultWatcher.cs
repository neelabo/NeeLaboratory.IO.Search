using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

            _engine.Core.NodeChanged += Core_NodeChanged;
        }



        /// <summary>
        /// 検索結果変更
        /// </summary>
        public event EventHandler<SearchResultChangedEventArgs>? SearchResultChanged;




        /// <summary>
        /// ファイル変化イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Core_NodeChanged(object? sender, NodeChangedEventArgs e)
        {
            if (_disposedValue) return;

            var node = e.Node;
            if (node == null) return;

            if (_engine?.Core == null) return;

            if (e.Action == NodeChangedAction.Add)
            {
                try
                {
                    var items = _engine.Core?.Search(_result.Keyword, _result.SearchOption, node.AllNodes, CancellationToken.None);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            Logger.Trace($"Add: {item.Name}");
                            _result.Items.Add(item.Content);
                            SearchResultChanged?.Invoke(this, new SearchResultChangedEventArgs(NodeChangedAction.Add, item.Content));
                        }
                    }
                }
                catch (SearchKeywordException ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
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
                    var rename = (NodeRenamedEventArgs)e;
                    SearchResultChanged?.Invoke(this, new SearchResultRenamedEventArgs(NodeChangedAction.Rename, node.Content, rename.OldPath));
                }
                else
                {
                    try
                    {
                        var items = _engine.Core?.Search(_result.Keyword, _result.SearchOption, new List<Node>() { node }, CancellationToken.None);
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                Logger.Trace($"Add: {item.Name}");
                                _result.Items.Add(item.Content);
                                SearchResultChanged?.Invoke(this, new SearchResultChangedEventArgs(NodeChangedAction.Add, item.Content));
                            }
                        }
                    }
                    catch (SearchKeywordException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return;
                    }
                }
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
                    _engine.Core.NodeChanged -= Core_NodeChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
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

        /// <summary>
        /// 検索失敗時の例外
        /// </summary>
        public Exception? Exception => _result.Exception;

        #endregion
    }
}
