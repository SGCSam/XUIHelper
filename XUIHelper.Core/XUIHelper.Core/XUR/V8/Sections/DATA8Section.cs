using Serilog;
using Serilog.Context;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XUIHelper.Core
{
    public class DATA8Section : IDATASection
    {
        public int Magic { get { return IDATASection.ExpectedMagic; } }

        public XMLExtensionsManager? ExtensionsManager { get; private set; }
        public ISTRNSection? STRNSection { get; private set; }

        public XUObject? RootObject { get; private set; }

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(DATA8Section));
                xur.Logger?.Here().Verbose("Reading DATA8 section.");

                if (ExtensionsManager == null)
                {
                    xur.Logger?.Here().Error("Extensions manager was null, returning false.");
                    return false;
                }

                STRNSection = xur.TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (STRNSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning false.");
                    return false;
                }

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IDATASection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading data from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                XUObject dummyParent = new XUObject("");
                RootObject = TryReadObject(xur, reader, ref dummyParent);
                if (RootObject == null)
                {
                    xur.Logger?.Here().Error("Root object was null, read must have failed, returning false.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading DATA8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private XUObject? TryReadObject(IXUR xur, BinaryReader reader, ref XUObject parentObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Reading object.");

                if (STRNSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                int stringIndex = (int)(reader.ReadPackedUInt()) - 1;
                xur.Logger?.Here().Verbose("Read string index of {0:X8}.", stringIndex);

                byte flags = reader.ReadByte();
                xur.Logger?.Here().Verbose("Read flags of {0:X8}.", flags);

                if (stringIndex < 0 || stringIndex > STRNSection.Strings.Count - 1)
                {
                    xur.Logger?.Here().Error("String index of {0:X8} is invalid, must be between 0 and {1:X8}, returning null.", stringIndex, STRNSection.Strings.Count - 1);
                    return null;
                }

                string className = STRNSection.Strings[stringIndex];

                XUObject thisObject = new XUObject(className);
                xur.Logger?.Here().Verbose("Reading class {0}.", className);

                if ((flags & 0x1) == 0x1)
                {
                    xur.Logger?.Here().Verbose("Class has properties.");
                    List<XUProperty>? readProperties = TryReadProperties(xur, reader, className);
                    if (readProperties == null)
                    {
                        xur.Logger?.Here().Error("Failed to read properties, returning null.");
                        return null;
                    }

                    thisObject.Properties = readProperties;
                    ((XUR8)xur).ReadPropertiesLists.Add(readProperties);
                }
                else if ((flags & 0x8) == 0x8)
                {
                    xur.Logger?.Here().Verbose("Class has shared properties.");
                    int propertyArrayIndex = (int)reader.ReadPackedUInt();
                    xur.Logger?.Here().Verbose("Got a shared properties index of {0}", propertyArrayIndex);

                    XUR8 xur8 = (XUR8)xur;
                    if(propertyArrayIndex < 0 || propertyArrayIndex >= xur8.ReadPropertiesLists.Count)
                    {
                        xur.Logger?.Here().Error("Failed to read shared properties as we got an invalid index of {0}. The shared properties length is {1}. Returning null.", propertyArrayIndex, xur8.ReadPropertiesLists.Count);
                        return null;
                    }

                    thisObject.Properties = new List<XUProperty>(xur8.ReadPropertiesLists[propertyArrayIndex]);
                }

                if ((flags & 0x2) == 0x2)
                {
                    xur.Logger?.Here().Verbose("Class has children, reading count.");

                    uint childrenCount = reader.ReadPackedUInt();
                    xur.Logger?.Here().Verbose("Class has {0} children.", childrenCount);

                    for (uint childIndex = 0; childIndex < childrenCount; childIndex++)
                    {
                        xur.Logger?.Here().Verbose("Reading child object index {0}.", childIndex);
                        XUObject? thisChild = TryReadObject(xur, reader, ref thisObject);
                        if (thisChild == null)
                        {
                            xur.Logger?.Here().Error("Failed to read child object index {0}, returning false.", childIndex);
                            return null;
                        }

                        thisObject.Children.Add(thisChild);
                    }
                }

                if ((flags & 0x4) == 0x4)
                {
                    xur.Logger?.Here().Verbose("Class has timeline data, reading named frames count.");

                    uint namedFramesCount = reader.ReadPackedUInt();
                    xur.Logger?.Here().Verbose("Class has {0} named frames.", namedFramesCount);

                    if(namedFramesCount > 0) 
                    {
                        int namedFrameBaseIndex = (int)reader.ReadPackedUInt();
                        xur.Logger?.Here().Verbose("Read named frame base index of {0:X8}.", namedFrameBaseIndex);

                        for (int namedFrameIndex = 0; namedFrameIndex < namedFramesCount; namedFrameIndex++)
                        {
                            int targetIndex = namedFrameBaseIndex + namedFrameIndex;
                            xur.Logger?.Here().Verbose("Reading named frame index {0}.", targetIndex);
                            XUNamedFrame? thisNamedFrame = ((XUR8)xur).TryReadNamedFrame(targetIndex);
                            if (thisNamedFrame == null)
                            {
                                xur.Logger?.Here().Error("Failed to read named frame index {0}, returning false.", targetIndex);
                                return null;
                            }

                            thisObject.NamedFrames.Add(thisNamedFrame);
                        }
                    }

                    if (thisObject.Children.Count == 0)
                    {
                        xur.Logger?.Here().Verbose("The parent object had no children, no need to load timeline data.");
                        return thisObject;
                    }

                    xur.Logger?.Here().Verbose("Reading timelines count.");
                    uint timelinesCount = reader.ReadPackedUInt();
                    xur.Logger?.Here().Verbose("Class has {0:X8} timelines.", timelinesCount);

                    if (timelinesCount == 0)
                    {
                        xur.Logger?.Here().Verbose("There are no timelines, no need to load timeline data, returning true.");
                        return thisObject;
                    }

                    for (int timelineIndex = 0; timelineIndex < timelinesCount; timelineIndex++)
                    {
                        xur.Logger?.Here().Verbose("Reading timeline index {0}.", timelineIndex);
                        XUTimeline? thisTimeline = xur.TryReadTimeline(reader, thisObject);
                        if (thisTimeline == null)
                        {
                            xur.Logger?.Here().Error("Failed to read timeline index {0}, returning false.", timelineIndex);
                            return null;
                        }

                        thisObject.Timelines.Add(thisTimeline);
                    }
                }

                return thisObject;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading object, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        private List<XUProperty>? TryReadProperties(IXUR xur, BinaryReader reader, string className)
        {
            try
            {
                xur.Logger?.Here().Verbose("Reading properties for class {0}.", className);

                List<XUClass>? classList = ExtensionsManager?.TryGetClassHierarchy(className);
                if (classList == null)
                {
                    xur.Logger?.Here().Error("Failed to get class hierarchy for class {0}, returning null.", className);
                    return null;
                }

                uint propertiesCount = reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Class {0} has {1:X8} properties.", className, propertiesCount);

                List<XUProperty> retProperties = new List<XUProperty>();
                foreach (XUClass xuClass in classList)
                {
                    xur.Logger?.Here().Verbose("Reading property data for hierarchy class {0}.", xuClass.Name);

                    uint thisPropertyMask = reader.ReadPackedUInt();
                    xur.Logger?.Here().Verbose("Handling property mask {0:X8} for hierarchy class {1}.", thisPropertyMask, xuClass.Name);

                    int propertyIndex = 0;
                    foreach (XUPropertyDefinition propertyDefinition in xuClass.PropertyDefinitions)
                    {
                        int flag = 1 << propertyIndex;

                        if ((thisPropertyMask & flag) == flag)
                        {
                            xur.Logger?.Here().Verbose("Reading {0} property.", propertyDefinition.Name);
                            XUProperty? xuProperty = xur.TryReadProperty(reader, propertyDefinition);
                            if (xuProperty == null)
                            {
                                xur.Logger?.Here().Error("Failed to read {0} property, returning null.", propertyDefinition.Name);
                                return null;
                            }

                            retProperties.Add(xuProperty);
                        }

                        propertyIndex++;
                    }
                }

                if(retProperties.Count != propertiesCount)
                {
                    xur.Logger?.Here().Error("Mismatch of properties count, returning null. Expected: {0}, Actual: {1}", propertiesCount, retProperties.Count);
                    return null;
                }

                return retProperties;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading properties, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public DATA8Section()
        {
            ExtensionsManager = XUIHelperCoreConstants.VersionedExtensions.GetValueOrDefault(0x8);
        }
    }
}
