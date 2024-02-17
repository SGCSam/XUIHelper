using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class DATA5Section : IDATASection
    {
        public int Magic { get { return IDATASection.ExpectedMagic; } }

        public XUObject? RootObject { get; private set; }

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(DATA5Section));
                xur.Logger?.Here().Verbose("Reading DATA5 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IDATASection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                ISTRNSection? strnSection = xur.TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading data from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead = 0;
                while(bytesRead < entry.Length) 
                {
                    short stringIndex = (short)(reader.ReadInt16BE() - 1);
                    xur.Logger?.Here().Verbose("Read string index of {0:X8}.", stringIndex);

                    byte flags = reader.ReadByte();
                    xur.Logger?.Here().Verbose("Read flags of {0:X8}.", flags);
                    bytesRead += 3;

                    if(stringIndex < 0 || stringIndex > strnSection.Strings.Count - 1)
                    {
                        xur.Logger?.Here().Verbose("String index of {0:X8} is invalid, must be between 0 and {1}, returning false.", stringIndex, strnSection.Strings.Count - 1);
                        return false;
                    }

                    string className = strnSection.Strings[stringIndex];
                    xur.Logger?.Here().Verbose("Reading class {0}.",className);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading DATA5 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
