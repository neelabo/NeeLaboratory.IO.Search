// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索キー
    /// </summary>
    public class SearchKey
    {
        #region Constructors

        public SearchKey()
        {
        }

        public SearchKey(string word)
        {
            Word = word;
        }

        public SearchKey(string word, bool isPerfect, bool isExclude, bool isWord)
        {
            Word = word;
            IsPerfect = isPerfect;
            IsExclude = isExclude;
            IsWord = isWord;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// 完全一致
        /// "word"
        /// </summary>
        public bool IsPerfect { get; set; }

        /// <summary>
        /// 単語一致
        /// @word
        /// </summary>
        public bool IsWord { get; set; }

        /// <summary>
        /// 除外
        /// -word
        /// </summary>
        public bool IsExclude { get; set; }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Word { get; set; }

        #endregion Properties

        #region Methods

        public SearchKey Clone()
        {
            return (SearchKey)this.MemberwiseClone();
        }

        public override bool Equals(object other)
        {
            if (other is SearchKey target)
            {
                return this.Word == target.Word
                    && this.IsPerfect == target.IsPerfect
                    && this.IsExclude == target.IsExclude
                    && this.IsWord == target.IsWord;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string s = "";
            if (IsExclude) s += "Not,";
            if (IsWord) s += "Word,";
            if (IsPerfect) s += "Perfect,";
            s += $"\"{Word}\"";
            return s;
        }

        #endregion Methods
    }
}
