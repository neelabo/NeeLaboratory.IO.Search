// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Runtime.Serialization;

namespace NeeLaboratory.IO.Search
{
    [Serializable]
    public class SearchKeywordException : Exception
    {
        public SearchKeywordException() : base() { }
        public SearchKeywordException(string message) : base(message) { }
        public SearchKeywordException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class SearchKeywordOptionException : SearchKeywordException
    {
        public SearchKeywordOptionException() : base() { }
        public SearchKeywordOptionException(string message) : base(message) { }
        public SearchKeywordOptionException(string message, Exception inner) : base(message, inner) { }

        public string Option { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Option), Option);
        }
    }

    [Serializable]
    public class SearchKeywordRegularExpressionException : SearchKeywordException
    {
        public SearchKeywordRegularExpressionException() : base() { }
        public SearchKeywordRegularExpressionException(string message) : base(message) { }
        public SearchKeywordRegularExpressionException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class SearchKeywordDateTimeException : SearchKeywordException
    {
        public SearchKeywordDateTimeException() : base() { }
        public SearchKeywordDateTimeException(string message) : base(message) { }
        public SearchKeywordDateTimeException(string message, Exception inner) : base(message, inner) { }
    }
}
