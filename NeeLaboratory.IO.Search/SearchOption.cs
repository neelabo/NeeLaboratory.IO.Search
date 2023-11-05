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
        /// フォルダーを含める
        /// </summary>
        [DataMember]
        public bool AllowFolder { get; set; }


        /// <summary>
        /// 複製
        /// </summary>
        public SearchOption Clone()
        {
            return (SearchOption)(this.MemberwiseClone());
        }
    }
}
