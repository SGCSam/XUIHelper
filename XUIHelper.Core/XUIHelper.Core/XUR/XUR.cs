using Serilog;
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
        public string FilePath { get; private set; }
        public IXURHeader Header { get; protected set; }
        public IXURSectionsTable SectionsTable { get; protected set; }

        protected BinaryReader Reader { get; private set; }

        public XUR(string filePath, IXURHeader header, IXURSectionsTable sectionsTable)
        {
            FilePath = filePath;
            Header = header;
            SectionsTable = sectionsTable;
        }

        public virtual bool TryReadAsync(ILogger? logger)
        {
            try
            {
                logger = logger?.ForContext(typeof(XUR));

                if (!File.Exists(FilePath))
                {
                    logger?.Here().Error("The XUR file at {0} doesn't exist, returning false.", FilePath);
                    return false;
                }

                logger?.Here().Information("Reading XUR file at {0}", FilePath);
                Reader = new BinaryReader(File.OpenRead(FilePath));

                if (!Header.TryReadAsync(this, Reader, logger))
                {
                    logger?.Here().Error("XUR file header read has failed, returning false.", FilePath);
                    return false;
                }

                if (!SectionsTable.TryReadAsync(this, Reader, logger))
                {
                    logger?.Here().Error("XUR file sections table read has failed, returning false.", FilePath);
                    return false;
                }

                logger?.Here().Information("Read successful!");
                return true;
            }
            catch(Exception ex)
            {
                logger?.Here().Error("Caught an exception when trying to read XUR file at {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }
    }
}
