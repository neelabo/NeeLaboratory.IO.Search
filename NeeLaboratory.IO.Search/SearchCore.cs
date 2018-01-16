﻿// Copyright (c) 2015-2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
using System.Windows.Data;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// ノード変更イベント種類
    /// </summary>
    public enum NodeChangedAction
    {
        None,
        Add,
        Remove,
        Rename,
    }

    /// <summary>
    /// ノード変更イベントデータ
    /// </summary>
    public class NodeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// イベント種類
        /// </summary>
        public NodeChangedAction Action { get; set; }

        /// <summary>
        /// 変更ノード
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// リネーム時の旧パス
        /// </summary>
        public string OldPath { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action"></param>
        /// <param name="node"></param>
        public NodeChangedEventArgs(NodeChangedAction action, Node node)
        {
            this.Action = action;
            this.Node = node;
        }
    }



    /// <summary>
    /// 検索コア
    /// 検索フォルダのファイルをインデックス化して保存し、検索を行う
    /// </summary>
    internal class SearchCore : IDisposable
    {
        /// <summary>
        /// ファイルシステム変更イベント
        /// </summary>
        internal event EventHandler<NodeTreeFileSystemEventArgs> FileSystemChanged;

        /// <summary>
        /// ノード変更イベント
        /// </summary>
        public event EventHandler<NodeChangedEventArgs> NodeChanged;


        /// <summary>
        /// ノード群
        /// </summary>
        private Dictionary<string, NodeTree> _fileIndexDirectory;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchCore()
        {
            _fileIndexDirectory = new Dictionary<string, NodeTree>();
        }


        /// <summary>
        /// 検索フォルダのインデックス化
        /// TODO: キャンセル処理
        /// </summary>
        /// <param name="areas">検索フォルダ群</param>
        public void Collect(string[] areas, CancellationToken token)
        {
            // フルパスに変換
            areas = areas.Select(e => Path.GetFullPath(e)).ToArray();

            var roots = new List<string>();

            // 他のパスに含まれるなら除外
            foreach (var path in areas)
            {
                if (!areas.Any(p => path != p && path.StartsWith(p.TrimEnd('\\') + "\\")))
                {
                    roots.Add(path);
                }
            }

            Collect(roots, token);
        }



        /// <summary>
        /// 検索フォルダのインデックス化
        /// 更新分のみ
        /// </summary>
        public void Collect(List<string> _roots, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var newDinctionary = new Dictionary<string, NodeTree>();

            foreach (var root in _roots)
            {
                NodeTree sub;

                if (!_fileIndexDirectory.ContainsKey(root))
                {
                    sub = new NodeTree(root);
                    sub.FileSystemChanged += (s, e) => FileSystemChanged?.Invoke(s, e);
                }
                else
                {
                    sub = _fileIndexDirectory[root];
                }

                newDinctionary.Add(root, sub);
            }


            Node.TotalCount = 0;

            ParallelOptions options = new ParallelOptions() { CancellationToken = token };
            Parallel.ForEach(newDinctionary.Values, options, sub =>
            {
                sub.Collect(options.CancellationToken);
            });

            // 再登録されなかったパスの後処理を行う
            foreach (var a in _fileIndexDirectory)
            {
                if (!newDinctionary.ContainsValue(a.Value))
                {
                    a.Value.Dispose();
                }
            }

            _fileIndexDirectory = newDinctionary;
            System.GC.Collect();
        }

        /// <summary>
        /// ノード数取得
        /// </summary>
        /// <returns></returns>
        public int NodeCount()
        {
            return _fileIndexDirectory.Sum(e => e.Value.NodeCount());
        }


        /// <summary>
        /// インデックス追加
        /// </summary>
        /// <param name="root">検索フォルダ</param>
        /// <param name="paths">追加パス</param>
        public Node AddPath(string root, string path)
        {
            if (!_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].AddNode(path);
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
        /// <param name="root"></param>
        /// <param name="oldFileName"></param>
        /// <param name="newFilename"></param>
        public Node RenamePath(string root, string oldFileName, string newFileName)
        {
            if (!_fileIndexDirectory.ContainsKey(root))
            {
                return null;
            }

            var node = _fileIndexDirectory[root].Rename(oldFileName, newFileName);
            NodeChanged?.Invoke(this, new NodeChangedEventArgs(NodeChangedAction.Rename, node) { OldPath = oldFileName });

            return node;
        }

        /// <summary>
        /// インデックスの情報更新
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        public void RefleshIndex(string root, string path)
        {
            if (!_fileIndexDirectory.ContainsKey(root))
            {
                return;
            }

            _fileIndexDirectory[root].RefleshNode(path);
        }


        #region 検索

        /// <summary>
        /// 単語区切り用の正規表現生成
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private string GetNotCodeBlockRegexString(char c)
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


        //
        private List<SearchKey> CreateOptionedKeys(string source)
        {
            string s = source;

            // スプリッター
            var regexStartSplitter = new Regex(@"^[\s]+");
            var regexQuatSentence = new Regex("^\"(.*?)\"");
            var regexSentence = new Regex(@"^[^\s]+");

            var keys = new List<SearchKey>();

            SearchKey key = null;

            while (s.Length > 0)
            {
                key = key ?? new SearchKey();

                // 先頭の空白を削除
                s = regexStartSplitter.Replace(s, "");

                // 除外オプション
                if (s[0] == '-')
                {
                    key.IsExclude = true;
                    s = s.Substring(1);
                    continue;
                }

                // 単語オプション
                if (s[0] == '@')
                {
                    key.IsWord = true;
                    s = s.Substring(1);
                    continue;
                }

                // 完全一致オプション
                if (s[0] == '"')
                {
                    var match = regexQuatSentence.Match(s);
                    if (match.Success)
                    {
                        key.IsPerfect = true;
                        key.Word = match.Groups[1].Value;
                        keys.Add(key);
                        key = null;
                        s = s.Substring(match.Length);
                        continue;
                    }
                }

                //
                {
                    var match = regexSentence.Match(s);
                    if (match.Success)
                    {
                        key.Word = match.Value;
                        keys.Add(key);
                        key = null;
                        s = s.Substring(match.Length);
                        continue;
                    }
                    else
                    {
                        Debugger.Break();
                        break;
                    }
                }
            }

            keys = keys.Where(e => !string.IsNullOrEmpty(e.Word)).Select(e => ValidateKey(e)).ToList();

            return keys;
        }


        /// <summary>
        /// 拡張キーワードとして検索キーリスト生成
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<SearchKey> CreateSimpleKeys(string source)
        {
            const string splitter = @"[\s]+";

            // 入力文字列を整列
            // 空白、改行文字でパーツ分け
            string s = new Regex("^" + splitter).Replace(source, "");
            s = new Regex(splitter + "$").Replace(s, "");
            var tokens = new Regex(splitter).Split(s);

            return tokens.Select(e => ValidateKey(new SearchKey() { Word = e })).ToList();
        }

        /// <summary>
        /// 拡張キーワードなしで検索キーリスト生成
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private SearchKey ValidateKey(SearchKey source)
        {
            var key = source.Clone();

            var word = key.IsPerfect ? key.Word : Node.ToNormalisedWord(key.Word, !key.IsWord);

            // 正規表現記号をエスケープ
            var t = Regex.Escape(word);

            if (!key.IsPerfect)
            {
                // (数値)部分を0*(数値)という正規表現に変換
                t = new Regex(@"0*(\d+)").Replace(t, match => "0*" + match.Groups[1]);
            }

            if (key.IsWord)
            {
                // 先頭文字
                var start = GetNotCodeBlockRegexString(word.First());
                if (start != null) t = $"(^|{start})" + t;

                // 終端文字
                var end = GetNotCodeBlockRegexString(word.Last());
                if (end != null) t = t + $"({end}|$)";
            }

            key.Word = t;
            return key;
        }


        /// <summary>
        /// 検索キーリスト生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private List<SearchKey> CreateKeys(string source, SearchOption option)
        {
            var keys = option.IsOptionEnabled ? CreateOptionedKeys(source) : CreateSimpleKeys(source);

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
                foreach (var part in _fileIndexDirectory)
                {
                    foreach (var node in part.Value.Root.AllChildren)
                        yield return node;
                }
            }
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public ObservableCollection<NodeContent> Search(string keyword, SearchOption option)
        {
            var items = new ObservableCollection<NodeContent>(Search(keyword, option, AllNodes).Select(e => e.Content));

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
        public IEnumerable<Node> Search(string keyword, SearchOption option, IEnumerable<Node> entries)
        {
            // pushpin保存
            var pushpins = entries.Where(f => f.Content.IsPushPin);

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
                var regex = new Regex(key.Word, RegexOptions.Compiled);
                if (key.IsPerfect)
                {
                    entries = entries.Where(f => Inverse(regex.Match(f.Name).Success, key.IsExclude));
                }
                else if (key.IsWord)
                {
                    entries = entries.Where(f => Inverse(regex.Match(f.NormalizedUnitWord).Success, key.IsExclude));
                }
                else
                {
                    entries = entries.Where(f => Inverse(regex.Match(f.NormalizedFazyWord).Success, key.IsExclude));
                }
            }

            // ディレクトリ除外
            if (!option.AllowFolder)
            {
                entries = entries.Where(f => !f.IsDirectory);
            }

            // pushpin除外
            entries = entries.Where(f => !f.Content.IsPushPin);

            // pushpinを先頭に連結して返す
            return pushpins.Concat(entries);
        }

        /// <summary>
        /// boolean反転
        /// </summary>
        /// <param name="value">入力値</param>
        /// <param name="inverse">反転フラグ</param>
        /// <returns></returns>
        private bool Inverse(bool value, bool inverse)
        {
            return inverse ? !value : value;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
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

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~SearchCore() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
}
