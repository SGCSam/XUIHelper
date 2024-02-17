using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface ICUSTSection : IXURSection
    {
        const int ExpectedMagic = 0x43555354;

        List<XUFigure> Figures { get; }
    }
}
