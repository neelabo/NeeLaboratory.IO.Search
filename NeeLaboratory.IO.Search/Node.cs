using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// Node
    /// </summary>
    public class Node : ISearchItem
    {
        // Logger
        private static Utility.Logger Logger => Development.Logger;

        /// <summary>
        /// Splitter
        /// </summary>
        private static readonly char[] s_splitter = new char[] { '\\' };

        /// <summary>
        /// 親ノード
        /// </summary>
        private Node? _parent;

        /// <summary>
        /// 子ノード
        /// </summary>
        private List<Node>? _children;

        /// <summary>
        /// リンク式ノードパス
        /// </summary>
        private readonly NodePath _nodePath;

        /// <summary>
        /// コンテンツ
        /// </summary>
        private readonly NodeContent _content;

        /// <summary>
        /// サブフォルダー再帰許可数。負で無限
        /// </summary>
        private readonly int _depth;



        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public Node(FileSystemInfo fileSystemInfo, Node? parent, int depth, SearchContext ctx)
        {
            _parent = parent;
            _depth = depth;
            _nodePath = _parent != null ? new NodePath(fileSystemInfo.Name, _parent._nodePath) : new NodePath(fileSystemInfo.FullName, null);
            _content = new NodeContent(_nodePath, fileSystemInfo);

            ctx.TotalCount++;
        }



        /// <summary>
        /// ノード名
        /// </summary>
        public string Name => _nodePath.Name;

        /// <summary>
        /// 親ノード
        /// </summary>
        public Node? Parent
        {
            get => _parent;
            set => _parent = value;
        }

        /// <summary>
        /// 子ノード
        /// </summary>
        public List<Node>? Children => _children;

        /// <summary>
        /// フルパス
        /// </summary>
        public string Path => _nodePath.Path;

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        public bool IsDirectory => _children != null;

        /// <summary>
        /// 検索用 値の取得
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public SearchValue GetValue(SearchPropertyProfile property)
        {
            switch (property.Name)
            {
                case "text":
                    return new StringSearchValue(Name);
                case "date":
                    return new DateTimeSearchValue(_content.FileInfo.LastWriteTime);
                default:
                    Debug.Assert(false, $"not support property: {property}");
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// コンテンツ
        /// </summary>
        public NodeContent Content => _content;

        /// <summary>
        /// ファイル最終更新日
        /// </summary>
        public DateTime DateTime => _content.FileInfo.LastWriteTime;

        /// <summary>
        /// PushPinフラグ
        /// </summary>
        public bool IsPushPin => _content != null && _content.IsPushPin;

        /// <summary>
        /// すべてのNodeを走査。自身は含まない
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> AllChildren
        {
            get
            {
                if (_children != null)
                {
                    foreach (var child in _children)
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
        /// 名前変更
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            _nodePath.Name = name;
            Refresh();
        }

        /// <summary>
        /// ファイル情報更新
        /// </summary>
        public void Refresh()
        {
            foreach (var node in AllNodes)
            {
                node._content?.Reflesh();
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
        /// <param name="name">ノード名。親が null の場合はフルパス</param>
        /// <param name="parent">親ノード</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Node? Collect(string name, Node? parent, int depth, SearchContext ctx, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var fullPath = parent != null ? System.IO.Path.Combine(parent.Path, name) : name;
            var dirInfo = new DirectoryInfo(fullPath);
            if (dirInfo.Exists)
            {
                return Collect(dirInfo, parent, depth, ctx, token);
            }
            else
            {
                var fileInfo = new System.IO.FileInfo(fullPath);
                if (fileInfo.Exists)
                {
                    return new Node(fileInfo, parent, depth, ctx);
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// ノード収拾 (ディレクトリ)
        /// </summary>
        /// <param name="dirInfo">ディレクトリ情報</param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Node Collect(DirectoryInfo dirInfo, Node? parent, int depth, SearchContext ctx, CancellationToken token)
        {
            if (dirInfo is null || !dirInfo.Exists) throw new ArgumentException("dirInfo must be exists");

            token.ThrowIfCancellationRequested();

            var node = new Node(dirInfo, parent, depth, ctx);

            if (depth == 0)
            {
                node._children = new List<Node>();
                return node;
            }

            try
            {
                var infos = dirInfo.GetFileSystemInfos();
                var directories = infos.OfType<DirectoryInfo>().Where(e => ctx.NodeFilter?.Invoke(e) == true).ToList();
                var files = infos.OfType<System.IO.FileInfo>().Where(e => ctx.NodeFilter?.Invoke(e) == true).ToList();

                var directoryNodes = new Node[directories.Count];
                var options = new ParallelOptions() { CancellationToken = token };
                Parallel.ForEach(directories, options, (s, state, index) =>
                {
                    options.CancellationToken.ThrowIfCancellationRequested();
                    Debug.Assert(directoryNodes[(int)index] == null);
                    directoryNodes[(int)index] = Collect(s, node, depth - 1, ctx, options.CancellationToken);
                });

                var fileNodes = files.Select(s => new Node(s, node, depth - 1, ctx));

                node._children = directoryNodes.Concat(fileNodes).ToList();
                return node;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Warning(e.Message);
                node._children = new List<Node>();
                return node;
            }
        }

        /// <summary>
        /// ノードの存在確認
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isCreate">なければ追加する</param>
        /// <returns></returns>
        private Node? Scanning(string path, bool isCreate, SearchContext ctx, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (path == Name) return this;

            if (_children is null) return null;

            var name = Name.TrimEnd('\\');
            if (!path.StartsWith(name + '\\')) return null;
            var childPath = path[(name.Length + 1)..];

            foreach (var child in _children)
            {
                var node = child.Scanning(childPath, isCreate, ctx, token);
                if (node != null) return node;
            }

            if (!isCreate) return null;
            if (_depth == 0) return null;

            // 作成
            var tokens = childPath.Split(s_splitter, 2);
            var childNode = Collect(tokens[0], this, _depth - 1, ctx, token);
            if (childNode == null) return null;

            _children.Add(childNode);
            childNode.Content.IsAdded = true;
            return childNode;
        }

        /// <summary>
        /// ノードの存在確認
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node? Search(string path, SearchContext ctx, CancellationToken token)
        {
            return Scanning(path, false, ctx, token);
        }

        /// <summary>
        /// ノードの追加
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Node? Add(string path, SearchContext ctx, CancellationToken token)
        {
            var node = Scanning(path, true, ctx, token);
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
        public Node? Remove(string path, SearchContext ctx)
        {
            var node = Scanning(path, false, ctx, CancellationToken.None);
            if (node == null) return null;

            node._parent?._children?.Remove(node);
            node._parent = null;

            foreach (var n in node.AllNodes)
            {
                n.Content.IsRemoved = true;
            }

            return node;
        }

        /// <summary>
        /// 開発用：ツリー出力
        /// </summary>
        /// <param name="level"></param>
        [Conditional("DEBUG")]
        public void Dump(int level = 0)
        {
            var text = new string(' ', level * 4) + string.Format("{0}: ({1})", Name + (IsDirectory ? "\\" : ""), _depth);
            Logger.Trace(text);
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.Dump(level + 1);
                }
            }

            ////Logger.Trace($"{Path}:({AllNodes.Count()})");
        }

    }
}
