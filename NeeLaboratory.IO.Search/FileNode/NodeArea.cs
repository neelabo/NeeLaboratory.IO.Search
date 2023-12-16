using System;
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
            Path = System.IO.Path.GetFullPath(path).Trim('\\');
        }

        public NodeArea(string path, bool includeSubdirectories) : this(path)
        {
            IncludeSubdirectories = includeSubdirectories;
        }

        public string Path { get; set; } = "";

        public bool IncludeSubdirectories { get; set; }


        public bool Contains(NodeArea other)
        {
            if (this == other) return false;

            if (this.Path == other.Path) return true;

            if (this.Path.Length > other.Path.Length) return false;

            if (IncludeSubdirectories)
            {
                if (other.Path.StartsWith(this.Path))
                {
                    return other.Path[Path.Length] == '\\';
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (other.IncludeSubdirectories)
                {
                    return false;
                }
                else
                {
                    return Path == other.Path;
                }
            }
        }
    }

}
