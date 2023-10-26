using System;

namespace NeeLaboratory.IO.Search
{
    public interface ISearchItem
    {
        // これらの除外フラグは別の形で汎用化できないか？
        bool IsDirectory { get; }
        bool IsPushPin { get; }

        string Name { get; }
        string NormalizedUnitName { get; }
        string NormalizedFuzzyName { get; }
         DateTime DateTime { get; }
    }
}
