// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// Nodeコンテンツ
    /// </summary>
    public class NodeContent : INotifyPropertyChanged, IComparable
    {
        // 参考：自然順ソート
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="path"></param>
        public NodeContent(string path)
        {
            Path = path;
            FileInfo = new FileInfo(Path);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Path property.
        /// </summary>
        private string _path;
        public string Path
        {
            get { return _path; }
            set { if (_path != value) { _path = value; RaisePropertyChanged(); UpdateName(); } }
        }

        /// <summary>
        /// Name property.
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// フォルダ名表示
        /// </summary>
        public string DirectoryName
        {
            get
            {
                string dir = System.IO.Path.GetDirectoryName(Path);
                string parentDir = System.IO.Path.GetDirectoryName(dir);
                return (parentDir == null) ? dir : System.IO.Path.GetFileName(dir) + " (" + parentDir + ")";
            }
        }

        /// <summary>
        /// 詳細表示
        /// </summary>
        public string Detail
        {
            get
            {
                string sizeText = (FileInfo.Size >= 0) ? $"サイズ: {(FileInfo.Size + 1024 - 1) / 1024:#,0} KB\n" : "サイズ: --\n";
                return $"{Name}\n種類: {FileInfo.TypeName}\n{sizeText}更新日時: {FileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm")}\nフォルダー: {DirectoryName}";
            }
        }

        /// <summary>
        /// ファイル情報
        /// </summary>
        public FileInfo FileInfo { get; private set; }

        /// <summary>
        /// ノード属性
        /// </summary>
        private NodeFlag Flags { get; set; }

        /// <summary>
        /// 属性：追加された
        /// </summary>
        public bool IsAdded
        {
            get { return IsFlag(NodeFlag.Added); }
            set { SetFlag(NodeFlag.Added, value); }
        }

        /// <summary>
        /// 属性：削除された
        /// </summary>
        public bool IsRemoved
        {
            get { return IsFlag(NodeFlag.Removed); }
            set { SetFlag(NodeFlag.Removed, value); }
        }

        /// <summary>
        /// 属性：ピン留め。検索結果に残す
        /// </summary>
        public bool IsPushPin
        {
            get { return IsFlag(NodeFlag.PushPin); }
            set { SetFlag(NodeFlag.PushPin, value); RaisePropertyChanged(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 名前更新
        /// </summary>
        private void UpdateName()
        {
            Name = System.IO.Path.GetFileName(Path);
        }

        /// <summary>
        /// 属性判定
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool IsFlag(NodeFlag flag)
        {
            return (Flags & flag) == flag;
        }

        /// <summary>
        /// 属性設定
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="state"></param>
        private void SetFlag(NodeFlag flag, bool state)
        {
            if (state)
                Flags = Flags | flag;
            else
                Flags = Flags & ~flag;
        }

        /// <summary>
        /// ファイル情報更新
        /// </summary>
        public void Reflesh()
        {
            FileInfo = new FileInfo(Path);
            RaisePropertyChanged(nameof(Path));
            UpdateName();
            RaisePropertyChanged(nameof(FileInfo));
            RaisePropertyChanged(nameof(DirectoryName));
            RaisePropertyChanged(nameof(Detail));
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
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            NodeContent other = (NodeContent)obj;
            return StrCmpLogicalW(this.Name, other.Name);
        }

        #endregion
    }
}
