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
            try
            {
                xur.Logger?.Here().Verbose("Building QUAT5 quaternions.");
                HashSet<XUQuaternion> builtQuats = new HashSet<XUQuaternion>();
                if (!TryBuildQuaternionsFromObject(xur, xuObject, ref builtQuats))
                {
                    xur.Logger?.Here().Error("Failed to build quaternions, returning null.");
                    return false;
                }

                Quaternions = builtQuats.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} QUAT5 quaternions successfully!", Quaternions.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build QUAT5 quaternions, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildQuaternionsFromObject(IXUR xur, XUObject xuObject, ref HashSet<XUQuaternion> builtQuats)
        {
            try
            {
                if (!TryBuildQuaternionsFromProperties(xur, xuObject.Properties, ref builtQuats))
                {
                    xur.Logger?.Here().Error("Failed to build quaternions from properties for {0}, returning false.", xuObject.ClassName);
                    return false;
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildQuaternionsFromObject(xur, childObject, ref builtQuats))
                    {
                        xur.Logger?.Here().Error("Failed to get quaternions for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach (XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Quaternion)
                            {
                                if (animatedProperty.Value is not XUQuaternion valueQuat)
                                {
                                    xur.Logger?.Here().Error("Animated property {0} marked as quaternion had a non-quaternion value of {1}, returning false.", animatedProperty.PropertyDefinition.Name, animatedProperty.Value);
                                    return false;
                                }

                                if (builtQuats.Add(valueQuat))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} animated property value quaternion {1}.", animatedProperty.PropertyDefinition.Name, valueQuat);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build QUAT5 quaternions for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private bool TryBuildQuaternionsFromProperties(IXUR xur, List<XUProperty> properties, ref HashSet<XUQuaternion> builtQuats)
        {
            try
            {
                foreach (XUProperty childProperty in properties)
                {
                    if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Quaternion)
                    {
                        if (childProperty.Value is not XUQuaternion valueQuat)
                        {
                            xur.Logger?.Here().Error("Child property {0} marked as quaternion had a non-quaternion value of {1}, returning false.", childProperty.PropertyDefinition.Name, childProperty.Value);
                            return false;
                        }

                        if (builtQuats.Add(valueQuat))
                        {
                            xur.Logger?.Here().Verbose("Added {0} property value quaternion {1}.", childProperty.PropertyDefinition.Name, valueQuat);
                        }
                    }
                    else if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if (!TryBuildQuaternionsFromProperties(xur, childProperty.Value as List<XUProperty>, ref builtQuats))
                        {
                            xur.Logger?.Here().Error("Failed to build quaternions for child compound properties, returning false.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build QUAT5 quaterions from properties, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(QUAT5Section));
                xur.Logger?.Here().Verbose("Writing QUAT5 section.");

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

                xur.Logger?.Here().Verbose("Wrote a total of {0} QUAT5 quaternions as {1:X8} bytes successfully!", Quaternions.Count, bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing QUAT5 section, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
