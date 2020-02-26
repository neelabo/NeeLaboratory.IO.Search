using System;
using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// 旧検索オプション
    /// </summary>
    [Obsolete, DataContract]
    public class SearchOptionLegacyV1
    {
        [DataMember]
        public bool IsOptionEnabled { get; set; } = true;

        [DataMember]
        public bool AllowFolder { get; set; }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.IsOptionEnabled = true;
        }
    }
}
