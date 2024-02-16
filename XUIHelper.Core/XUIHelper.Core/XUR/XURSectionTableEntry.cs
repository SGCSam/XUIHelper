using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XURSectionTableEntry : IXURReadable
    {
        public int Magic { get; private set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }

        public bool TryReadAsync(IXUR xur, BinaryReader reader, ILogger? logger = null)
        {
            try
            {
                logger = logger?.ForContext(typeof(XURSectionTableEntry));

                logger?.Here().Verbose("Reading XUR section table entry.");

                Magic = reader.ReadInt32BE();
                logger?.Here().Verbose("Magic is {0:X8}", Magic);

                Offset = reader.ReadInt32BE();
                logger?.Here().Verbose("Offset is {0:X8}", Offset);

                Length = reader.ReadInt32BE();
                logger?.Here().Verbose("Length is {0:X8}", Length);

                return true;
            }
            catch (Exception ex)
            {
                logger?.Here().Error("Caught an exception when reading XUR5 section table entry, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
