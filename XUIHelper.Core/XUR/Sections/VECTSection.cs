using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public abstract class VECTSection : IVECTSection
    {
        public int Magic { get { return IVECTSection.ExpectedMagic; } }

        public List<XUVector> Vectors { get; protected set; } = new List<XUVector>();

        public virtual async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(VECTSection));
                xur.Logger?.Here().Verbose("Reading VECT section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IVECTSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading vectors from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                int vectIndex = 0;
                for (bytesRead = 0; bytesRead < entry.Length;)
                {
                    xur.Logger?.Here().Verbose("Reading vector index {0} from offset {1:X8}.", vectIndex, reader.BaseStream.Position);
                    float thisX = reader.ReadSingleBE();
                    float thisY = reader.ReadSingleBE();
                    float thisZ = reader.ReadSingleBE();
                    vectIndex++;
                    bytesRead += 12;

                    XUVector thisVector = new XUVector(thisX, thisY, thisZ);
                    Vectors.Add(thisVector);
                    xur.Logger?.Here().Verbose("Read vector index {0} as {1}.", vectIndex, thisVector);
                }

                xur.Logger?.Here().Verbose("Read vectors successfully, read a total of {0} vectors, {1:X8} bytes.", Vectors.Count, bytesRead);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading VECT section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public virtual async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(VECTSection));
                xur.Logger?.Here().Verbose("Building VECT vectors.");
                List<XUVector>? builtVects = await xuObject.TryBuildPropertyTypeAsync<XUVector>(xur);
                if (builtVects == null)
                {
                    xur.Logger?.Here().Error("Built vectors was null, returning false.");
                    return false;
                }

                Vectors = builtVects.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} vectors successfully!", Vectors.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build vectors, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public virtual async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(VECTSection));
                xur.Logger?.Here().Verbose("Writing VECT section.");

                int bytesWritten = 0;
                int vectsWritten = 0;
                foreach (XUVector vect in Vectors)
                {
                    writer.WriteSingleBE(vect.X);
                    writer.WriteSingleBE(vect.Y);
                    writer.WriteSingleBE(vect.Z);
                    bytesWritten += 12;
                    xur.Logger?.Here().Verbose("Wrote vector index {0}: {1}.", vectsWritten, vect);
                    vectsWritten++;
                }

                xur.Logger?.Here().Verbose("Wrote a total of {0} vectors as {1:X8} bytes successfully!", Vectors.Count, bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing VECT section, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
