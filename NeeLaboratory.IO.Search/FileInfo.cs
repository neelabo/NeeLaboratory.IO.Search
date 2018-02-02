// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileInfo
    {
        #region Fields

        /// <summary>
        /// パス
        /// </summary>
        private NodePath _nodePath;

        /// <summary>
        /// 属性
        /// </summary>
        private FileAttributes _attributes;

        /// <summary>
        /// ファイルの種類
        /// </summary>
        private string _typeName;

        /// <summary>
        /// ファイルアイコン
        /// </summary>
        private BitmapSource _iconSource;

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        private long _size;

        /// <summary>
        /// 最終更新日
        /// </summary>
        private long _lastWriteTime;

        #endregion

        #region Constructors

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

        #endregion

        #region Properties

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
        /// ファイルの種類
        /// </summary>
        public string TypeName
        {
            get
            {
                if (_typeName == null) _typeName = FileSystem.CreateTypeName(_nodePath.Path, this.IsDirectory);
                return _typeName;
            }
        }

        /// <summary>
        /// アイコン
        /// </summary>
        public BitmapSource IconSource
        {
            get
            {
                if (_iconSource == null) _iconSource = FileSystem.CreateIcon(_nodePath.Path, this.IsDirectory);
                return _iconSource;
            }
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

        #endregion

        #region Methods

        /// <summary>
        /// 情報の初期化
        /// </summary>
        public void Reflesh()
        {
            _typeName = null;
            _iconSource = null;

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

        #endregion
    }
}