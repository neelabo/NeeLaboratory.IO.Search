// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


namespace NeeLaboratory.IO.Search
{
    /// <summary>
    ///  リンク形式のパス表現
    /// </summary>
    public class NodePath
    {
        #region Fields

        /// <summary>
        /// 名前
        /// </summary>
        private string _name;

        /// <summary>
        /// 親
        /// </summary>
        private NodePath _parent;

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public NodePath(string name, NodePath parent)
        {
            _name = name;
            _parent = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 名前
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// パス
        /// </summary>
        public string Path => _parent == null ? _name : System.IO.Path.Combine(_parent.Path, Name);

        #endregion
    }
}
