using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索コア 絞り込み
    /// </summary>
    public class SearchCore : IDisposable
    {
        private static readonly Regex _regexNumber = new(@"0*(\d+)", RegexOptions.Compiled);

        private readonly SearchKeyAnalyzer _searchKeyAnalyzer;
        private bool _disposedValue = false;
        private SearchValueContext _context;


        public SearchCore()
            : this(SearchValueContext.Default)
        {
        }

        public SearchCore(SearchValueContext context)
        {
            _context = context;
            _searchKeyAnalyzer = new SearchKeyAnalyzer(_context.Options, _context.OptionAlias);
        }

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
        /// 単語区切り用の正規表現生成
        /// </summary>
        public static string? GetNotCodeBlockRegexString(char c)
        {
            if ('0' <= c && c <= '9')
                return @"\D";
            //else if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
            //    return @"\P{L}";
            else if (0x3040 <= c && c <= 0x309F)
                return @"\P{IsHiragana}";
            else if (0x30A0 <= c && c <= 0x30FF)
                return @"\P{IsKatakana}";
            else if ((0x3400 <= c && c <= 0x4DBF) || (0x4E00 <= c && c <= 0x9FFF) || (0xF900 <= c && c <= 0xFAFF))
                return @"[^\p{IsCJKUnifiedIdeographsExtensionA}\p{IsCJKUnifiedIdeographs}\p{IsCJKCompatibilityIdeographs}]";
            else if (new Regex(@"^\p{L}").IsMatch(c.ToString()))
                return @"\P{L}";
            else
                return null;
        }

        /// <summary>
        /// (数値)部分を0*(数値)という正規表現に変換
        /// </summary>
        public static string ToFuzzyNumberRegex(string source)
        {
            return _regexNumber.Replace(source, match => "0*" + match.Groups[1]);
        }

        /// <summary>
        /// 検索キーリスト生成
        /// </summary>
        private List<SearchKey> CreateKeys(string source)
        {
            var keys = _searchKeyAnalyzer.Analyze(source)
                .Where(e => !string.IsNullOrEmpty(e.Word))
                ////.Select(e => ValidateKey(e))
                .ToList();

            ////Debug.WriteLine("--\n" + string.Join("\n", keys.Select(e => e.ToString())));
            return keys;
        }


        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        /// <param name="entries">検索対象</param>
        /// <param name="isSearchFolder">フォルダを検索対象に含めるフラグ</param>
        /// <returns></returns>
        public IEnumerable<ISearchItem> Search(string keyword, SearchOption option, IEnumerable<ISearchItem> entries, CancellationToken token)
        {
            ThrowIfDisposed();
            token.ThrowIfCancellationRequested();

            var all = entries;

            // pushpin保存
            var pushpins = entries.Where(f => f.IsPushPin);

            // キーワード無し
            if (string.IsNullOrWhiteSpace(keyword)) return pushpins;

            // キーワード登録
            var keys = CreateKeys(keyword);
            if (keys == null || keys.Count == 0)
            {
                return pushpins;
            }

            // キーワードによる絞込
            foreach (var key in keys)
            {
                token.ThrowIfCancellationRequested();

                var match = key.Pattern.CreateFunc(key.Property, key.Word);

                switch (key.Conjunction)
                {
                    case SearchConjunction.And:
                        entries = entries.Where(e => match.IsMatch(_context, e)).ToList();
                        break;
                    case SearchConjunction.Or:
                        entries = entries.Union(all.Where(e => match.IsMatch(_context, e)));
                        break;
                    case SearchConjunction.Not:
                        entries = entries.Where(e => !match.IsMatch(_context, e));
                        break;
                }
            }

            // ディレクトリ除外
            if (!option.AllowFolder)
            {
                entries = entries.Where(f => !f.IsDirectory);
            }

            // pushpin除外
            entries = entries.Where(f => !f.IsPushPin);

            // pushpinを先頭に連結して返す
            return pushpins.Concat(entries);
        }
    }

}
