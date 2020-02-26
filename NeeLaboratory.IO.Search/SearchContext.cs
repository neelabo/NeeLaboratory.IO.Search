using System;
using System.IO;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// ノード環境
    /// </summary>
    public class SearchContext
    {
        #region Fields

        /// <summary>
        /// 通知用のノード総数
        /// 非同期で加算されるため、正確な値にならない
        /// </summary>
        private int _totalCount;

        /// <summary>
        /// ノード用フィルター関数
        /// </summary>
        private Func<FileSystemInfo, bool> _nodeFilter = info => true;

        #endregion

        #region Properties

        /// <summary>
        /// 通知用のノード総数.
        /// 非同期で加算されるため、正確な値にならない
        /// </summary>
        public int TotalCount
        {
            get { return _totalCount; }
            set { _totalCount = value; }
        }

        /// <summary>
        /// NodeFilter property.
        /// </summary>
        public Func<FileSystemInfo, bool> NodeFilter
        {
            get { return _nodeFilter; }
            set { if (_nodeFilter != value) { _nodeFilter = value; } }
        }

        #endregion

        #region Constructors

        public SearchContext()
        {
        }

        #endregion

        #region Methods

        #endregion
    }
}
