using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public interface ISearchItem
    {
        SearchValue GetValue(SearchPropertyProfile profile);
    }

}
