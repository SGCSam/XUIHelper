using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;
using static System.Collections.Specialized.BitVector32;

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

                    if (HasReadableCountHeader())
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

                    if (CountHeader != null)
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
            catch (Exception ex)
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

                if (!await TryBuildSectionsFromObjectAsync(rootObject))
                {
                    Logger?.Here().Error("Failed to build sections, returning false.");
                    return false;
                }

                if(!XUIHelperCoreUtilities.IsStringValidPath(FilePath))
                {
                    Logger?.Here().Error("The provided file path {0} is invalid, returning false.", FilePath);
                    return false;
                }

                string? dirName = Path.GetDirectoryName(FilePath);
                if(string.IsNullOrEmpty(dirName))
                {
                    Logger?.Here().Error("The directory name for {0} was null or empty, returning false.", FilePath);
                    return false;
                }

                Directory.CreateDirectory(dirName);
                File.Delete(FilePath);

                int writtenFileSize = 0;
                Dictionary<int, int> sectionsLengths = new Dictionary<int, int>();
                string sectionsFilePath = Path.GetTempFileName();
                Logger?.Here().Verbose("Writing sections to {0}", sectionsFilePath);
                using (BinaryWriter sectionsWriter = new BinaryWriter(File.OpenWrite(sectionsFilePath)))
                {
                    foreach (IXURSection section in Sections)
                    {
                        if (sectionsLengths.ContainsKey(section.Magic))
                        {
                            Logger?.Here().Error("Sections lengths already contained a duplicate magic of {0}, returning false.", section.Magic);
                            return false;
                        }

                        int? bytesWritten = await section.TryWriteAsync(this, rootObject, sectionsWriter);
                        if (bytesWritten == null)
                        {
                            Logger?.Here().Error("Failed to write section {0:X8}, returning false.", section.Magic);
                            return false;
                        }

                        sectionsLengths[section.Magic] = bytesWritten.Value;
                        writtenFileSize += bytesWritten.Value;
                    }
                }

                int countHeaderBytesWritten = 0;
                string countHeaderFilePath = string.Empty;
                if (ShouldWriteCountHeader(rootObject))
                {
                    Logger?.Here().Verbose("The count header should be written.");

                    CountHeader = GetCountHeader();
                    if (CountHeader == null)
                    {
                        Logger?.Here().Error("Count header was null, returning false.");
                        return false;
                    }

                    countHeaderFilePath = Path.GetTempFileName();
                    Logger?.Here().Verbose("Writing count header to {0}", countHeaderFilePath);
                    using (BinaryWriter countHeaderWriter = new BinaryWriter(File.OpenWrite(countHeaderFilePath)))
                    {
                        int? bytesWritten = await CountHeader.TryWriteAsync(this, countHeaderWriter, rootObject);
                        if (bytesWritten == null)
                        {
                            Logger?.Here().Error("Failed to write count header, returning false.");
                            return false;
                        }

                        countHeaderBytesWritten = bytesWritten.Value;
                        writtenFileSize += countHeaderBytesWritten;
                    }
                }
                else
                {
                    Logger?.Here().Verbose("The count header should not be written.");
                }

                int expectedSectionTableBytesWritten = Sections.Count * 12;
                int headerBytesWritten = 20;
                writtenFileSize += expectedSectionTableBytesWritten;
                writtenFileSize += headerBytesWritten;

                List<XURSectionTableEntry> sectionTableEntries = new List<XURSectionTableEntry>();
                int thisSectionOffset = headerBytesWritten + countHeaderBytesWritten + expectedSectionTableBytesWritten;
                foreach (var sectionLength in sectionsLengths)
                {
                    XURSectionTableEntry sectionTableEntry = new XURSectionTableEntry(sectionLength.Key, thisSectionOffset, sectionLength.Value);
                    sectionTableEntries.Add(sectionTableEntry);
                    thisSectionOffset += sectionLength.Value;
                    Logger?.Here().Verbose("Added section table entry: {0}", sectionTableEntry);
                }

                if (sectionTableEntries.Count != Sections.Count)
                {
                    Logger?.Here().Error("There was a mismatch between the sections and the table entries, returning false. Expected: {0}, Actual: {1}", Sections.Count, sectionTableEntries.Count);
                    return false;
                }

                string sectionTableFilePath = Path.GetTempFileName();
                Logger?.Here().Verbose("Writing sections table to {0}", sectionTableFilePath);
                using (BinaryWriter sectionTableWriter = new BinaryWriter(File.OpenWrite(sectionTableFilePath)))
                {
                    int? actualSectionTableBytesWritten = await SectionsTable.TryWriteAsync(this, sectionTableWriter, sectionTableEntries);
                    if (actualSectionTableBytesWritten == null)
                    {
                        Logger?.Here().Error("Section table bytes written was null, returning false.");
                        return false;
                    }

                    if (expectedSectionTableBytesWritten != actualSectionTableBytesWritten)
                    {
                        Logger?.Here().Error("There was a mismatch between the section table bytes written, returning false. Expected: {0}, Actual: {1}", expectedSectionTableBytesWritten, actualSectionTableBytesWritten);
                        return false;
                    }
                }

                if(this is XUR5)
                {
                    Header = new XUR5Header(string.IsNullOrEmpty(countHeaderFilePath) ? 0x0 : 0x1, writtenFileSize, (short)Sections.Count);
                }
                else if(this is XUR8)
                {
                    Header = new XUR8Header(0x0, writtenFileSize, (short)Sections.Count);
                }
                else
                {
                    Logger?.Here().Error("Unhandled XUR for writing header, returning false.");
                    return false;
                }

                Logger?.Here().Verbose("Writing XUR to {0}", FilePath);
                using (BinaryWriter xurWriter = new BinaryWriter(File.OpenWrite(FilePath)))
                {
                    Logger?.Here().Verbose("Writing header.");
                    if (await Header.TryWriteAsync(this, xurWriter) == null)
                    {
                        Logger?.Here().Error("Failed to write header, returning false.");
                        return false;
                    }

                    if(!string.IsNullOrEmpty(countHeaderFilePath))
                    {
                        Logger?.Here().Verbose("Copying count header from {0}", countHeaderFilePath);
                        using (BinaryReader countHeaderReader = new BinaryReader(File.OpenRead(countHeaderFilePath)))
                        {
                            await countHeaderReader.BaseStream.CopyToAsync(xurWriter.BaseStream);
                        }
                        File.Delete(countHeaderFilePath);
                    }

                    Logger?.Here().Verbose("Copying sections table from {0}", sectionTableFilePath);
                    using (BinaryReader sectionTableReader = new BinaryReader(File.OpenRead(sectionTableFilePath)))
                    {
                        await sectionTableReader.BaseStream.CopyToAsync(xurWriter.BaseStream);
                    }
                    File.Delete(sectionTableFilePath);

                    Logger?.Here().Verbose("Copying sections from {0}", sectionsFilePath);
                    using (BinaryReader sectionsReader = new BinaryReader(File.OpenRead(sectionsFilePath)))
                    {
                        await sectionsReader.BaseStream.CopyToAsync(xurWriter.BaseStream);
                    }
                    File.Delete(sectionsFilePath);

                    if(xurWriter.BaseStream.Length != writtenFileSize)
                    {
                        Logger?.Here().Error("Mismatch of written length, returning false. Expected: {0}, Actual: {1}", writtenFileSize, xurWriter.BaseStream.Length);
                        return false;
                    }
                }

                Logger?.Here().Information("Wrote XUR file to {0} successfully!", FilePath);
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to write XUR file to {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }

        protected abstract IXURSection? TryCreateXURSectionForMagic(int Magic);

        protected abstract bool HasReadableCountHeader();

        protected abstract bool ShouldWriteCountHeader(XUObject rootObject);

        protected abstract IXURCountHeader GetCountHeader();

        protected abstract Task<bool> TryBuildSectionsFromObjectAsync(XUObject xuObject);
    }
}
