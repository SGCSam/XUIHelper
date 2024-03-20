using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class STRN8Section : ISTRNSection
    {
        public int Magic { get { return ISTRNSection.ExpectedMagic; } }

        public List<string> Strings { get; private set; } = new List<string>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(STRN8Section));
                xur.Logger?.Here().Verbose("Reading STRN8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ISTRNSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading strings from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int totalStringsLength = reader.ReadInt32BE();
                short stringsCount = reader.ReadInt16BE();

                Strings.Add("");

                for (int stringIndex = 0; stringIndex < stringsCount; stringIndex++)
                {
                    string readStr = reader.ReadNullTerminatedString();
                    xur.Logger?.Here().Verbose("Read a string {0}", readStr);
                    Strings.Add(readStr);
                }

                if (stringsCount != Strings.Count - 1)
                {
                    xur.Logger?.Here().Error("Mismatch of strings count when reading STRN8 section, returning false. Expected: {0:X8}, Actual: {1:X8}", stringsCount, Strings.Count - 1);
                    return false;
                }

                xur.Logger?.Here().Verbose("Read strings successfully, read a total of {0} strings", Strings.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading STRN8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Building STRN8 strings.");

                Strings.Add("");

                HashSet<string> builtStrings = new HashSet<string>();
                if (!TryBuildStringsFromObject(xur, xuObject, ref builtStrings))
                {
                    xur.Logger?.Here().Error("Failed to build strings, returning null.");
                    return false;
                }

                Strings.AddRange(builtStrings.ToList());
                xur.Logger?.Here().Verbose("Built a total of {0} STRN8 strings successfully!", Strings.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build STRN8 strings, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildStringsFromObject(IXUR xur, XUObject xuObject, ref HashSet<string> builtStrings)
        {
            try
            {
                if (!TryBuildStringsFromProperties(xur, xuObject.Properties, ref builtStrings))
                {
                    xur.Logger?.Here().Error("Failed to build strings from properties for {0}, returning false.", xuObject.ClassName);
                    return false;
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
                    if (!string.IsNullOrEmpty(childTimeline.ElementName) && builtStrings.Add(childTimeline.ElementName))
                    {
                        xur.Logger?.Here().Verbose("Added timeline string {0}.", childTimeline.ElementName);
                    }
                }

                foreach (XUNamedFrame childNamedFrame in xuObject.NamedFrames)
                {
                    if (!string.IsNullOrEmpty(childNamedFrame.Name) && builtStrings.Add(childNamedFrame.Name))
                    {
                        xur.Logger?.Here().Verbose("Added named frame string {0}.", childNamedFrame.Name);
                    }

                    if (!string.IsNullOrEmpty(childNamedFrame.TargetParameter) && builtStrings.Add(childNamedFrame.TargetParameter))
                    {
                        xur.Logger?.Here().Verbose("Added named frame target parameter {0}.", childNamedFrame.TargetParameter);
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach (XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.String)
                            {
                                if (animatedProperty.Value is not string valueString)
                                {
                                    xur.Logger?.Here().Error("Animated property {0} marked as string had a non-string value of {1}, returning false.", animatedProperty.PropertyDefinition.Name, animatedProperty.Value);
                                    return false;
                                }

                                if (!string.IsNullOrEmpty(valueString) && builtStrings.Add(valueString))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} animated property value string {1}.", animatedProperty.PropertyDefinition.Name, valueString);
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(xuObject.ClassName) && builtStrings.Add(xuObject.ClassName))
                {
                    xur.Logger?.Here().Verbose("Added class name string {0}.", xuObject.ClassName);
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build STRN5 strings for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private bool TryBuildStringsFromProperties(IXUR xur, List<XUProperty> properties, ref HashSet<string> builtStrings)
        {
            try
            {
                foreach (XUProperty childProperty in properties)
                {
                    if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.String)
                    {
                        if (childProperty.Value is not string valueString)
                        {
                            xur.Logger?.Here().Error("Child property {0} marked as string had a non-string value of {1}, returning false.", childProperty.PropertyDefinition.Name, childProperty.Value);
                            return false;
                        }

                        if (!string.IsNullOrEmpty(valueString) && builtStrings.Add(valueString))
                        {
                            xur.Logger?.Here().Verbose("Added {0} property value string {1}.", childProperty.PropertyDefinition.Name, valueString);
                        }
                    }
                    else if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if (!TryBuildStringsFromProperties(xur, childProperty.Value as List<XUProperty>, ref builtStrings))
                        {
                            xur.Logger?.Here().Error("Failed to build strings for child compound properties, returning false.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build STRN5 strings from properties, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(STRN8Section));
                xur.Logger?.Here().Verbose("Writing STRN8 section.");

                int totalStringsLength = 0;
                short stringsCount = 0;

                foreach (string str in Strings)
                {
                    if(string.IsNullOrEmpty(str))
                    {
                        continue;
                    }

                    totalStringsLength += (str.Length) + 1;
                    stringsCount++;
                }

                int bytesWritten = 0;
                writer.WriteInt32BE(totalStringsLength);
                writer.WriteInt16BE(stringsCount);
                bytesWritten += 6;

                foreach (string str in Strings)
                {
                    //This is NOT an error, the empty string doesn't get written to the STRN section
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }

                    writer.WriteNullTerminatedString(str);
                    bytesWritten += (str.Length) + 1;
                }

                xur.Logger?.Here().Verbose("Wrote a total of {0} STRN8 strings as {1:X8} bytes successfully!", Strings.Count, bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing STRN8 section, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
