using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXURSectionsTable : IXURReadable
    {
        List<XURSectionTableEntry> Entries { get; }

        Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer, List<XURSectionTableEntry> entries);
    }
}
