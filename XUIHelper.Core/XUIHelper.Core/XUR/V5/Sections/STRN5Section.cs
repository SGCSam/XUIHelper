using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class STRN5Section : ISTRNSection
    {
        public int Magic { get { return ISTRNSection.ExpectedMagic; } }

        public List<string> Strings { get; private set; } = new List<string>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(STRN5Section));
                xur.Logger?.Here().Verbose("Reading STRN5 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ISTRNSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading strings from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                for(bytesRead = 0; bytesRead < entry.Length;)
                {
                    short stringLength = reader.ReadInt16BE();
                    xur.Logger?.Here().Verbose("Got a string length of {0:X8}", stringLength);

                    string readStr = reader.ReadUTF8String(stringLength, false);
                    Strings.Add(readStr);
                    xur.Logger?.Here().Verbose("Read a string {0}", readStr);
                    bytesRead += (2 + (stringLength * 2));
                }

                xur.Logger?.Here().Verbose("Read strings successfully, read a total of {0} strings, {1:X8} bytes.", Strings.Count, bytesRead);
                return true;
            }
            catch(Exception ex) 
            {
                xur.Logger?.Here().Error("Caught an exception when reading STRN5 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            xur.Logger?.Here().Verbose("Building STRN5 strings.");

            HashSet<string> builtStrings = new HashSet<string>();
            if(!TryBuildStringsFromObject(xur, xuObject, ref builtStrings))
            {
                xur.Logger?.Here().Error("Failed to build strings, returning null.");
                return false;
            }

            Strings = builtStrings.ToList();
            xur.Logger?.Here().Verbose("Built a total of {0} STRN5 strings successfully!", Strings.Count);
            return true;
        }

        private bool TryBuildStringsFromObject(IXUR xur, XUObject xuObject, ref HashSet<string> builtStrings)
        {
            try
            {
                if(builtStrings.Add(xuObject.ClassName))
                {
                    xur.Logger?.Here().Verbose("Added class name string {0}.", xuObject.ClassName);
                }

                foreach (XUProperty childProperty in xuObject.Properties)
                {
                    if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.String)
                    {
                        if (childProperty.Value is not string valueString)
                        {
                            xur.Logger?.Here().Error("Child property {0} marked as string had a non-string value of {1}, returning false.", childProperty.PropertyDefinition.Name, childProperty.Value);
                            return false;
                        }

                        if(builtStrings.Add(valueString))
                        {
                            xur.Logger?.Here().Verbose("Added {0} property value string {1}.", childProperty.PropertyDefinition.Name, valueString);
                        }
                    }
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildStringsFromObject(xur, childObject, ref builtStrings))
                    {
                        xur.Logger?.Here().Error("Failed to get strings for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    if(builtStrings.Add(childTimeline.ElementName))
                    {
                        xur.Logger?.Here().Verbose("Added timeline string {0}.", childTimeline.ElementName);
                    }

                    foreach(XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach(XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.String)
                            {
                                if (animatedProperty.Value is not string valueString)
                                {
                                    xur.Logger?.Here().Error("Animated property {0} marked as string had a non-string value of {1}, returning false.", animatedProperty.PropertyDefinition.Name, animatedProperty.Value);
                                    return false;
                                }

                                if (builtStrings.Add(valueString))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} animated property value string {1}.", animatedProperty.PropertyDefinition.Name, valueString);
                                }
                            }
                        }
                    }
                }

                foreach (XUNamedFrame childNamedFrame in xuObject.NamedFrames)
                {
                    if (builtStrings.Add(childNamedFrame.Name))
                    {
                        xur.Logger?.Here().Verbose("Added named frame string {0}.", childNamedFrame.Name);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build STRN5 strings for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }
    }
}
