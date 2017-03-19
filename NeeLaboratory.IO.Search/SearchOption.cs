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
        /// 単語一致
        /// </summary>
        [DataMember]
        public bool IsWord { get; set; }

        /// <summary>
        /// 完全一致
        /// </summary>
        [DataMember]
        public bool IsPerfect { get; set; }

        /// <summary>
        /// 順番一致（未対応）
        /// </summary>
        [DataMember]
        public bool IsOrder { get; set; }

        /// <summary>
        /// フォルダーを含める
        /// </summary>
        [DataMember]
        public bool AllowFolder { get; set; }

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
