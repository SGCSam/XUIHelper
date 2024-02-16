using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUR5Header : IXURHeader
    {
        public int Magic { get; private set; }
        public int Version { get; private set; }
        public int Flags { get; private set; }
        public short ToolVersion { get; private set; }
        public int FileSize { get; private set; }

        public bool TryRead(IXUR xur, BinaryReader reader, ILogger? logger = null)
        {
            throw new NotImplementedException();
        }
    }
}
