// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Drawing;
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
        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        public FileInfo(string path, bool isDirectory)
        {
            _path = path;
            _isDirectory = isDirectory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// パス
        /// </summary>
        private string _path;
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        private bool _isDirectory;
        public bool IsDirectory
        {
            get { return _isDirectory; }
        }

        /// <summary>
        /// ファイルの種類
        /// </summary>
        private string _typeName;
        public string TypeName
        {
            get
            {
                if (_typeName == null) _typeName = FileSystem.CreateTypeName(_path, _isDirectory);
                return _typeName;
            }
        }

        /// <summary>
        /// アイコン
        /// </summary>
        private BitmapSource _iconSource;
        public BitmapSource IconSource
        {
            get
            {
                if (_iconSource == null) _iconSource = FileSystem.CreateIcon(_path, _isDirectory);
                return _iconSource;
            }
        }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        private long? _size;
        public long Size
        {
            get
            {
                if (_size == null) _size = FileSystem.GetSize(_path);
                return (long)_size;
            }
        }

        /// <summary>
        /// 最終更新日
        /// </summary>
        private DateTime? _lastWriteTime;
        public DateTime LastWriteTime
        {
            get
            {
                if (_lastWriteTime == null) _lastWriteTime = FileSystem.GetLastWriteTime(_path);
                return (DateTime)_lastWriteTime;
            }
        }

        #endregion
    }
}