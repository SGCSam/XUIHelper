using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR5Header : IXURHeader
    {
        public const int ExpectedVersion = 0x00000005;

        public int Magic { get; private set; }
        public int Version { get; private set; }
        public int Flags { get; private set; }
        public short ToolVersion { get; private set; }
        public int FileSize { get; private set; }

        public bool TryReadAsync(IXUR xur, BinaryReader reader, ILogger? logger = null)
        {
            try
            {
                logger = logger?.ForContext(typeof(XUR5Header));

                logger?.Here().Verbose("Reading XUR5 header.");
                Magic = reader.ReadInt32BE();
                if(Magic != IXURHeader.ExpectedMagic)
                {
                    logger?.Here().Error("Read magic was not the expected value, returning false. Expected: {0}, Actual: {1}", IXURHeader.ExpectedMagic, Magic);
                    return false;
                }

                Version = reader.ReadInt32BE();
                if (Version != ExpectedVersion)
                {
                    logger?.Here().Error("Read version was not the expected value, returning false. Expected: {0}, Actual: {1}", ExpectedVersion, Version);
                    return false;
                }

                Flags = reader.ReadInt32BE();
                logger?.Here().Verbose("Flags is {0:X8}", Flags);

                ToolVersion = reader.ReadInt16BE();
                logger?.Here().Verbose("ToolVersion is {0:X8}", ToolVersion);

                FileSize = reader.ReadInt32BE();
                if(FileSize != reader.BaseStream.Length)
                {
                    logger?.Here().Error("Read file size didn't match, returning false. Expected: {0}, Actual: {1}", FileSize, reader.BaseStream.Length);
                    return false;
                }

                return true;

            }
            catch(Exception ex)
            {
                logger?.Here().Error("Caught an exception when reading XUR5 header, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
