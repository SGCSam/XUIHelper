using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class QUAT5Section : IQUATSection
    {
        public int Magic { get { return IQUATSection.ExpectedMagic; } }

        public List<XUQuaternion> Quaternions { get; private set; } = new List<XUQuaternion>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(QUAT5Section));
                xur.Logger?.Here().Verbose("Reading QUAT5 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IQUATSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading quaternions from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                int quatIndex = 0;
                for (bytesRead = 0; bytesRead < entry.Length;)
                {
                    xur.Logger?.Here().Verbose("Reading quaternion index {0} from offset {1:X8}.", quatIndex, reader.BaseStream.Position);
                    float thisX = reader.ReadSingleBE();
                    float thisY = reader.ReadSingleBE();
                    float thisZ = reader.ReadSingleBE();
                    float thisW = reader.ReadSingleBE();
                    quatIndex++;
                    bytesRead += 16;

                    XUQuaternion thisQuat = new XUQuaternion(thisX, thisY, thisZ, thisW);
                    Quaternions.Add(thisQuat);
                    xur.Logger?.Here().Verbose("Read quaternion index {0} as {1}.", quatIndex, thisQuat);
                }

                xur.Logger?.Here().Verbose("Read quaternions successfully, read a total of {0} quaternions, {1:X8} bytes.", Quaternions.Count, bytesRead);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading QUAT5 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            throw new NotImplementedException();
        }
    }
}
