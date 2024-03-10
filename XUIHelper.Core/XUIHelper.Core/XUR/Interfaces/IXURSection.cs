using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXURSection : IXURReadable
    {
        int Magic { get; }

        Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject);

        Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer);
    }
}
