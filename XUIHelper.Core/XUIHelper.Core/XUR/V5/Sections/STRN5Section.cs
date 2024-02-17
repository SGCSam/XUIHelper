﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class STRN5Section : ISTRNSection
    {
        public List<string> Strings { get; private set; } = new List<string>();

        public bool TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(STRN5Section));
                xur.Logger?.Here().Verbose("Reading STRN5 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ISTRNSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading string from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                for(bytesRead = 0; bytesRead < entry.Length;)
                {
                    short stringLength = reader.ReadInt16BE();
                    xur.Logger?.Here().Verbose("Got a string length of {0:X8}", stringLength);

                    string readStr = reader.ReadUTF8String(stringLength, false);
                    Strings.Add(readStr);
                    xur.Logger?.Here().Verbose("Read a string {0}", readStr);
                    bytesRead += (2 + (stringLength * 2));
                }

                xur.Logger?.Here().Verbose("Read all strings successfully, read a total of {0:X8} bytes.", bytesRead);
                return true;
            }
            catch(Exception ex) 
            {
                xur.Logger?.Here().Error("Caught an exception when reading STRN5 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
