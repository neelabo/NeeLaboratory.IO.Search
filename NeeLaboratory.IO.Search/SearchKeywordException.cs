// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeLaboratory.IO.Search
{
    [Serializable]
    public class SearchKeywordException : Exception
    {
        public SearchKeywordException() : base() { }
        public SearchKeywordException(string message) : base(message) { }
        public SearchKeywordException(string message, Exception inner) : base(message, inner) { }
    }
}
