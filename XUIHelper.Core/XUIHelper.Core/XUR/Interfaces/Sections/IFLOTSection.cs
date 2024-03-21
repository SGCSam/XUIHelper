using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IFLOTSection : IXURSection
    {
        const int ExpectedMagic = 0x464C4F54;

        List<float> Floats { get; }
    }
}
