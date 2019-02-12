// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search
{
    [DataContract]
    public enum SearchMode
    {
        /// <summary>
        /// シンプルモード。拡張機能なし
        /// </summary>
        [EnumMember]
        Simple,

        /// <summary>
        /// 通常モード。拡張機能あり
        /// </summary>
        [EnumMember]
        Advanced,

        /// <summary>
        /// 正規表現モード
        /// </summary>
        [EnumMember]
        RegularExpression,
    }
}
