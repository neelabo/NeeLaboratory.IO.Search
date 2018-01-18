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
        /// <summary>
        /// 単語一致
        /// @word
        /// </summary>
        public bool IsWord { get; set; }

        /// <summary>
        /// 完全一致
        /// "word"
        /// </summary>
        public bool IsPerfect { get; set; }

        /// <summary>
        /// 除外
        /// -word
        /// </summary>
        public bool IsExclude { get; set; }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Word { get; set; }

        //
        public SearchKey Clone()
        {
            return (SearchKey)this.MemberwiseClone();
        }

        //
        public override string ToString()
        {
            string s = "";
            if (IsExclude) s += "Not,";
            if (IsWord) s += "Word,";
            if (IsPerfect) s += "Perfect,";
            s += $"\"{Word}\"";
            return s;
        }
    }
}
