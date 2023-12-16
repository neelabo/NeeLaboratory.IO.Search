using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search.FileNode
{
    /// <summary>
    /// Nodeコンテンツ
    /// </summary>
    public class NodeContent : INotifyPropertyChanged, IComparable
    {
        internal static class NativeMethods
        {
            // 参考：自然順ソート
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion


        /// <summary>
        /// パス
        /// </summary>
        private readonly NodePath _nodePath;

        /// <summary>
        /// ファイル情報
        /// </summary>
        private readonly FileInfoX _fileInfo;

        /// <summary>
        /// ノード属性
        /// </summary>
        private NodeContentFlag _flags;



        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="path"></param>
        public NodeContent(NodePath nodePath, FileSystemInfo fileSystemInfo)
        {
            _nodePath = nodePath;
            IsDirectory = fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory);
            _fileInfo = new FileInfoX(_nodePath, fileSystemInfo);
        }



        /// <summary>
        /// Path property.
        /// </summary>
        public string Path => _nodePath.Path;

        /// <summary>
        /// Name property.
        /// </summary>
        public string Name
        {
            get { return _nodePath.Name; }
        }

        /// <summary>
        /// フォルダ名表示
        /// </summary>
        public string? DirectoryName
        {
            get
            {
                string? dir = System.IO.Path.GetDirectoryName(Path);
                string? parentDir = System.IO.Path.GetDirectoryName(dir);
                return parentDir == null ? dir : System.IO.Path.GetFileName(dir) + " (" + parentDir + ")";
            }
        }

        /// <summary>
        /// 詳細表示
        /// </summary>
        public string Detail
        {
            get
            {
                string sizeText = FileInfo.Size >= 0 ? $"Size: {(FileInfo.Size + 1024 - 1) / 1024:#,0} KB\n" : "Size: --\n";
                var dateText = FileInfo.LastWriteTime.ToString(SearchDateTimeTools.DateTimeFormat);
                return $"{Name}\n{sizeText}Date: {dateText}\nFolder: {DirectoryName}";
            }
        }

        /// <summary>
        /// ファイル情報
        /// </summary>
        public FileInfoX FileInfo
        {
            get { return _fileInfo; }
        }


        /// <summary>
        /// 属性：追加された
        /// </summary>
        public bool IsAdded
        {
            get { return _flags.IsFlag(NodeContentFlag.Added); }
            set { _flags = _flags.SetFlag(NodeContentFlag.Added, value); RaisePropertyChanged(); }
        }

        /// <summary>
        /// 属性：削除された
        /// </summary>
        public bool IsRemoved
        {
            get { return _flags.IsFlag(NodeContentFlag.Removed); }
            set { _flags = _flags.SetFlag(NodeContentFlag.Removed, value); RaisePropertyChanged(); }
        }

        /// <summary>
        /// 属性：ピン留め。検索結果に残す
        /// </summary>
        public bool IsPushPin
        {
            get { return _flags.IsFlag(NodeContentFlag.PushPin); }
            set { _flags = _flags.SetFlag(NodeContentFlag.PushPin, value); RaisePropertyChanged(); }
        }

        /// <summary>
        /// 属性：ディレクトリ
        /// </summary>
        public bool IsDirectory
        {
            get { return _flags.IsFlag(NodeContentFlag.Directory); }
            set { _flags = _flags.SetFlag(NodeContentFlag.Directory, value); RaisePropertyChanged(); }
        }



        /// <summary>
        /// ファイル情報更新
        /// </summary>
        public void Reflesh()
        {
            _fileInfo.Reflesh();
            RaisePropertyChanged(null);
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
        /// 名前で比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            NodeContent other = (NodeContent)obj;
            return NativeMethods.StrCmpLogicalW(Name, other.Name);
        }
    }
}
