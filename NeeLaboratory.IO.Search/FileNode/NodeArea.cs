using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search.FileNode
{
    [DataContract]
    public class NodeArea
    {
        public NodeArea()
        {
        }

        public NodeArea(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
        }

        public NodeArea(string path, bool includeSubdirectories) : this(path)
        {
            IncludeSubdirectories = includeSubdirectories;
        }

        [DataMember]
        public string Path { get; set; } = "";

        [DataMember]
        public bool IncludeSubdirectories { get; set; }
    }

}
