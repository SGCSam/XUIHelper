using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public abstract class XUR : IXUR
    {
        public ILogger? Logger { get; set; }

        public string FilePath { get; private set; }
        public IXURHeader Header { get; protected set; }
        public IXURSectionsTable SectionsTable { get; protected set; }
        public List<IXURSection> Sections { get; protected set; } = new List<IXURSection>();

        protected BinaryReader Reader { get; private set; }

        public XUR(string filePath, IXURHeader header, IXURSectionsTable sectionsTable, ILogger? logger = null)
        {
            FilePath = filePath;
            Header = header;
            SectionsTable = sectionsTable;
            Logger = logger?.ForContext(typeof(XUR));
        }

        public virtual bool TryReadAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Logger?.Here().Error("The XUR file at {0} doesn't exist, returning false.", FilePath);
                    return false;
                }

                Logger?.Here().Information("Reading XUR file at {0}", FilePath);
                Reader = new BinaryReader(File.OpenRead(FilePath));

                if (!Header.TryReadAsync(this, Reader))
                {
                    Logger?.Here().Error("XUR file header read has failed, returning false.", FilePath);
                    return false;
                }

                if (!SectionsTable.TryReadAsync(this, Reader))
                {
                    Logger?.Here().Error("XUR file sections table read has failed, returning false.", FilePath);
                    return false;
                }

                if(SectionsTable.Entries.Count <= 0)
                {
                    Logger?.Here().Error("The sections table has no entries, returning false.", FilePath);
                    return false;
                }

                Logger?.Here().Verbose("Reading all entries from sections table.");
                foreach (XURSectionTableEntry entry in SectionsTable.Entries)
                {
                    IXURSection? section = TryGetXURSectionForMagic(entry.Magic);
                    if (section == null)
                    {
                        Logger?.Here().Error("Failed to get XUR section for magic {0:X8}, returning false.", entry.Magic);
                        return false;
                    }

                    Logger?.Here().Verbose("Reading section {0:X8} from offset {1:X8}.", entry.Magic, entry.Offset);
                    Reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                    if(!section.TryReadAsync(this, Reader))
                    {
                        Logger?.Here().Error("Failed to read section {0:X8}, returning false.", entry.Magic);
                        return false;
                    }

                    int expectedOffset = entry.Offset + entry.Length;
                    if(Reader.BaseStream.Position != expectedOffset)
                    {
                        Logger?.Here().Error("The read of section {0:X8} succeeded, but the expected offset did not match the actual offset. returning false. " +
                            "There must be a logic bug with some data unread. Expected: {1:X8}, Actual: {2:X8}", entry.Magic, expectedOffset, Reader.BaseStream.Position);
                        return false;
                    }

                    Logger?.Here().Verbose("Read {0:X8} section successfully!", entry.Magic);
                }

                Logger?.Here().Information("Read successful!");
                return true;
            }
            catch(Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to read XUR file at {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }

        

        protected abstract IXURSection? TryGetXURSectionForMagic(int Magic);
    }
}
