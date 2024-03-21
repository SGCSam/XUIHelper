using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface ICOLRSection : IXURSection
    {
        const int ExpectedMagic = 0x434F4C52;

        List<XUColour> Colours { get; }
    }
}
