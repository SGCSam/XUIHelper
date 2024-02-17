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
        Task<bool> TryReadAsync(IXUR xur, BinaryReader reader);
    }
}
