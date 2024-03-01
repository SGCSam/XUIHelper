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
    public class XUR8SectionsTable : IXURSectionsTable
    {
        public List<XURSectionTableEntry> Entries { get; private set; } = new List<XURSectionTableEntry>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR8SectionsTable));

                xur.Logger?.Here().Verbose("Reading XUR8 sections table.");

                if (xur.Header is not XUR8Header xur8Header)
                {
                    xur.Logger?.Here().Error("The header of the XUR file was not a XUR8 header, returning false.");
                    return false;
                }

                for (int i = 0; i < xur8Header.SectionsCount; i++)
                {
                    XURSectionTableEntry thisEntry = new XURSectionTableEntry();
                    if (!await thisEntry.TryReadAsync(xur, reader))
                    {
                        xur.Logger?.Here().Error("XUR section table entry read for index {0} has failed, returning false.", i);
                        return false;
                    }

                    Entries.Add(thisEntry);
                }

                xur.Logger?.Here().Verbose("XUR8 sections table read successful!");
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR8 sections table, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
