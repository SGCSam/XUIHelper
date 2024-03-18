using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core.Extensions
{
    internal static class BinaryWriterExtensions
    {
        #region Int16
        public static void WriteInt16(this BinaryWriter writer, Endianness endianness, short val)
        {
            byte[] bytes = new byte[sizeof(short)];
            if(endianness == Endianness.Little)
            {
                BinaryPrimitives.WriteInt16LittleEndian(bytes, val);
            }
            else
            {
                BinaryPrimitives.WriteInt16BigEndian(bytes, val);
            }

            writer.Write(bytes);
        }

        public static void WriteInt16LE(this BinaryWriter writer, short val) { WriteInt16(writer, Endianness.Little, val); }
        public static void WriteInt16BE(this BinaryWriter writer, short val) { WriteInt16(writer, Endianness.Big, val); }
        #endregion

        #region UInt32
        public static void WriteUInt32(this BinaryWriter writer, Endianness endianness, uint val)
        {
            byte[] bytes = new byte[sizeof(uint)];
            if (endianness == Endianness.Little)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(bytes, val);
            }
            else
            {
                BinaryPrimitives.WriteUInt32BigEndian(bytes, val);
            }

            writer.Write(bytes);
        }

        public static void WriteUInt32LE(this BinaryWriter writer, uint val) { WriteUInt32(writer, Endianness.Little, val); }
        public static void WriteUInt32BE(this BinaryWriter writer, uint val) { WriteUInt32(writer, Endianness.Big, val); }
        #endregion

        #region Int32
        public static void WriteInt32(this BinaryWriter writer, Endianness endianness, int val)
        {
            byte[] bytes = new byte[sizeof(int)];
            if (endianness == Endianness.Little)
            {
                BinaryPrimitives.WriteInt32LittleEndian(bytes, val);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(bytes, val);
            }

            writer.Write(bytes);
        }

        public static void WriteInt32LE(this BinaryWriter writer, int val) { WriteInt32(writer, Endianness.Little, val); }
        public static void WriteInt32BE(this BinaryWriter writer, int val) { WriteInt32(writer, Endianness.Big, val); }
        #endregion

        #region Single
        public static void WriteSingle(this BinaryWriter writer, Endianness endianness, float val)
        {
            byte[] bytes = new byte[sizeof(float)];
            if (endianness == Endianness.Little)
            {
                BinaryPrimitives.WriteSingleLittleEndian(bytes, val);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(bytes, val);
            }

            writer.Write(bytes);
        }

        public static void WriteSingleLE(this BinaryWriter writer, float val) { WriteSingle(writer, Endianness.Little, val); }
        public static void WriteSingleBE(this BinaryWriter writer, float val) { WriteSingle(writer, Endianness.Big, val); }
        #endregion

        #region String

        public static void WriteUTF8String(this BinaryWriter writer, string val)
        {
            for(int i = 0; i < val.Length; i++)
            {
                writer.Write((byte)0x00);
                writer.Write(val[i]);
            }
        }
        #endregion

        #region PackedUInt

        public static void WritePackedUInt(this BinaryWriter writer, uint val)
        {
            if (val > 0xEFF)
            {
                writer.Write((byte)0xFF);
                writer.Write((int)val);
            }
            else if (val >= 0xF0)
            {
                uint highPart = val >> 8;
                highPart |= 0xF0;
                byte lowPart = (byte)(val & 0xFF);

                writer.Write((byte)highPart);
                writer.Write((byte)lowPart);
            }
            else
            {
                writer.Write((byte)val);
            }
        }
        #endregion
    }
}
