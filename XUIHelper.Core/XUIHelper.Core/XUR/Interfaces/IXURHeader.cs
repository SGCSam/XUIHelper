using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXURHeader : IXURReadable
    {
        public const int ExpectedMagic = 0x58554942;
    }
}
