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
        public IXURCountHeader? CountHeader { get; protected set; }
        public IXURSectionsTable SectionsTable { get; protected set; }
        public List<IXURSection> Sections { get; protected set; } = new List<IXURSection>();

        protected BinaryReader Reader { get; private set; }

        public XMLExtensionsManager? ExtensionsManager { get; private set; }

        public XUR(string filePath, IXURHeader header, IXURSectionsTable sectionsTable, ILogger? logger = null)
        {
            FilePath = filePath;
            Header = header;
            SectionsTable = sectionsTable;
            Logger = logger?.ForContext(typeof(XUR));
        }

        public virtual async Task<bool> TryReadAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Logger?.Here().Error("The XUR file at {0} doesn't exist, returning false.", FilePath);
                    return false;
                }

                Logger?.Here().Information("Reading XUR file at {0}", FilePath);
                using (Reader = new BinaryReader(File.OpenRead(FilePath)))
                {
                    if (!await Header.TryReadAsync(this, Reader))
                    {
                        Logger?.Here().Error("XUR file header read has failed, returning false.", FilePath);
                        return false;
                    }

                    if (HasCountHeader())
                    {
                        Logger?.Here().Verbose("Reading count header...");
                        CountHeader = GetCountHeader();
                        if (!await CountHeader.TryReadAsync(this, Reader))
                        {
                            Logger?.Here().Error("Failed to read count header, returning false.");
                            return false;
                        }
                    }

                    if (!await SectionsTable.TryReadAsync(this, Reader))
                    {
                        Logger?.Here().Error("XUR file sections table read has failed, returning false.", FilePath);
                        return false;
                    }

                    if (SectionsTable.Entries.Count <= 0)
                    {
                        Logger?.Here().Error("The sections table has no entries, returning false.", FilePath);
                        return false;
                    }

                    Logger?.Here().Verbose("Reading all entries from sections table.");
                    foreach (XURSectionTableEntry entry in SectionsTable.Entries)
                    {
                        IXURSection? section = TryCreateXURSectionForMagic(entry.Magic);
                        if (section == null)
                        {
                            Logger?.Here().Error("Failed to create XUR section for magic {0:X8}, returning false.", entry.Magic);
                            return false;
                        }

                        Logger?.Here().Verbose("Reading section {0:X8} from offset {1:X8}.", entry.Magic, entry.Offset);
                        Reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                        if (!await section.TryReadAsync(this, Reader))
                        {
                            Logger?.Here().Error("Failed to read section {0:X8}, returning false.", entry.Magic);
                            return false;
                        }

                        int expectedOffset = entry.Offset + entry.Length;
                        if (Reader.BaseStream.Position != expectedOffset)
                        {
                            Logger?.Here().Error("The read of section {0:X8} succeeded, but the expected offset did not match the actual offset. returning false. " +
                                "There must be a logic bug with some data unread. Expected: {1:X8}, Actual: {2:X8}", entry.Magic, expectedOffset, Reader.BaseStream.Position);
                            return false;
                        }

                        Sections.Add(section);
                        Logger?.Here().Verbose("Read {0:X8} section successfully!", entry.Magic);
                    }

                    Logger?.Here().Verbose("All sections read successfully!");

                    if(CountHeader != null) 
                    {
                        Logger?.Here().Verbose("Verifying count header...");
                        if (!CountHeader.TryVerify(this))
                        {
                            Logger?.Here().Error("Failed to verify count header, returning false.");
                            return false;
                        }

                        Logger?.Here().Verbose("Verified count header successfully!");
                    }

                    Logger?.Here().Information("Read successful!");
                    return true;
                }
            }
            catch(Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to read XUR file at {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }

        public async Task<bool> TryWriteAsync(XUObject rootObject)
        {
            try
            {
                Logger?.Here().Information("Writing XUR file to {0}", FilePath);
                if (rootObject.ClassName != "XuiCanvas")
                {
                    Logger?.Here().Error("The object to write wasn't the root XuiCanvas, returning false.");
                    return false;
                }

                int extensionVersion = -1;
                if(this is XUR5)
                {
                    extensionVersion = 0x5;
                }
                else if(this is XUR8)
                {
                    extensionVersion = 0x8;
                }
                else
                {
                    Logger?.Here().Error("Unhandled XUR type for extension version, returning false.");
                    return false;
                }

                if (!XUIHelperCoreConstants.VersionedExtensions.ContainsKey(extensionVersion))
                {
                    Logger?.Here().Error("Failed to find extensions with version {0}, returning false.", extensionVersion);
                    return false;
                }

                ExtensionsManager = XUIHelperCoreConstants.VersionedExtensions[extensionVersion];
                

                List<IXURSection>? sections = await TryBuildSectionsFromObjectAsync(rootObject);
                if(sections == null)
                {
                    Logger?.Here().Error("Failed to build sections, returning false.");
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to write XUR file to {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }

        protected abstract IXURSection? TryCreateXURSectionForMagic(int Magic);

        protected abstract bool HasCountHeader();

        protected abstract IXURCountHeader GetCountHeader();

        protected abstract Task<List<IXURSection>?> TryBuildSectionsFromObjectAsync(XUObject xuObject);
    }
}
