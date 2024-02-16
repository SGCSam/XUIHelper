using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXURReadable
    {
        bool TryReadAsync(IXUR xur, BinaryReader reader, ILogger? logger = null);
    }
}
