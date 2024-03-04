using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class KEYP8Section : IKEYPSection
    {
        public int Magic { get { return IKEYPSection.ExpectedMagic; } }

        public List<ulong> PropertyIndexes { get; private set; } = new List<ulong>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(KEYP8Section));
                xur.Logger?.Here().Verbose("Reading KEYP8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IKEYPSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading keyframe property indexes from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int index = 0;
                for (int bytesRead = 0; bytesRead < entry.Length;)
                {
                    byte packedBytesRead = 0;
                    ulong readIndex = reader.ReadPackedUInt(out packedBytesRead);
                    PropertyIndexes.Add(readIndex);
                    xur.Logger?.Here().Verbose("Read keyframe property index {0} as {1}.", index, readIndex);
                    index++;
                    bytesRead += packedBytesRead;
                }

                xur.Logger?.Here().Verbose("Read keyframe property indexes successfully, read a total of {0} indexes", PropertyIndexes.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading KEYP8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            throw new NotImplementedException();
        }
    }
}
