using System;

namespace NeeLaboratory.IO.Search
{
    public interface ISearchItem
    {
        // これらの除外フラグは別の形で汎用化できないか？
        bool IsDirectory { get; }
        bool IsPushPin { get; }

        string Name { get; }
        string NormalizedUnitWord { get; }
        string NormalizedFuzzyWord { get; }
         DateTime DateTime { get; }
    }
}
