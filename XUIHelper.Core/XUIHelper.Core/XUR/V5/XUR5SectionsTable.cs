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
        public short SectionsCount { get; private set; }
        public List<XURSectionTableEntry> Entries { get; private set; } = new List<XURSectionTableEntry>();

        public bool TryReadAsync(IXUR xur, BinaryReader reader, ILogger? logger = null)
        {
            try
            {
                logger = logger?.ForContext(typeof(XUR5SectionsTable));

                logger?.Here().Verbose("Reading XUR5 sections table.");

                SectionsCount = reader.ReadInt16BE();
                logger?.Here().Verbose("Sections count is {0:X8}", SectionsCount);

                for(int i = 0; i < SectionsCount; i++)
                {
                    XURSectionTableEntry thisEntry = new XURSectionTableEntry();
                    if (!thisEntry.TryReadAsync(xur, reader, logger))
                    {
                        logger?.Here().Error("XUR section table entry read for index {0} has failed, returning false.", i);
                        return false;
                    }
                }

                logger?.Here().Verbose("XUR5 sections table read successful!");
                return true;
            }
            catch (Exception ex)
            {
                logger?.Here().Error("Caught an exception when reading XUR5 sections table, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
