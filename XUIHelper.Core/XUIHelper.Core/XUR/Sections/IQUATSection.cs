using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IQUATSection : IXURSection
    {
        const int ExpectedMagic = 0x51554154;

        List<XUQuaternion> Quaternions { get; }
    }
}
