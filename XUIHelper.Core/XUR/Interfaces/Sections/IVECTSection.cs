using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IVECTSection : IXURSection
    {
        const int ExpectedMagic = 0x56454354;

        List<XUVector> Vectors { get; }
    }
}
