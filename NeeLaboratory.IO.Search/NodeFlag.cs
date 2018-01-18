// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// Node属性
    /// </summary>
    [Flags]
    public enum NodeFlag
    {
        Added = (1 << 0),
        Removed = (1 << 1),
        PushPin = (1 << 2),
    }
}
