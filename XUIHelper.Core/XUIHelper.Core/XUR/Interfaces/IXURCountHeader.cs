using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXURCountHeader : IXURReadable
    {
        bool TryVerify(IXUR xur);

        Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer, XUObject rootObject);
    }
}
