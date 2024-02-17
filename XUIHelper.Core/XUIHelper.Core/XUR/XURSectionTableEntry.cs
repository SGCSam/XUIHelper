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

        public bool TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XURSectionTableEntry));

                xur.Logger?.Here().Verbose("Reading XUR section table entry.");

                Magic = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Magic is {0:X8}", Magic);

                Offset = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Offset is {0:X8}", Offset);

                Length = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Length is {0:X8}", Length);

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR5 section table entry, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
