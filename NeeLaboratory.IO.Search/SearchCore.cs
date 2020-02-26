using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索コア
    /// 検索フォルダのファイルをインデックス化して保存し、検索を行う
    /// </summary>
    internal class SearchCore : IDisposable
    {
        #region Fields

        private static Regex _regexNumber = new Regex(@"0*(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// ノード環境
        /// </summary>
        private SearchContext _context = new SearchContext();

        /// <summary>
        /// ノード群
        /// </summary>
        private Dictionary<string, NodeTree> _fileIndexDirectory;

        /// <summary>
        /// 検索キーワード解析
        /// </summary>
        private SearchKeyAnalyzer _searchKeyAnalyzer = new SearchKeyAnalyzer();

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchCore()
        {
            _fileIndexDirectory = new Dictionary<string, NodeTree>();
        }

        #endregion

        #region Events

        /// <summary>
        /// ファイルシステム変更イベント
        /// </summary>
        internal event EventHandler<NodeTreeFileSystemEventArgs> FileSystemChanged;

        /// <summary>
        /// ノード変更イベント
        /// </summary>
        public event EventHandler<NodeChangedEventArgs> NodeChanged;

        #endregion

        #region Properties

        /// <summary>
        /// ノード環境
        /// </summary>
        public SearchContext Context => _context;

        #endregion

        #region Methods

        #region Index

        /// <summary>
        /// 検索フォルダのインデックス化
        /// </summary>
        /// <param name="areas">検索フォルダ群</param>
        public void Collect(List<SearchArea> areas, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Debug.WriteLine($"Search: Index Collect: ...");

            if (areas.Count() == 0) return;

            var roots = new List<SearchArea>();

            foreach (var area in areas)
            {
                // 重複の除外。再帰フラグはONを優先。
                var member = roots.FirstOrDefault(e => e.Path == area.Path);
                if (member != null)
                {
                    member.IncludeSubdirectories = member.IncludeSubdirectories | area.IncludeSubdirectories;
                    continue;
                }

                // 他のパスに含まれるなら除外
                if (areas.Any(e => area != e && e.IncludeSubdirectories && area.Path.StartsWith(e.Path.TrimEnd('\\') + "\\")))
                {
                    continue;
                }

                roots.Add(area);

            }

            CollectCore(roots, token);

            Debug.WriteLine($"Search: Index Collect: {_context.TotalCount}");
        }

        /// <summary>
        /// 検索フォルダのインデックス化
        /// 更新分のみ
        /// </summary>
        private void CollectCore(List<SearchArea> areas, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var newDictionary = new Dictionary<string, NodeTree>();

            foreach (var area in areas)
            {
                _fileIndexDirectory.TryGetValue(area.Path, out NodeTree nodeTree);
                if (nodeTree is null || nodeTree.IncludeSubdirectories != area.IncludeSubdirectories)
                {
                    nodeTree = new NodeTree(area, _context);
                    nodeTree.FileSystemChanged += (s, e) => FileSystemChanged?.Invoke(s, e);
                }
                newDictionary.Add(area.Path, nodeTree);
            }

            _context.TotalCount = 0;

            ParallelOptions options = new ParallelOptions() { CancellationToken = token };
            Parallel.ForEach(newDictionary.Values, options, nodeTree =>
            {
                nodeTree.Collect(options.CancellationToken);
            });

            // 再登録されなかったパスの後処理を行う
            foreach (var nodeTree in _fileIndexDirectory.Values)
            {
                if (!newDictionary.ContainsValue(nodeTree))
                {
                    nodeTree.Dispose();
                }
            }

            // ノードの統合
            foreach (var nodeTree in newDictionary.Values)
            {
                if (!nodeTree.IncludeSubdirectories)
                {
                    foreach (var node in nodeTree.Root.Children.Where(e => e.IsDirectory).ToList())
                    {
                        if (newDictionary.TryGetValue(node.Path, out NodeTree sub))
                        {
                            if (sub.Root.Parent != null) throw new InvalidOperationException("検索エリア構成が不正");
                            nodeTree.Root.Children.Remove(node);
                            sub.Root.Parent = nodeTree.Root;
                            nodeTree.Root.Children.Add(sub.Root);
                        }
                    }
                }
            }


            _fileIndexDirectory = newDictionary;
            System.GC.Collect();
        }


        /// <summary>
        /// 開発用：ツリー表示
        /// </summary>
        [Conditional("DEBUG")]
        public void DumpTree(bool verbose)
        {
            foreach (var nodeTree in _fileIndexDirectory.Values)
            {
                if (nodeTree.IsChild)
                {
                    nodeTree.DumpTree(false);
                }
                else
                {
                    nodeTree.DumpTree(verbose);
                }
            }
        }

        /// <summary>
        /// ノード数取得
        /// </summary>
        /// <returns></returns>
        public int NodeCount()
        {
            return _fileIndexDirectory.Sum(e => e.Value.IsChild ? 0 : e.Value.NodeCount());
        }

        /// <summary>
        /// インデックス追加
        /// </summary>
        /// <param name="root">検索フォルダ</param>
        /// <param name="paths">追加パス</param>
        public Node AddPath(string root, string path, CancellationToken token)
        {
            if (!_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].AddNode(path, token);
            NodeChanged?.Invoke(this, new NodeChangedEventArgs(NodeChangedAction.Add, node));
            return node;
        }

        /// <summary>
        /// インデックス削除
        /// </summary>
        /// <param name="root">検索フォルダ</param>
        /// <param name="path">削除パス</param>
        public Node RemovePath(string root, string path)
        {
            if (!_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].RemoveNode(path);
            NodeChanged?.Invoke(this, new NodeChangedEventArgs(NodeChangedAction.Remove, node));

            return node;
        }

        /// <summary>
        /// リネーム
        /// </summary>
        /// <param name="root">所属するツリー。nullの場合はoldFileNameから推定する</param>
        /// <param name="oldFileName"></param>
        /// <param name="newFilename"></param>
        public Node RenamePath(string root, string oldFileName, string newFileName)
        {
            root = root ?? GetRoot(oldFileName);
            if (root == null || !_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].Rename(oldFileName, newFileName);
            if (node != null)
            {
                NodeChanged?.Invoke(this, new NodeChangedEventArgs(NodeChangedAction.Rename, node) { OldPath = oldFileName });
            }

            return node;
        }

        /// <summary>
        /// インデックスの情報更新
        /// </summary>
        /// <param name="root">所属するツリー。nullの場合はパスから推定する</param>
        /// <param name="path">変更するインデックスのパス</param>
        public void RefleshIndex(string root, string path)
        {
            root = root ?? GetRoot(path);
            if (root == null || !_fileIndexDirectory.ContainsKey(root))
            {
                return;
            }

            _fileIndexDirectory[root].RefleshNode(path);
        }

        /// <summary>
        /// パスが所属するRoot名を返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetRoot(string path)
        {
            if (path == null) return null;
            return _fileIndexDirectory.Keys.FirstOrDefault(e => path.StartsWith(e.TrimEnd('\\') + "\\"));
        }

        #endregion

        #region Search

        /// <summary>
        /// 単語区切り用の正規表現生成
        /// </summary>
        private static string GetNotCodeBlockRegexString(char c)
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
        private static string ToFuzzyNumberRegex(string source)
        {
            return _regexNumber.Replace(source, match => "0*" + match.Groups[1]);
        }

        /// <summary>
        /// 検索キーリスト生成
        /// </summary>
        private List<SearchKey> CreateKeys(string source, SearchOption option)
        {
            var keys = _searchKeyAnalyzer.Analyze(source)
                .Where(e => !string.IsNullOrEmpty(e.Word))
                ////.Select(e => ValidateKey(e))
                .ToList();

            Debug.WriteLine("--\n" + string.Join("\n", keys.Select(e => e.ToString()))); // ##
            return keys;
        }

        /// <summary>
        /// すべてのNodeを走査
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> AllNodes
        {
            get
            {
                foreach (var nodeTree in _fileIndexDirectory.Values)
                {
                    if (nodeTree.Root != null && !nodeTree.IsChild)
                    {
                        foreach (var node in nodeTree.Root.AllChildren)
                            yield return node;
                    }
                }
            }
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public ObservableCollection<NodeContent> Search(string keyword, SearchOption option, CancellationToken token)
        {
            var items = new ObservableCollection<NodeContent>(Search(keyword, option, AllNodes, token).Select(e => e.Content));

            // 複数スレッドからコレクション操作できるようにする
            //BindingOperations.EnableCollectionSynchronization(items, new object());

            return items;
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        /// <param name="entries">検索対象</param>
        /// <param name="isSearchFolder">フォルダを検索対象に含めるフラグ</param>
        /// <returns></returns>
        public IEnumerable<Node> Search(string keyword, SearchOption option, IEnumerable<Node> entries, CancellationToken token)
        {
            var all = entries;

            // pushpin保存
            var pushpins = entries.Where(f => f.IsPushPin);

            // キーワード無し
            if (string.IsNullOrWhiteSpace(keyword)) return pushpins;

            // キーワード登録
            var keys = CreateKeys(keyword, option);
            if (keys == null || keys.Count == 0)
            {
                return pushpins;
            }

            // キーワードによる絞込
            foreach (var key in keys)
            {
                token.ThrowIfCancellationRequested();

                var match = CreateMatchable(key);

                switch (key.Conjunction)
                {
                    case SearchConjunction.And:
                        entries = entries.Where(e => match.IsMatch(e));
                        break;
                    case SearchConjunction.Or:
                        entries = entries.Union(all.Where(e => match.IsMatch(e)));
                        break;
                    case SearchConjunction.Not:
                        entries = entries.Where(e => !match.IsMatch(e));
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

        private IMatchable<Node> CreateMatchable(SearchKey key)
        {
            switch (key.Pattern)
            {
                default:
                    throw new NotSupportedException();
                case SearchPattern.Exact:
                    return new ExactMatch(key);
                case SearchPattern.Word:
                    return new WordMatch(key);
                case SearchPattern.Standard:
                    return new StandardMatch(key);
                case SearchPattern.RegularExpression:
                    return new RegularExpressionMatch(key);
                case SearchPattern.RegularExpressionIgnoreCase:
                    return new RegularExpressionIgnoreCaseMatch(key);
                case SearchPattern.Since:
                    return new SinceMatch(key);
                case SearchPattern.Until:
                    return new UntilMatch(key);
            }
        }

        interface IMatchable<T>
        {
            bool IsMatch(T e);
        }

        class SinceMatch : IMatchable<Node>
        {
            private DateTime _since;

            public SinceMatch(SearchKey key)
            {
                try
                {
                    _since = DateTime.Parse(key.Word);
                }
                catch (Exception ex)
                {
                    throw new SearchKeywordDateTimeException($"DateTime parse error: {key.Word}", ex);
                }
            }

            public bool IsMatch(Node e)
            {
                return _since <= e.LastWriteTime;
            }
        }

        class UntilMatch : IMatchable<Node>
        {
            private DateTime _until;

            public UntilMatch(SearchKey key)
            {
                try
                {
                    _until = DateTime.Parse(key.Word);
                }
                catch (Exception ex)
                {
                    throw new SearchKeywordDateTimeException($"DateTime parse error: {key.Word}", ex);
                }
            }

            public bool IsMatch(Node e)
            {
                return e.LastWriteTime <= _until;
            }
        }

        class RegularExpressionMatch : IMatchable<Node>
        {
            private Regex _regex;

            public RegularExpressionMatch(SearchKey key)
            {
                try
                {
                    _regex = new Regex(key.Word, RegexOptions.Compiled);
                }
                catch (Exception ex)
                {
                    throw new SearchKeywordRegularExpressionException($"RegularExpression error: {key.Word}", ex);
                }
            }

            public bool IsMatch(Node e)
            {
                return _regex.Match(e.Name).Success;
            }
        }

        class RegularExpressionIgnoreCaseMatch : IMatchable<Node>
        {
            private Regex _regex;

            public RegularExpressionIgnoreCaseMatch(SearchKey key)
            {
                try
                {
                    _regex = new Regex(key.Word, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                catch (Exception ex)
                {
                    throw new SearchKeywordRegularExpressionException($"RegularExpression error: {key.Word}", ex);
                }
            }

            public bool IsMatch(Node e)
            {
                return _regex.Match(e.Name).Success;
            }
        }

        class ExactMatch : IMatchable<Node>
        {
            private Regex _regex;

            public ExactMatch(SearchKey key)
            {
                var s = Regex.Escape(key.Word);
                _regex = new Regex(s, RegexOptions.Compiled);
            }

            public bool IsMatch(Node e)
            {
                return _regex.Match(e.Name).Success;
            }
        }

        class WordMatch : IMatchable<Node>
        {
            private Regex _regex;

            public WordMatch(SearchKey key)
            {
                var s = key.Word;
                var first = GetNotCodeBlockRegexString(s.First());
                var last = GetNotCodeBlockRegexString(s.Last());
                s = Node.ToNormalisedWord(s, false);
                s = Regex.Escape(s);
                s = ToFuzzyNumberRegex(s);
                if (first != null) s = $"(^|{first}){s}";
                if (last != null) s = $"{s}({last}|$)";

                _regex = new Regex(s, RegexOptions.Compiled);
            }

            public bool IsMatch(Node e)
            {
                return _regex.Match(e.NormalizedUnitWord).Success;
            }
        }

        class StandardMatch : IMatchable<Node>
        {
            private Regex _regex;

            public StandardMatch(SearchKey key)
            {
                var s = key.Word;
                s = Node.ToNormalisedWord(s, true);
                s = Regex.Escape(s);
                s = ToFuzzyNumberRegex(s);
                _regex = new Regex(s, RegexOptions.Compiled);
            }

            public bool IsMatch(Node e)
            {
                return _regex.Match(e.NormalizedFazyWord).Success;
            }
        }

        #endregion

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_fileIndexDirectory != null)
                    {
                        foreach (var pair in _fileIndexDirectory)
                        {
                            pair.Value.Dispose();
                        }
                    }
                    _fileIndexDirectory = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
