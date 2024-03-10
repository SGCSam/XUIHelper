using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class FLOT8Section : IFLOTSection
    {
        public int Magic { get { return IFLOTSection.ExpectedMagic; } }

        public List<float> Floats { get; private set; } = new List<float>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(FLOT8Section));
                xur.Logger?.Here().Verbose("Reading FLOT8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IFLOTSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading floats from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int floatIndex = 0;
                for(int bytesRead = 0; bytesRead < entry.Length;)
                {
                    float thisFloat = reader.ReadSingleBE();
                    Floats.Add(thisFloat);
                    xur.Logger?.Here().Verbose("Read float index {0} as {1}.", floatIndex, thisFloat);
                    floatIndex++;
                    bytesRead += 0x4;
                }

                xur.Logger?.Here().Verbose("Read floats successfully, read a total of {0} floats", Floats.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading FLOT8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            throw new NotImplementedException();
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
