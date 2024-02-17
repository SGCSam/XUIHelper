using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IDATASection : IXURSection
    {
        const int ExpectedMagic = 0x44415441;

        XUObject? RootObject { get; }
    }
}
