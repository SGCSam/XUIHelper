using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface INAMESection : IXURSection
    {
        const int ExpectedMagic = 0x4E414D45;

        List<XUNamedFrame> NamedFrames { get; }
    }
}
