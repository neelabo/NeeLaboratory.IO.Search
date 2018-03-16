// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 検索オプション
    /// </summary>
    [DataContract]
    public class SearchOption
    {
        /// <summary>
        /// 検索オプション有効
        /// </summary>
        [DataMember]
        public bool IsOptionEnabled { get; set; } = true;

        /// <summary>
        /// フォルダーを含める
        /// </summary>
        [DataMember]
        public bool AllowFolder { get; set; }
        
        //
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.IsOptionEnabled = true;
        }

        /// <summary>
        /// 複製
        /// </summary>
        /// <returns></returns>
        public SearchOption Clone()
        {
            return (SearchOption)(this.MemberwiseClone());
        }
    }
}
