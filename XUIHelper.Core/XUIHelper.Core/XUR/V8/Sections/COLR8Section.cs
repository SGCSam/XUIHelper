using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class COLR8Section : ICOLRSection
    {
        public int Magic { get { return ICOLRSection.ExpectedMagic; } }

        public List<XUColour> Colours { get; private set; } = new List<XUColour>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(COLR8Section));
                xur.Logger?.Here().Verbose("Reading COLR8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ICOLRSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading colours from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int colIndex = 0;
                for (int bytesRead = 0; bytesRead < entry.Length;)
                {
                    byte a = reader.ReadByte();
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();

                    XUColour readColour = new XUColour(a, r, g, b);
                    Colours.Add(readColour);
                    xur.Logger?.Here().Verbose("Read colour index {0} as {1}.", colIndex, readColour);
                    colIndex++;
                    bytesRead += 0x4;
                }

                xur.Logger?.Here().Verbose("Read colours successfully, read a total of {0} colours", Colours.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading COLR8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Building COLR8 colours.");
                HashSet<XUColour> builtColours = new HashSet<XUColour>();
                if (!TryBuildColoursFromObject(xur, xuObject, ref builtColours))
                {
                    xur.Logger?.Here().Error("Failed to build colours, returning null.");
                    return false;
                }

                Colours = builtColours.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} COLR8 colours successfully!", Colours.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build COLR8 colours, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildColoursFromObject(IXUR xur, XUObject xuObject, ref HashSet<XUColour> builtColours)
        {
            try
            {
                if (!TryBuildColoursFromProperties(xur, xuObject.Properties, ref builtColours))
                {
                    xur.Logger?.Here().Error("Failed to build colours from properties for {0}, returning false.", xuObject.ClassName);
                    return false;
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildColoursFromObject(xur, childObject, ref builtColours))
                    {
                        xur.Logger?.Here().Error("Failed to get colours for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach (XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Colour)
                            {
                                if(animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                                {
                                    int valueIndex = 0;
                                    foreach (object? valueObj in animatedProperty.Value as List<object?>)
                                    {
                                        if(animatedProperty.Value == null)
                                        {
                                            //This index isn't animated
                                            continue;
                                        }

                                        if (valueObj is not XUColour valueColour)
                                        {
                                            xur.Logger?.Here().Error("Animated indexed child property {0} at index {1} marked as colour had a non-colour value of {2}, returning false.", animatedProperty.PropertyDefinition.Name, valueIndex, valueObj);
                                            return false;
                                        }

                                        if (builtColours.Add(valueColour))
                                        {
                                            xur.Logger?.Here().Verbose("Added {0} animated indexed property value index {1} colour {2}.", animatedProperty.PropertyDefinition.Name, valueIndex, valueColour);
                                        }

                                        valueIndex++;
                                    }
                                }
                                else
                                {
                                    if (animatedProperty.Value is not XUColour valueColour)
                                    {
                                        xur.Logger?.Here().Error("Animated property {0} marked as colour had a non-colour value of {1}, returning false.", animatedProperty.PropertyDefinition.Name, animatedProperty.Value);
                                        return false;
                                    }

                                    if (builtColours.Add(valueColour))
                                    {
                                        xur.Logger?.Here().Verbose("Added {0} animated property value colour {1}.", animatedProperty.PropertyDefinition.Name, valueColour);
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build COLR8 colours for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private bool TryBuildColoursFromProperties(IXUR xur, List<XUProperty> properties, ref HashSet<XUColour> builtColours)
        {
            try
            {
                foreach (XUProperty childProperty in properties)
                {
                    if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Colour)
                    {
                        if (childProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            int valueIndex = 0;
                            foreach (object valueObj in childProperty.Value as List<object>)
                            {
                                if (valueObj is not XUColour valueColour)
                                {
                                    xur.Logger?.Here().Error("Indexed child property {0} at index {1} marked as colour had a non-colour value of {2}, returning false.", childProperty.PropertyDefinition.Name, valueIndex, valueObj);
                                    return false;
                                }

                                if (builtColours.Add(valueColour))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} indexed property value index {1} colour {2}.", childProperty.PropertyDefinition.Name, valueIndex, valueColour);
                                }

                                valueIndex++;
                            }
                        }
                        else
                        {
                            if (childProperty.Value is not XUColour valueColour)
                            {
                                xur.Logger?.Here().Error("Child property {0} marked as colour had a non-colour value of {1}, returning false.", childProperty.PropertyDefinition.Name, childProperty.Value);
                                return false;
                            }

                            if (builtColours.Add(valueColour))
                            {
                                xur.Logger?.Here().Verbose("Added {0} property value colour {1}.", childProperty.PropertyDefinition.Name, valueColour);
                            }
                        }
                    }
                    else if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if (!TryBuildColoursFromProperties(xur, childProperty.Value as List<XUProperty>, ref builtColours))
                        {
                            xur.Logger?.Here().Error("Failed to build colours for child compound properties, returning false.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build COLR8 colours from properties, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
