using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public abstract class QUATSection : IQUATSection
    {
        public int Magic { get { return IQUATSection.ExpectedMagic; } }

        public List<XUQuaternion> Quaternions { get; protected set; } = new List<XUQuaternion>();

        public virtual async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(QUATSection));
                xur.Logger?.Here().Verbose("Reading QUAT section.");

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
                xur.Logger?.Here().Error("Caught an exception when reading QUAT section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public virtual async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(QUATSection));
                xur.Logger?.Here().Verbose("Building QUAT quaternions.");
                List<XUQuaternion>? builtQuats = await xuObject.TryBuildPropertyTypeAsync<XUQuaternion>(xur);
                if (builtQuats == null)
                {
                    xur.Logger?.Here().Error("Built quaternions was null, returning false.");
                    return false;
                }

                Quaternions = builtQuats.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} quaternions successfully!", Quaternions.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build quaternions, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public virtual async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(QUATSection));
                xur.Logger?.Here().Verbose("Writing QUAT section.");

                int bytesWritten = 0;
                int vectsWritten = 0;
                foreach (XUQuaternion quat in Quaternions)
                {
                    writer.WriteSingleBE(quat.X);
                    writer.WriteSingleBE(quat.Y);
                    writer.WriteSingleBE(quat.Z);
                    writer.WriteSingleBE(quat.W);
                    bytesWritten += 16;
                    xur.Logger?.Here().Verbose("Wrote quaternion index {0}: {1}.", vectsWritten, quat);
                    vectsWritten++;
                }

                xur.Logger?.Here().Verbose("Wrote a total of {0} quaternions as {1:X8} bytes successfully!", Quaternions.Count, bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing QUAT section, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
