using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IKEYPSection : IXURSection
    {
        const int ExpectedMagic = 0x4B455950;

        List<uint> PropertyIndexes { get; }
        List<List<uint>> GroupedPropertyIndexes { get; }
    }
}
