using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class KEYP8Section : IKEYPSection
    {
        public int Magic { get { return IKEYPSection.ExpectedMagic; } }

        public List<uint> PropertyIndexes { get; private set; } = new List<uint>();

        public List<List<uint>> GroupedPropertyIndexes { get; private set; } = new List<List<uint>>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(KEYP8Section));
                xur.Logger?.Here().Verbose("Reading KEYP8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IKEYPSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading keyframe property indexes from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int index = 0;
                for (int bytesRead = 0; bytesRead < entry.Length;)
                {
                    byte packedBytesRead = 0;
                    uint readIndex = reader.ReadPackedUInt(out packedBytesRead);
                    PropertyIndexes.Add(readIndex);
                    xur.Logger?.Here().Verbose("Read keyframe property index {0} as {1}.", index, readIndex);
                    index++;
                    bytesRead += packedBytesRead;
                }

                xur.Logger?.Here().Verbose("Read keyframe property indexes successfully, read a total of {0} indexes", PropertyIndexes.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading KEYP8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Building KEYP8 indexes.");
                List<uint> builtIndexes = new List<uint>();
                List<List<uint>> groupedPropertyIndexes = new List<List<uint>>();

                if (!TryBuildIndexesFromObject(xur, xuObject, ref builtIndexes, ref groupedPropertyIndexes))
                {
                    xur.Logger?.Here().Error("Failed to build indexes, returning null.");
                    return false;
                }

                PropertyIndexes = builtIndexes.ToList();
                GroupedPropertyIndexes = groupedPropertyIndexes;

                xur.Logger?.Here().Verbose("Built a total of {0} KEYP8 indexes successfully!", PropertyIndexes.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when building KEYP8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildIndexesFromObject(IXUR xur, XUObject xuObject, ref List<uint> builtIndexes, ref List<List<uint>> groupedPropertyIndexes)
        {
            try
            {
                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildIndexesFromObject(xur, childObject, ref builtIndexes, ref groupedPropertyIndexes))
                    {
                        xur.Logger?.Here().Error("Failed to get indexes for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        List<uint>? propertyIndexes = TryGetKeyframePropertyIndexes(xur, childKeyframe);
                        if (propertyIndexes == null)
                        {
                            xur.Logger?.Here().Error("Failed to get property indexes, returning false.");
                            return false;
                        }

                        bool found = false;
                        foreach(List<uint> indexGroup in groupedPropertyIndexes)
                        {
                            if(propertyIndexes.SequenceEqual(indexGroup))
                            {
                                xur.Logger?.Here().Verbose("Found an identical sequence of property indexes, we won't re-add these. The sequence is: {0}", string.Join(" ", propertyIndexes));
                                found = true;
                                break;
                            }
                        }

                        //I think Microsoft actually missed a chance to be more efficient with XUR8 here unless I'm mistaken
                        //For some reason, even though this index may already be in the built indexes list,
                        //it'll add it again if that index was not part of an identical list of indexes from some animated properties
                        //i.e. if we have "Show" as part of a list of animated properties "Show", "ClassOverride, "Visual",
                        //we'll add "Show "again if it's the only one in the list. The only time we wouldn't re-add is if we found EXACTLY
                        //"Show, "ClassOverride" and "Visual" again, which is what we're checking against with buildPropertyIndexes
                        if (!found)
                        {
                            builtIndexes.AddRange(propertyIndexes);
                            groupedPropertyIndexes.Add(propertyIndexes);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build KEYP8 indexes for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private static uint? TryGetPropertyIndexForAnimatedPropertyValue(IXUR xur, XUPropertyDefinition animatedPropertyDefinition, object val)
        {
            try
            {
                switch (animatedPropertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        if (val is not int indexVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return (uint)indexVal;
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        if (val is not uint indexVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return indexVal;
                    }
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        if (val is not bool boolVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        uint indexVal = boolVal ? 1u : 0u;
                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return indexVal;
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        if (val is not string strVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        ISTRNSection? strnSection = xur.TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                        if (strnSection == null)
                        {
                            xur.Logger?.Here().Error("STRN section was null when trying to add property index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        int indexVal = strnSection.Strings.IndexOf(strVal);
                        if (indexVal == -1)
                        {
                            xur.Logger?.Here().Error("Failed to find string index for animated property {0} with value {1}, returning false.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return (uint)indexVal;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        if (val is not float floatVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        IFLOTSection? flotSection = xur.TryFindXURSectionByMagic<IFLOTSection>(IFLOTSection.ExpectedMagic);
                        if (flotSection == null)
                        {
                            xur.Logger?.Here().Error("FLOT section was null when trying to add property index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        int indexVal = flotSection.Floats.IndexOf(floatVal);
                        if (indexVal == -1)
                        {
                            xur.Logger?.Here().Error("Failed to find float index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return (uint)indexVal;
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        if (val is not XUColour colourVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        ICOLRSection? colrSection = xur.TryFindXURSectionByMagic<ICOLRSection>(ICOLRSection.ExpectedMagic);
                        if (colrSection == null)
                        {
                            xur.Logger?.Here().Error("COLR section was null when trying to add property index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        int indexVal = colrSection.Colours.IndexOf(colourVal);
                        if (indexVal == -1)
                        {
                            xur.Logger?.Here().Error("Failed to find colour index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return (uint)indexVal;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        if (val is not XUVector vectVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        IVECTSection? vectSection = xur.TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
                        if (vectSection == null)
                        {
                            xur.Logger?.Here().Error("VECT section was null when trying to add property index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        int indexVal = vectSection.Vectors.IndexOf(vectVal);
                        if (indexVal == -1)
                        {
                            xur.Logger?.Here().Error("Failed to find vector index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return (uint)indexVal;
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        if (val is not XUQuaternion quatVal)
                        {
                            xur.Logger?.Here().Error("Animated property {0} of type {1} had an unexpected value type {2}, returning null.", animatedPropertyDefinition.Name, animatedPropertyDefinition.Type, val);
                            return null;
                        }

                        IQUATSection? quatSection = xur.TryFindXURSectionByMagic<IQUATSection>(IQUATSection.ExpectedMagic);
                        if (quatSection == null)
                        {
                            xur.Logger?.Here().Error("QUAT section was null when trying to add property index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        int indexVal = quatSection.Quaternions.IndexOf(quatVal);
                        if (indexVal == -1)
                        {
                            xur.Logger?.Here().Error("Failed to find quaternion index for animated property {0} with value {1}, returning null.", animatedPropertyDefinition.Name, val);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Added animated property {0} index value {1}.", animatedPropertyDefinition.Name, indexVal);
                        return (uint)indexVal;
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled type {0} for animated property {1}, returning null.", animatedPropertyDefinition.Type, animatedPropertyDefinition.Name);
                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to add property index for animated property {0}, returning null. The exception is: {1}", animatedPropertyDefinition.Name, ex);
                return null;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(KEYP8Section));
                xur.Logger?.Here().Verbose("Writing KEYP8 section.");

                int bytesWritten = 0;
                int indexesWritten = 0;
                foreach (uint propertyIndex in PropertyIndexes)
                {
                    int indexBytesWritten = 0;
                    writer.WritePackedUInt(propertyIndex, out indexBytesWritten);
                    bytesWritten += indexBytesWritten;
                    xur.Logger?.Here().Verbose("Wrote property index {0} of {1} bytes: {2}.", indexesWritten, indexBytesWritten, propertyIndex);
                    indexesWritten++;
                }

                xur.Logger?.Here().Verbose("Wrote a total of {0} KEYP8 indexes as {1:X8} bytes successfully!", PropertyIndexes.Count, bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing KEYP8 section, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static List<uint>? TryGetKeyframePropertyIndexes(IXUR xur, XUKeyframe keyframe)
        {
            try
            {
                List<uint> retIndexes = new List<uint>();
                foreach (XUProperty animatedProperty in keyframe.Properties)
                {
                    if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        int valueIndex = 0;
                        foreach (object? val in animatedProperty.Value as List<object?>)
                        {
                            if (val == null)
                            {
                                //This index isn't animated
                                valueIndex++;
                                continue;
                            }

                            uint? index = TryGetPropertyIndexForAnimatedPropertyValue(xur, animatedProperty.PropertyDefinition, val);
                            if (index == null)
                            {
                                xur.Logger?.Here().Error("Failed to get property index for indexed animated property {0} at index {1}, returning null.", animatedProperty.PropertyDefinition.Name, valueIndex);
                                return null;
                            }

                            retIndexes.Add(index.Value);
                        }
                    }
                    else
                    {
                        uint? index = TryGetPropertyIndexForAnimatedPropertyValue(xur, animatedProperty.PropertyDefinition, animatedProperty.Value);
                        if (index == null)
                        {
                            xur.Logger?.Here().Error("Failed to get property index for animated property {0}, returning null.", animatedProperty.PropertyDefinition.Name);
                            return null;
                        }

                        retIndexes.Add(index.Value);
                    }
                }

                return retIndexes;
            }
            catch(Exception ex) 
            {
                xur.Logger?.Here().Error("Caught an exception when trying to get keyframe property indexes, returning false. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
