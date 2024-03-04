using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class STRN8Section : ISTRNSection
    {
        public int Magic { get { return ISTRNSection.ExpectedMagic; } }

        public List<string> Strings { get; private set; } = new List<string>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(STRN8Section));
                xur.Logger?.Here().Verbose("Reading STRN8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ISTRNSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading strings from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int totalStringsLength = reader.ReadInt32BE();
                short stringsCount = reader.ReadInt16BE();

                int debugLength = 0;
                for(int stringIndex = 0; stringIndex < stringsCount; stringIndex++)
                {
                    string readStr = reader.ReadNullTerminatedString();
                    xur.Logger?.Here().Verbose("Read a string {0}", readStr);
                    Strings.Add(readStr);
                    debugLength += readStr.Length;
                }

                //We add + 2 to our checks too, since a empty string "" is still included in the totalStringsLength even though it may not be in the table...
                int expectedOffset = entry.Offset + totalStringsLength + 6;
                if(reader.BaseStream.Position != expectedOffset && reader.BaseStream.Position != expectedOffset + 2)
                {
                    xur.Logger?.Here().Error("Mismatch of offsets when reading STRN8 section, returning false. Expected: {0:X8}, Actual: {1:X8}", expectedOffset, reader.BaseStream.Position);
                    return true;
                }

                if(stringsCount != Strings.Count)
                {
                    xur.Logger?.Here().Error("Mismatch of strings count when reading STRN8 section, returning false. Expected: {0:X8}, Actual: {1:X8}", stringsCount, Strings.Count);
                    return false;
                }

                xur.Logger?.Here().Verbose("Read strings successfully, read a total of {0} strings", Strings.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading STRN8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
