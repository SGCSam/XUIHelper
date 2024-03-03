using Serilog;
using Serilog.Core;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public interface IXUR
    {
        ILogger? Logger { get; set; }
        string FilePath { get; }
        IXURHeader Header { get; }
        IXURSectionsTable SectionsTable { get; }
        List<IXURSection> Sections { get; }

        public Task<bool> TryReadAsync();

        public XURSectionTableEntry? TryGetXURSectionTableEntryForMagic(int Magic)
        {
            try
            {
                Logger?.Here().Verbose("Trying to get XUR section table entry for magic {0:X8}", Magic);

                foreach (XURSectionTableEntry entry in SectionsTable.Entries)
                {
                    if (entry.Magic == Magic)
                    {
                        Logger?.Here().Verbose("Found entry successfully!");
                        return entry;
                    }
                }

                Logger?.Here().Error("Failed to find entry, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to get XUR section table entry for magic {0:X8}, returning null. The exception is: {1}", Magic, ex);
                return null;
            }
        }

        public T? TryFindXURSectionByMagic<T>(int Magic)
        {
            foreach(IXURSection section in Sections)
            {
                if(section.Magic == Magic)
                {
                    return (T?)section;
                }
            }

            return default(T);
        }
    }
}
