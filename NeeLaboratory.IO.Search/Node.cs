// Copyright (c) 2015-2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using CSharp.Japanese.Kanaxs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// Node
    /// </summary>
    public class Node
    {
        // WIN32API: 自然順ソート
        ////[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        ////private static extern int StrCmpLogicalW(string psz1, string psz2);

        // Logger
        private static Utility.Logger Logger => Development.Logger;

        /// <summary>
        /// 通知用のノード総数.
        /// 非同期で加算されるため、正確な値にならない
        /// </summary>
        public static int TotalCount { get; set; }


        /// <summary>
        /// ノード名
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            private set
            {
                _name = value;
                NormalizedFazyWord = ToNormalisedWord(_name, true);
                NormalizedUnitWord = ToNormalisedWord(_name, false);
            }
        }

        /// <summary>
        /// 名前変更
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            Name = name;

            foreach (var node in AllNodes)
            {
                node.RefleshPath();
            }
        }

        /// <summary>
        /// ノード情報更新
        /// </summary>
        private void RefleshPath()
        {
            if (Content != null)
            {
                Content.Path = Path;
                Content.Reflesh();
            }
        }

        /// <summary>
        /// 親ノード
        /// </summary>
        public Node Parent { get; private set; }

        /// <summary>
        /// 子ノード
        /// </summary>
        public List<Node> Children { get; set; }

        /// <summary>
        /// フルパス
        /// </summary>
        public string Path => Parent == null ? Name : System.IO.Path.Combine(Parent.Path, Name);

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        public bool IsDirectory => Children != null;

        /// <summary>
        /// 検索用正規化ファイル名
        /// </summary>
        public string NormalizedFazyWord { get; private set; }

        /// <summary>
        /// 検索用正規化ファイル名。ひらかな、カタカナを区別する
        /// </summary>
        public string NormalizedUnitWord { get; private set; }

        /// <summary>
        /// コンテンツ
        /// </summary>
        public NodeContent Content;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public Node(string name, Node parent)
        {
            Name = name;
            Parent = parent;
            Content = new NodeContent(Path);

            TotalCount++;
        }

        /// <summary>
        /// ファイル情報更新
        /// </summary>
        public void Reflesh()
        {
            Content.Reflesh();

            if (IsDirectory)
            {
                foreach (var child in Children)
                {
                    child.Reflesh();
                }
            }
        }

        /// <summary>
        /// 正規化された文字列に変換する
        /// </summary>
        /// <param name="src"></param>
        /// <param name="isFazy"></param>
        /// <returns></returns>
        public static string ToNormalisedWord(string src, bool isFazy)
        {
            string s = src;

            s = KanaEx.ToPadding(s); // 濁点を１文字にまとめる
            s = s.Normalize(NormalizationForm.FormKC); // 正規化
            s = s.ToUpper(); // アルファベットを大文字にする

            if (isFazy)
            {
                s = ToKatakanaWithNormalize(s); // ひらがなをカタカナにする ＋ 特定文字の正規化
            }

            return s;
        }


        /// <summary>
        /// ひらがなをカタカナにする ＋ 特定文字の正規化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ToKatakanaWithNormalize(string str)
        {
            if (str == null || str.Length == 0)
            {
                return str;
            }

            char[] cs = str.ToCharArray();
            int f = cs.Length;

            for (int i = 0; i < f; i++)
            {
                char c = cs[i];
                // ぁ(0x3041) ～ ゖ(0x3096)
                // ゝ(0x309D) ゞ(0x309E)
                if (('ぁ' <= c && c <= 'ゖ') ||
                    ('ゝ' <= c && c <= 'ゞ'))
                {
                    cs[i] = (char)(c + 0x0060);
                }

                else
                {
                    // 一分文字の正規化
                    cs[i] = ToNormalisedChar(c);
                }
            }

            return new string(cs);
        }

        /// <summary>
        /// 特定文字の正規化
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static char ToNormalisedChar(char src)
        {
            switch (src)
            {
                case 'ー': return '-';
                case '　': return ' ';
                case '♠': return '♤';
                case '♥': return '♡';
                case '❤': return '♡';
                case '❥': return '♡';
                case '♢': return '◇';
                case '♦': return '◇';
                case '◆': return '◇';
                case '♣': return '♧';
                default: return src;
            }

        }



        /// <summary>
        /// 表示文字列
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// ノード収拾
        /// </summary>
        /// <param name="name">ノード名。親がnullの場合はフルパス</param>
        /// <param name="parent">親ノード</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Node Collect(string name, Node parent, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var fullpath = parent != null ? System.IO.Path.Combine(parent.Path, name) : name;
            var dirInfo = new DirectoryInfo(fullpath);
            if (dirInfo.Exists)
            {
                return Collect(dirInfo, parent, token);
            }
            else
            {
                return new Node(name, parent);
            }
        }

        /// <summary>
        /// ノード収拾 (ディレクトリ)
        /// </summary>
        /// <param name="dirInfo">ディレクトリ情報</param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Node Collect(DirectoryInfo dirInfo, Node parent, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (dirInfo == null || !dirInfo.Exists) return null;

            Node node = parent == null ? new Node(dirInfo.FullName, null) : new Node(dirInfo.Name, parent);

            try
            {
                var directories = dirInfo.GetDirectories().ToList();
                ////directories.Sort(StrCmpLogicalW);

                var files = dirInfo.GetFiles().ToList();
                ////files.Sort(StrCmpLogicalW);

                var directoryNodes = new Node[directories.Count];
                ParallelOptions options = new ParallelOptions() { CancellationToken = token };
                Parallel.ForEach(directories, options, (s, state, index) =>
                {
                    options.CancellationToken.ThrowIfCancellationRequested();
                    Debug.Assert(directoryNodes[(int)index] == null);
                    directoryNodes[(int)index] = Collect(s, node, options.CancellationToken);
                });

                var fileNodes = files.Select(s => new Node(s.Name, node));

                node.Children = directoryNodes.Concat(fileNodes).ToList();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Warning(e.Message);
                node.Children = new List<Node>();
            }

            return node;
        }

        /// <summary>
        /// Splitter
        /// </summary>
        private static readonly char[] s_splitter = new char[] { '\\' };


        /// <summary>
        /// ノードの存在確認
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isCreate">なければ追加する</param>
        /// <returns></returns>
        private Node Scanning(string path, bool isCreate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (path == Name) return this;

            if (!IsDirectory) return null;
            if (!path.StartsWith(Name + '\\')) return null;
            var childPath = path.Substring(Name.Length + 1);

            foreach (var child in Children)
            {
                var node = child.Scanning(childPath, isCreate, token);
                if (node != null) return node;
            }

            if (!isCreate) return null;

            // 作成
            var tokens = childPath.Split(s_splitter, 2);
            var childNode = Collect(tokens[0], this, token);
            this.Children.Add(childNode);
            childNode.Content.IsAdded = true;
            return childNode;
        }


        /// <summary>
        /// ノードの存在確認
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node Search(string path, CancellationToken token)
        {
            return Scanning(path, false, token);
        }


        /// <summary>
        /// ノードの追加
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node Add(string path, CancellationToken token)
        {
            var node = Scanning(path, true, token);
            if (node != null && node.Content.IsAdded)
            {
                node.Content.IsAdded = false; // 追加フラグをOFFにしておく
                return node;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ノードの削除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node Remove(string path)
        {
            var node = Scanning(path, false, CancellationToken.None);
            if (node == null) return null;

            node.Parent?.Children.Remove(node);
            node.Parent = null;

            foreach (var n in node.AllNodes)
            {
                n.Content.IsRemoved = true;
            }

            return node;
        }


        /// <summary>
        /// すべてのNodeを走査。自身は含まない
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> AllChildren
        {
            get
            {
                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        yield return child;
                        foreach (var node in child.AllChildren)
                        {
                            yield return node;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// すべてのNodeを走査。自身を含む
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> AllNodes
        {
            get
            {
                yield return this;
                foreach (var child in AllChildren)
                {
                    yield return child;
                }
            }
        }



        /// <summary>
        /// 開発用：ツリー出力
        /// </summary>
        /// <param name="level"></param>
        [Conditional("DEBUG")]
        public void Dump(int level = 0)
        {
            var text = new string(' ', level * 4) + Name + (IsDirectory ? "\\" : "");
            Logger.Trace(text);
            if (IsDirectory)
            {
                foreach (var child in Children)
                {
                    child.Dump(level + 1);
                }
            }

            ////Logger.Trace($"{Path}:({AllNodes.Count()})");
        }
    }
}
