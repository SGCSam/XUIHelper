using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class DATA5Section : IDATASection
    {
        public int Magic { get { return IDATASection.ExpectedMagic; } }

        public XMLExtensionsManager? ExtensionsManager { get; private set; }

        public XUObject? RootObject { get; private set; }

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(DATA5Section));
                xur.Logger?.Here().Verbose("Reading DATA5 section.");

                if(ExtensionsManager == null)
                {
                    xur.Logger?.Here().Error("Extensions manager was null, returning false.");
                    return false;
                }

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

                    if((flags & 0x1) == 0x1)
                    {
                        xur.Logger?.Here().Verbose("Class has properties.", className);
                        List<XUProperty>? readProperties = TryReadProperties(xur, reader, className);
                        if (readProperties == null)
                        {
                            xur.Logger?.Here().Error("Failed to read properties, returning false.");
                            return false;
                        }
                    }

                    //So I think the single byte is the (number of properties TO READ IN THAT CLASS * 8) - how every many multiples of 8 are required in the hierarchy
                    //So canvas is always 0xC as (2 * 8) - (4) = 0xC
                    //XuiScene is normally 0x1C as (4 * 8) - (4) = 0x1C, as it is normally just Id, width, height and position
                    //XuiScene with just IgnorePresses would be 0x1 as (1 * 8) - (7) = 0x1, since we have to trawl through the hierarchy 7 multiples of 8 to get to XuiScene

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

        private List<XUProperty>? TryReadProperties(IXUR xur, BinaryReader reader, string className)
        {
            xur.Logger?.Here().Verbose("Reading properties for class {0}.", className);
            List<XUClass>? classList = ExtensionsManager?.TryGetClassHierarchy(className);
            if (classList == null)
            {
                xur.Logger?.Here().Error("Failed to get class hierarchy for class {0}, returning null.", className);
                return null;
            }

            short propertiesCount = reader.ReadInt16BE();
            xur.Logger?.Here().Verbose("Class {0} has {1:X8} properties.", className, propertiesCount);

            byte hierarchicalPropertiesCount = reader.ReadByte();
            xur.Logger?.Here().Verbose("Class {0} has a hierarchical properties count of {1:X8}.", className, hierarchicalPropertiesCount);

            return null;
        }

        public DATA5Section()
        {
            ExtensionsManager = XUIHelperCoreConstants.VersionedExtensions.GetValueOrDefault(0x5);
        }
    }
}
