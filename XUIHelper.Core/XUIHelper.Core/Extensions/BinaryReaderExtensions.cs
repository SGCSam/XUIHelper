using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public enum Endianness
        {
            Little,
            Big,
        }

        #region UInt16
        public static ushort ReadUInt16(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadUInt16LittleEndian(reader.ReadBytes(sizeof(ushort)))
                : BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(sizeof(ushort)));
        }

        public static ushort ReadUInt16LE(this BinaryReader reader) { return ReadUInt16(reader, Endianness.Little); }
        public static ushort ReadUInt16BE(this BinaryReader reader) { return ReadUInt16(reader, Endianness.Big); }
        #endregion

        #region Int16
        public static short ReadInt16(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadInt16LittleEndian(reader.ReadBytes(sizeof(short)))
                : BinaryPrimitives.ReadInt16BigEndian(reader.ReadBytes(sizeof(short)));
        }

        public static short ReadInt16LE(this BinaryReader reader) { return ReadInt16(reader, Endianness.Little); }
        public static short ReadInt16BE(this BinaryReader reader) { return ReadInt16(reader, Endianness.Big); }
        #endregion

        #region UInt24
        public static uint ReadUInt24(this BinaryReader reader, Endianness endianness)
        {
            byte[] bytes = new byte[3];
            reader.ReadBytes(3);

            return (uint)(endianness == Endianness.Little
                ? (bytes[0] | (bytes[1] << 8) | (bytes[2] << 16))
                : (bytes[2] | (bytes[1] << 8) | (bytes[0] << 16)));
        }

        public static uint ReadUInt24LE(this BinaryReader reader) { return ReadUInt24(reader, Endianness.Little); }
        public static uint ReadUInt24BE(this BinaryReader reader) { return ReadUInt24(reader, Endianness.Big); }
        #endregion

        #region Int24
        public static int ReadInt24(this BinaryReader reader, Endianness endianness)
        {
            byte[] bytes = new byte[3];
            bytes = reader.ReadBytes(3);

            return (int)(endianness == Endianness.Little
                ? (bytes[0] | (bytes[1] << 8) | (bytes[2] << 16))
                : (bytes[2] | (bytes[1] << 8) | (bytes[0] << 16)));
        }

        public static int ReadInt24LE(this BinaryReader reader) { return ReadInt24(reader, Endianness.Little); }
        public static int ReadInt24BE(this BinaryReader reader) { return ReadInt24(reader, Endianness.Big); }
        #endregion

        #region UInt32
        public static uint ReadUInt32(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(sizeof(uint)))
                : BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(sizeof(uint)));
        }

        public static uint ReadUInt32LE(this BinaryReader reader) { return ReadUInt32(reader, Endianness.Little); }
        public static uint ReadUInt32BE(this BinaryReader reader) { return ReadUInt32(reader, Endianness.Big); }
        #endregion

        #region Int32
        public static int ReadInt32(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(sizeof(int)))
                : BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(sizeof(int)));
        }

        public static int ReadInt32LE(this BinaryReader reader) { return ReadInt32(reader, Endianness.Little); }
        public static int ReadInt32BE(this BinaryReader reader) { return ReadInt32(reader, Endianness.Big); }
        #endregion

        #region UInt64
        public static ulong ReadUInt64(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadUInt64LittleEndian(reader.ReadBytes(sizeof(ulong)))
                : BinaryPrimitives.ReadUInt64BigEndian(reader.ReadBytes(sizeof(ulong)));
        }

        public static ulong ReadUInt64LE(this BinaryReader reader) { return ReadUInt64(reader, Endianness.Little); }
        public static ulong ReadUInt64BE(this BinaryReader reader) { return ReadUInt64(reader, Endianness.Big); }
        #endregion

        #region Int64
        public static long ReadInt64(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadInt64LittleEndian(reader.ReadBytes(sizeof(long)))
                : BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(sizeof(long)));
        }

        public static long ReadInt64LE(this BinaryReader reader) { return ReadInt64(reader, Endianness.Little); }
        public static long ReadInt64BE(this BinaryReader reader) { return ReadInt64(reader, Endianness.Big); }
        #endregion

        #region Single
        public static float ReadSingle(this BinaryReader reader, Endianness endianness)
        {
            return endianness == Endianness.Little
                ? BinaryPrimitives.ReadSingleLittleEndian(reader.ReadBytes(sizeof(float)))
                : BinaryPrimitives.ReadSingleBigEndian(reader.ReadBytes(sizeof(float)));
        }

        public static float ReadSingleLE(this BinaryReader reader) { return ReadSingle(reader, Endianness.Little); }
        public static float ReadSingleBE(this BinaryReader reader) { return ReadSingle(reader, Endianness.Big); }
        #endregion

        #region String
        public static string ReadASCIIString(this BinaryReader reader, int length)
        {
            return System.Text.Encoding.ASCII.GetString(reader.ReadBytes(length));
        }

        public static string ReadUTF8String(this BinaryReader reader, int length, bool lengthIncludesSeparatorBytes = true)
        {
            string retStr = string.Empty;

            if (!lengthIncludesSeparatorBytes)
            {
                length *= 2;
            }

            for (int i = 0; i < length; i++)
            {
                char thisChar = (char)reader.ReadByte();
                if (i % 2 != 0)
                {
                    retStr += thisChar;
                }
            }

            return retStr;
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            string retStr = string.Empty;

            while (true)
            {
                byte readByte = reader.ReadByte();

                if (readByte == 0x00)
                {
                    return retStr;
                }

                else
                {
                    retStr += (char)readByte;
                }
            }
        }
        #endregion

        #region PackedULong
        public static ulong ReadPackedULong(this BinaryReader reader)
        {
            byte firstByte = reader.ReadByte();

            ulong ret = 0;

            if (firstByte != 0xFF)
            {
                if (firstByte < 0xF0)
                {
                    ret = (ulong)firstByte;
                }

                else
                {
                    byte secondByte = reader.ReadByte();
                    ulong highPart = ((ulong)firstByte << 8) & 0xF00;
                    ret = highPart | (ulong)secondByte;
                }
            }

            else
            {
                ret = (ulong)reader.ReadInt32BE();
            }

            return ret;
        }
        #endregion
    }
}
