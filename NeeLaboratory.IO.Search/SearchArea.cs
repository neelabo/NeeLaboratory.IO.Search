// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search
{
    [DataContract]
    public class SearchArea
    {
        public SearchArea(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
        }

        public SearchArea(string path, bool includeSubdirectories) : this(path)
        {
            IncludeSubdirectories = includeSubdirectories;
        }

        [DataMember]
        public string Path { get; private set; }

        [DataMember]
        public bool IncludeSubdirectories { get; set; }
    }

}
