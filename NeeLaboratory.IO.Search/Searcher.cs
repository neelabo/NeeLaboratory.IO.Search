using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索本体
    /// </summary>
    public class Searcher : IDisposable
    {
        private readonly SearchKeyAnalyzer _searchKeyAnalyzer;
        private bool _disposedValue = false;
        private readonly SearchContext _context;


        public Searcher(SearchContext context)
        {
            _context = context;
            _searchKeyAnalyzer = new SearchKeyAnalyzer(_context.KeyOptions, _context.KeyAlias);
        }


        public List<SearchKey> PreKeys { get; set; } = new();

        public List<SearchKey> PostKeys { get; set; } = new();


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
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 生成キー解析
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        /// <exception cref="SearchKeywordException">フォーマットエラー</exception>
        public IEnumerable<SearchKey> Analyze(string keyword)
        {
            return _searchKeyAnalyzer.Analyze(keyword);
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        /// <param name="entries">検索対象</param>
        /// <returns></returns>
        public IEnumerable<ISearchItem> Search(string keyword, IEnumerable<ISearchItem> entries, CancellationToken token)
        {
            IEnumerable<SearchKey> keys;
            try
            {
                keys = Analyze(keyword);
            }
            catch (Exception)
            {
                return entries;
            }

            if (!keys.Any())
            {
                return entries;
            }

            return Search(keys, entries, token);
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keys">検索キー</param>
        /// <param name="entries">検索対象</param>
        /// <returns></returns>
        public IEnumerable<ISearchItem> Search(IEnumerable<SearchKey> keys, IEnumerable<ISearchItem> entries, CancellationToken token)
        {
            ThrowIfDisposed();
            token.ThrowIfCancellationRequested();

            var all = entries;

            // キーワードやエントリーが空ならそのまま返す
            if (!keys.Any() || !entries.Any())
            {
                return all;
            }

            // 定義キーを追加
            var fixedKeys = PreKeys.Concat(keys).Concat(PostKeys);

            // キーワードによる絞込
            foreach (var key in fixedKeys)
            {
                token.ThrowIfCancellationRequested();

                var filter = key.Filter.CreateFunc(key.Property, key.PropertyParameter, key.Format);

                switch (key.Conjunction)
                {
                    case SearchConjunction.And:
                        entries = entries.Where(e => filter.IsMatch(_context, e, token));
                        break;
                    case SearchConjunction.Or:
                        entries = entries.Union(all.Where(e => filter.IsMatch(_context, e, token)));
                        break;
                    case SearchConjunction.Not:
                        entries = entries.Where(e => !filter.IsMatch(_context, e, token));
                        break;
                }
            }

            return entries;
        }
    }

}
