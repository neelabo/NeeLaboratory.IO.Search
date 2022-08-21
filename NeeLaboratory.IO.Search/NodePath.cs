namespace NeeLaboratory.IO.Search
{
    /// <summary>
    ///  リンク形式のパス表現
    /// </summary>
    public class NodePath
    {
        /// <summary>
        /// 名前
        /// </summary>
        private string _name;

        /// <summary>
        /// 親
        /// </summary>
        private NodePath? _parent;



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public NodePath(string name, NodePath? parent)
        {
            _name = name;
            _parent = parent;
        }



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

    }
}
