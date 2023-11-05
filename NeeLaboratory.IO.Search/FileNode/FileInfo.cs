using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search.FileNode
{
    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// パス
        /// </summary>
        private readonly NodePath _nodePath;

        /// <summary>
        /// 属性
        /// </summary>
        private FileAttributes _attributes;

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        private long _size;

        /// <summary>
        /// 最終更新日
        /// </summary>
        private long _lastWriteTime;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        public FileInfo(NodePath nodePath, FileSystemInfo fileSystemInfo)
        {
            _nodePath = nodePath;
            SetFileSystemInfo(fileSystemInfo);
        }


        /// <summary>
        /// パス
        /// </summary>
        public string Path
        {
            get { return _nodePath.Path; }
        }

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        public bool IsDirectory
        {
            get { return _attributes.HasFlag(FileAttributes.Directory); }
        }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Size
        {
            get { return _size; }
        }

        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime LastWriteTime
        {
            get { return DateTime.FromBinary(_lastWriteTime); }
        }

        /// <summary>
        /// 拡張子
        /// </summary>
        public string? Extension
        {
            get { return IsDirectory ? null : System.IO.Path.GetExtension(Path); }
        }


        /// <summary>
        /// 情報の初期化
        /// </summary>
        public void Reflesh()
        {
            ////_typeName = null;
            ////_iconSource = null;

            if (IsDirectory)
            {
                SetFileSystemInfo(new DirectoryInfo(_nodePath.Path));
            }
            else
            {
                SetFileSystemInfo(new System.IO.FileInfo(_nodePath.Path));
            }
        }

        /// <summary>
        /// FileSystemInfo情報適用
        /// </summary>
        /// <param name="fileSystemInfo"></param>
        private void SetFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null || !fileSystemInfo.Exists) return;

            _attributes = fileSystemInfo.Attributes;
            _size = fileSystemInfo is System.IO.FileInfo fileInfo ? fileInfo.Length : -1;
            _lastWriteTime = fileSystemInfo.LastWriteTime.ToBinary();
        }
    }
}
