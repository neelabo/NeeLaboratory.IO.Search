using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public interface ISearchItem
    {
        // これらの除外フラグは別の形で汎用化できないか？
        bool IsDirectory { get; }
        bool IsPushPin { get; }

        SearchValue GetValue(SearchPropertyProfile profile);
    }

}
