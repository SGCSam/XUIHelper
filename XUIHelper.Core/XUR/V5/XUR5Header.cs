﻿using Serilog;
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
        public short SectionsCount { get; private set; }

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR5Header));

                xur.Logger?.Here().Verbose("Reading XUR5 header.");
                Magic = reader.ReadInt32BE();
                if(Magic != IXURHeader.ExpectedMagic)
                {
                    xur.Logger?.Here().Error("Read magic was not the expected value, returning false. Expected: {0}, Actual: {1}", IXURHeader.ExpectedMagic, Magic);
                    return false;
                }

                Version = reader.ReadInt32BE();
                if (Version != ExpectedVersion)
                {
                    xur.Logger?.Here().Error("Read version was not the expected value, returning false. Expected: {0}, Actual: {1}", ExpectedVersion, Version);
                    return false;
                }

                Flags = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Flags is {0:X8}", Flags);

                ToolVersion = reader.ReadInt16BE();
                xur.Logger?.Here().Verbose("ToolVersion is {0:X8}", ToolVersion);

                FileSize = reader.ReadInt32BE();
                if(FileSize != reader.BaseStream.Length)
                {
                    xur.Logger?.Here().Error("Read file size didn't match, returning false. Expected: {0}, Actual: {1}", FileSize, reader.BaseStream.Length);
                    return false;
                }

                SectionsCount = reader.ReadInt16BE();
                xur.Logger?.Here().Verbose("Sections count is {0:X8}", SectionsCount);
               
                xur.Logger?.Here().Verbose("XUR5 header read successful!");
                return true;

            }
            catch(Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR5 header, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR5Header));
                xur.Logger?.Here().Verbose("Writing XUR5 header.");

                int bytesWritten = 0;

                xur.Logger?.Here().Verbose("Writing magic of {0:X8}.", Magic);
                writer.WriteInt32BE(Magic);
                bytesWritten += 4;

                xur.Logger?.Here().Verbose("Writing version of {0:X8}.", Version);
                writer.WriteInt32BE(Version);
                bytesWritten += 4;

                xur.Logger?.Here().Verbose("Writing flags of {0:X8}.", Flags);
                writer.WriteInt32BE(Flags);
                bytesWritten += 4;

                xur.Logger?.Here().Verbose("Writing tool version of {0:X8}.", ToolVersion);
                writer.WriteInt16BE(ToolVersion);
                bytesWritten += 2;

                xur.Logger?.Here().Verbose("Writing file size of {0:X8}.", FileSize);
                writer.WriteInt32BE(FileSize);
                bytesWritten += 4;

                xur.Logger?.Here().Verbose("Writing sections count of {0:X8}.", SectionsCount);
                writer.WriteInt16BE(SectionsCount);
                bytesWritten += 2;

                xur.Logger?.Here().Verbose("Wrote XUR5 header with a total of {0:X8} bytes successfully!", bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing XUR5 header, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public XUR5Header()
        {

        }

        public XUR5Header(int flags, int fileSize, short sectionsCount)
        {
            Magic = IXURHeader.ExpectedMagic;
            Version = 0x5;
            Flags = flags;
            ToolVersion = 0x0C;
            FileSize = fileSize;
            SectionsCount = sectionsCount;
        }
    }
}
