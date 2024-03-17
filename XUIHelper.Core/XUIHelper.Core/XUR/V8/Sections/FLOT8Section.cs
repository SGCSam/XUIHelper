using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class FLOT8Section : IFLOTSection
    {
        public int Magic { get { return IFLOTSection.ExpectedMagic; } }

        public List<float> Floats { get; private set; } = new List<float>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(FLOT8Section));
                xur.Logger?.Here().Verbose("Reading FLOT8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IFLOTSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading floats from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int floatIndex = 0;
                for(int bytesRead = 0; bytesRead < entry.Length;)
                {
                    float thisFloat = reader.ReadSingleBE();
                    Floats.Add(thisFloat);
                    xur.Logger?.Here().Verbose("Read float index {0} as {1}.", floatIndex, thisFloat);
                    floatIndex++;
                    bytesRead += 0x4;
                }

                xur.Logger?.Here().Verbose("Read floats successfully, read a total of {0} floats", Floats.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading FLOT8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Building FLOT8 floats.");
                HashSet<float> builtFloats = new HashSet<float>();
                if (!TryBuildFloatsFromObject(xur, xuObject, ref builtFloats))
                {
                    xur.Logger?.Here().Error("Failed to build floats, returning null.");
                    return false;
                }

                Floats = builtFloats.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} FLOT8 floats successfully!", Floats.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build FLOT8 floats, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildFloatsFromObject(IXUR xur, XUObject xuObject, ref HashSet<float> builtFloats)
        {
            try
            {
                if (!TryBuildFloatsFromProperties(xur, xuObject.Properties, ref builtFloats))
                {
                    xur.Logger?.Here().Error("Failed to build floats from properties for {0}, returning false.", xuObject.ClassName);
                    return false;
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildFloatsFromObject(xur, childObject, ref builtFloats))
                    {
                        xur.Logger?.Here().Error("Failed to get floats for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach (XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Float)
                            {
                                if (animatedProperty.Value is not float valueFloat)
                                {
                                    xur.Logger?.Here().Error("Animated property {0} marked as float had a non-float value of {1}, returning false.", animatedProperty.PropertyDefinition.Name, animatedProperty.Value);
                                    return false;
                                }

                                if (builtFloats.Add(valueFloat))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} animated property value float {1}.", animatedProperty.PropertyDefinition.Name, valueFloat);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build FLOT8 floats for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private bool TryBuildFloatsFromProperties(IXUR xur, List<XUProperty> properties, ref HashSet<float> builtFloats)
        {
            try
            {
                foreach (XUProperty childProperty in properties)
                {
                    if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Float)
                    {
                        if (childProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            int valueIndex = 0;
                            foreach(object valueObj in childProperty.Value as List<object>)
                            {
                                if (valueObj is not float valueFloat)
                                {
                                    xur.Logger?.Here().Error("Indexed child property {0} at index {1} marked as float had a non-float value of {2}, returning false.", childProperty.PropertyDefinition.Name, valueIndex, valueObj);
                                    return false;
                                }

                                if (builtFloats.Add(valueFloat))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} indexed property value index {1} float {2}.", childProperty.PropertyDefinition.Name, valueIndex, valueFloat);
                                }

                                valueIndex++;
                            }
                        }
                        else
                        {
                            if (childProperty.Value is not float valueFloat)
                            {
                                xur.Logger?.Here().Error("Child property {0} marked as float had a non-float value of {1}, returning false.", childProperty.PropertyDefinition.Name, childProperty.Value);
                                return false;
                            }

                            if (builtFloats.Add(valueFloat))
                            {
                                xur.Logger?.Here().Verbose("Added {0} property value float {1}.", childProperty.PropertyDefinition.Name, valueFloat);
                            }
                        }
                    }
                    else if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if (!TryBuildFloatsFromProperties(xur, childProperty.Value as List<XUProperty>, ref builtFloats))
                        {
                            xur.Logger?.Here().Error("Failed to build floats for child compound properties, returning false.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build FLOT8 floats from properties, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
