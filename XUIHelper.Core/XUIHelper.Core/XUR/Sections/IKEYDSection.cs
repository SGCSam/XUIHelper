using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IKEYDSection : IXURSection
    {
        const int ExpectedMagic = 0x4B455944;

        List<XURKeyframe> Keyframes { get; }
    }
}
