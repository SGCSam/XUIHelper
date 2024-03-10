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
            byte[] bytes = new byte[2];
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
    }
}
