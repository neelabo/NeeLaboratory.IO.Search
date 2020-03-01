using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search
{
    [DataContract]
    public class SearchArea
    {
        public SearchArea()
        {
        }

        public SearchArea(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
        }

        public SearchArea(string path, bool includeSubdirectories) : this(path)
        {
            IncludeSubdirectories = includeSubdirectories;
        }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public bool IncludeSubdirectories { get; set; }
    }

}
