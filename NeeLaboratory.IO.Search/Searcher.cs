using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索コア 絞り込み
    /// </summary>
    public class Searcher : IDisposable
    {
        private readonly SearchKeyAnalyzer _searchKeyAnalyzer;
        private bool _disposedValue = false;
        private readonly SearchContext _context;


        public Searcher()
            : this(SearchContext.Default)
        {
        }

        public Searcher(SearchContext context)
        {
            _context = context;
            _searchKeyAnalyzer = new SearchKeyAnalyzer(_context.KeyOptions, _context.KeyOptionAlias);
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
        /// 検索キーリスト生成
        /// </summary>
        private List<SearchKey> CreateKeys(string source)
        {
            var keys = _searchKeyAnalyzer.Analyze(source)
                .Where(e => !string.IsNullOrEmpty(e.Format));

            if (!keys.Any())
            {
                return new List<SearchKey>();
            }

            keys = PreKeys.Concat(keys).Concat(PostKeys);

            ////Debug.WriteLine("--\n" + string.Join("\n", keys.Select(e => e.ToString())));
            return keys.ToList();
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        /// <param name="entries">検索対象</param>
        /// <param name="isSearchFolder">フォルダを検索対象に含めるフラグ</param>
        /// <returns></returns>
        public IEnumerable<ISearchItem> Search(string keyword, IEnumerable<ISearchItem> entries, CancellationToken token)
        {
            ThrowIfDisposed();
            token.ThrowIfCancellationRequested();

            var all = entries;

            // キーワード登録
            var keys = CreateKeys(keyword);

            if (keys == null || keys.Count == 0 || !entries.Any())
            {
                return Array.Empty<ISearchItem>();
            }

            // キーワードによる絞込
            foreach (var key in keys)
            {
                token.ThrowIfCancellationRequested();

                var filter = key.Filter.CreateFunc(key.Property, key.Format);

                switch (key.Conjunction)
                {
                    case SearchConjunction.And:
                        entries = entries.Where(e => filter.IsMatch(_context, e));
                        break;
                    case SearchConjunction.Or:
                        entries = entries.Union(all.Where(e => filter.IsMatch(_context, e)));
                        break;
                    case SearchConjunction.Not:
                        entries = entries.Where(e => !filter.IsMatch(_context, e));
                        break;
                }
            }

            return entries;
        }
    }

}
