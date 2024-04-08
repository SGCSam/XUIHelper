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

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
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
                xur.Logger?.Here().Error("Caught an exception when reading XUR section table entry, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XURSectionTableEntry));
                xur.Logger?.Here().Verbose("Writing section table entry.");

                writer.WriteInt32BE(Magic);
                xur.Logger?.Here().Verbose("Wrote magic {0:X8}", Magic);

                writer.WriteInt32BE(Offset);
                xur.Logger?.Here().Verbose("Wrote offset {0:X8}", Offset);

                writer.WriteInt32BE(Length);
                xur.Logger?.Here().Verbose("Wrote length {0:X8}", Length);

                return 12;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing XUR section table entry, returning false. The exception is: {0}", ex);
                return null;
            }
        }

        public XURSectionTableEntry()
        {

        }

        public XURSectionTableEntry(int magic, int offset, int length)
        {
            Magic = magic;
            Offset = offset;
            Length = length;
        }

        public override string ToString()
        {
            return string.Format("(Magic: {0:X8}, Offset: {1:X8}, Length: {2:X8})", Magic, Offset, Length);
        }
    }
}
