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
    public class COLR8Section : ICOLRSection
    {
        public int Magic { get { return ICOLRSection.ExpectedMagic; } }

        public List<XUColour> Colours { get; private set; } = new List<XUColour>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(COLR8Section));
                xur.Logger?.Here().Verbose("Reading COLR8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ICOLRSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading colours from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int colIndex = 0;
                for (int bytesRead = 0; bytesRead < entry.Length;)
                {
                    byte a = reader.ReadByte();
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();

                    XUColour readColour = new XUColour(a, r, g, b);
                    Colours.Add(readColour);
                    xur.Logger?.Here().Verbose("Read colour index {0} as {1}.", colIndex, readColour);
                    colIndex++;
                    bytesRead += 0x4;
                }

                xur.Logger?.Here().Verbose("Read colours successfully, read a total of {0} colours", Colours.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading COLR8 section, returning false. The exception is: {0}", ex);
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
