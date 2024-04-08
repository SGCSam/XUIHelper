using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR5SectionsTable : IXURSectionsTable
    {
        public List<XURSectionTableEntry> Entries { get; private set; } = new List<XURSectionTableEntry>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR5SectionsTable));
                xur.Logger?.Here().Verbose("Reading XUR5 sections table.");

                if(xur.Header is not XUR5Header xur5Header)
                {
                    xur.Logger?.Here().Error("The header of the XUR file was not a XUR5 header, returning false.");
                    return false;
                }

                for(int i = 0; i < xur5Header.SectionsCount; i++)
                {
                    XURSectionTableEntry thisEntry = new XURSectionTableEntry();
                    if (!await thisEntry.TryReadAsync(xur, reader))
                    {
                        xur.Logger?.Here().Error("XUR section table entry read for index {0} has failed, returning false.", i);
                        return false;
                    }

                    Entries.Add(thisEntry);
                }

                xur.Logger?.Here().Verbose("XUR5 sections table read successful!");
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR5 sections table, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer, List<XURSectionTableEntry> entries)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR5SectionsTable));
                xur.Logger?.Here().Verbose("Writing XUR5 sections table.");

                Entries = entries;
                int bytesWritten = 0;

                int entryIndex = 0;
                foreach (XURSectionTableEntry entry in Entries)
                {
                    int? entryBytesWritten = await entry.TryWriteAsync(xur, writer);
                    if (entryBytesWritten == null)
                    {
                        xur.Logger?.Here().Error("Failed to write entry index {0}, returning null.", entryIndex);
                        return null;
                    }

                    bytesWritten += entryBytesWritten.Value;
                    entryIndex++;
                }

                return bytesWritten;
            }
            catch(Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing XUR5 sections table, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
