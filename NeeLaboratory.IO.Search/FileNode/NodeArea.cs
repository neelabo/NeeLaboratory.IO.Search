using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search.FileNode
{
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

        public string Path { get; set; } = "";

        public bool IncludeSubdirectories { get; set; }
    }

}
