using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface ISTRNSection : IXURSection
    {
        const int ExpectedMagic = 0x5354524E;

        List<string> Strings { get; }
    }
}
