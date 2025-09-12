using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR8ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, bool readIndex = true)
        {
            try
            {
                int indexCount = 1;
                if (readIndex && propertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                {
                    indexCount = (int)reader.ReadByte();
                    xur.Logger?.Here().Verbose("The property {0} is indexed and has an index count of {1}.", propertyDefinition.Name, indexCount);
                }

                List<object> readValues = new List<object>();
                for (int i = 0; i < indexCount; i++)
                {
                    object? value = null;

                    if (indexCount > 1)
                    {
                        xur.Logger?.Here().Verbose("Reading indexed property {0}.", i);
                    }

                    switch (propertyDefinition.Type)
                    {
                        case XUPropertyDefinitionTypes.Bool:
                        {
                            value = TryReadBoolProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Integer:
                        {
                            value = TryReadIntegerProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Unsigned:
                        {
                            value = TryReadUnsignedProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.String:
                        {
                            value = TryReadStringProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Float:
                        {
                            value = TryReadFloatProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Vector:
                        {
                            value = TryReadVectorProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Object:
                        {
                            value = TryReadObjectProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Colour:
                        {
                            value = TryReadColourProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Custom:
                        {
                            value = TryReadCustomProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Quaternion:
                        {
                            value = TryReadQuaternionProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        default:
                        {
                            xur.Logger?.Here().Error("Unhandled property type of {0} when reading property {1}, returning null.", propertyDefinition.Type, propertyDefinition.Name);
                            return null;
                        }
                    }

                    if (value == null)
                    {
                        xur.Logger?.Here().Error("Read value was null when reading property {0}, an error must have occurred, returning null.", propertyDefinition.Name);
                        return null;
                    }

                    if (!propertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        return new XUProperty(propertyDefinition, value);
                    }
                    else
                    {
                        readValues.Add(value);
                    }
                }

                return new XUProperty(propertyDefinition, readValues);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static bool? TryReadBoolProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                bool val = reader.ReadByte() > 0 ? true : false;
                xur.Logger?.Here().Verbose("Read boolean property value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryReadIntegerProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int val = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read integer property value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static uint? TryReadUnsignedProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                uint val = reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read unsigned property value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static string? TryReadStringProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int stringIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading string, got string index of {0}", stringIndex);
                return TryReadStringProperty(xur, reader, propertyDefinition, stringIndex);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static string? TryReadStringProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, int stringIndex)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                if (strnSection.Strings.Count == 0 || strnSection.Strings.Count <= stringIndex)
                {
                    xur.Logger?.Here().Error("Failed to read string as we got an invalid index of {0}. The string length is {1}. Returning null.", stringIndex, strnSection.Strings.Count);
                    return null;
                }

                string val = strnSection.Strings[stringIndex];
                xur.Logger?.Here().Verbose("Read string value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static float? TryReadFloatProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int floatIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading float, got float index of {0}", floatIndex);
                return TryReadFloatProperty(xur, reader, propertyDefinition, floatIndex);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static float? TryReadFloatProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, int floatIndex)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                IFLOTSection? flotSection = ((IXUR)xur).TryFindXURSectionByMagic<IFLOTSection>(IFLOTSection.ExpectedMagic);
                if (flotSection == null)
                {
                    xur.Logger?.Here().Error("FLOT section was null, returning null.");
                    return null;
                }

                if (flotSection.Floats.Count == 0 || flotSection.Floats.Count <= floatIndex)
                {
                    xur.Logger?.Here().Error("Failed to read float as we got an invalid index of {0}. The floats length is {1}. Returning null.", floatIndex, flotSection.Floats.Count);
                    return null;
                }

                float val = flotSection.Floats[floatIndex];
                xur.Logger?.Here().Verbose("Read float value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUVector? TryReadVectorProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int vectIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading vector, got vector index of {0}", vectIndex);
                return TryReadVectorProperty(xur, reader, propertyDefinition, vectIndex);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUVector? TryReadVectorProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, int vectIndex)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                IVECTSection? vectSection = ((IXUR)xur).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
                if (vectSection == null)
                {
                    xur.Logger?.Here().Error("VECT section was null, returning null.");
                    return null;
                }

                if (vectSection.Vectors.Count == 0 || vectSection.Vectors.Count <= vectIndex)
                {
                    xur.Logger?.Here().Error("Failed to read vector as we got an invalid index of {0}. The vectors length is {1}. Returning null.", vectIndex, vectSection.Vectors.Count);
                    return null;
                }

                XUVector val = vectSection.Vectors[vectIndex];
                xur.Logger?.Here().Verbose("Read vector value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static List<XUProperty>? TryReadObjectProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int compoundDataIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Got a compound data index of {0}.", compoundDataIndex);

                if(compoundDataIndex >= 0 && compoundDataIndex < xur.CompoundPropertyDatas.Count)
                {
                    List<XUProperty> retList = new List<XUProperty>(xur.CompoundPropertyDatas[compoundDataIndex]);
                    xur.Logger?.Here()?.Verbose("Found with the index, returning {0} properties.", retList.Count);
                    return retList;
                }

                xur.Logger?.Here()?.Verbose("Didn't find data with the index, creating new.");
                XUClass? compoundClass = null;
                switch (propertyDefinition.Name)
                {
                    case "Fill":
                    {
                        xur.Logger?.Here()?.Verbose("Reading fill object.");
                        compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureFill");
                        break;
                    }

                    case "Gradient":
                    {
                        xur.Logger?.Here()?.Verbose("Reading gradient object.");
                        compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureFillGradient");
                        break;
                    }

                    case "Stroke":
                    {
                        xur.Logger?.Here()?.Verbose("Reading stroke object.");
                        compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureStroke");
                        break;
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled compound class of {0}, returning null.", propertyDefinition.Name);
                        return null;
                    }
                }

                if (compoundClass == null)
                {
                    xur.Logger?.Here()?.Error("Compound class was null, the class lookup must have failed, returning null.");
                    return null;
                }

                int propertiesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Compound class has {0:X8} properties.", propertiesCount);

                List<XUProperty> compoundProperties = new List<XUProperty>();

                int thisPropertyMask = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Handling property mask {0:X8} for compound class {1}.", thisPropertyMask, compoundClass.Name);

                int propertyIndex = 0;
                foreach (XUPropertyDefinition compoundPropertyDefinition in compoundClass.PropertyDefinitions)
                {
                    int flag = 1 << propertyIndex;

                    if ((thisPropertyMask & flag) == flag)
                    {
                        xur.Logger?.Here().Verbose("Reading {0} property.", compoundPropertyDefinition.Name);
                        XUProperty? xuProperty = xur.TryReadProperty(reader, compoundPropertyDefinition);
                        if (xuProperty == null)
                        {
                            xur.Logger?.Here().Error("Failed to read {0} property, returning null.", compoundPropertyDefinition.Name);
                            return null;
                        }

                        compoundProperties.Add(xuProperty);
                    }

                    propertyIndex++;
                }

                xur.CompoundPropertyDatas.Add(compoundProperties);
                return compoundProperties;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUColour? TryReadColourProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int colourIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading colour, got colour index of {0}", colourIndex);
                return TryReadColourProperty(xur, reader, propertyDefinition, colourIndex);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUColour? TryReadColourProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, int colourIndex)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                ICOLRSection? colrSection = ((IXUR)xur).TryFindXURSectionByMagic<ICOLRSection>(ICOLRSection.ExpectedMagic);
                if (colrSection == null)
                {
                    xur.Logger?.Here().Error("COLR section was null, returning null.");
                    return null;
                }

                if (colrSection.Colours.Count == 0 || colrSection.Colours.Count <= colourIndex)
                {
                    xur.Logger?.Here().Error("Failed to read colour as we got an invalid index of {0}. The colours length is {1}. Returning null.", colourIndex, colrSection.Colours.Count);
                    return null;
                }

                XUColour val = colrSection.Colours[colourIndex];
                xur.Logger?.Here().Verbose("Read colour value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUFigure? TryReadCustomProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int custOffset = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading custom, got offset index of {0:X8}", custOffset);

                ICUSTSection? custSection = ((IXUR)xur).TryFindXURSectionByMagic<ICUSTSection>(ICUSTSection.ExpectedMagic);
                if (custSection == null)
                {
                    xur.Logger?.Here().Error("CUST section was null, returning null.");
                    return null;
                }

                if (custSection.Figures.Count <= 0)
                {
                    xur.Logger?.Here().Error("Failed to read custom as we have no figures, returning null.");
                    return null;
                }

                if (custOffset == 0)
                {
                    xur.Logger?.Here().Verbose("Custom offset is 0, returning first figure.");
                    return custSection.Figures[0];
                }
                else
                {
                    int calculatedOffset = 0;
                    for (int i = 0; i < custSection.Figures.Count - 1; i++)
                    {
                        XUFigure thisFigure = custSection.Figures[i];
                        calculatedOffset += 0x10 + (thisFigure.Points.Count * 0x18);

                        if (calculatedOffset == custOffset)
                        {
                            xur.Logger?.Here().Verbose("Custom offset is {0:X8}, returning figure at index {1}.", custOffset, i + 1);
                            return custSection.Figures[i + 1];
                        }
                        else if (calculatedOffset > custOffset)
                        {
                            break;
                        }
                    }

                    xur.Logger?.Here().Error("Failed to find figured with custom offset {0:X8}, returning null.", custOffset);
                    return null;
                }
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUQuaternion? TryReadQuaternionProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int quatIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading quaternion, got quaternion index of {0}", quatIndex);
                return TryReadQuaternionProperty(xur, reader, propertyDefinition, quatIndex);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUQuaternion? TryReadQuaternionProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, int quatIndex)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                IQUATSection? quatSection = ((IXUR)xur).TryFindXURSectionByMagic<IQUATSection>(IQUATSection.ExpectedMagic);
                if (quatSection == null)
                {
                    xur.Logger?.Here().Error("QUAT section was null, returning null.");
                    return null;
                }

                if (quatSection.Quaternions.Count == 0 || quatSection.Quaternions.Count <= quatIndex)
                {
                    xur.Logger?.Here().Error("Failed to read quaternion as we got an invalid index of {0}. The quaternions length is {1}. Returning null.", quatIndex, quatSection.Quaternions.Count);
                    return null;
                }

                XUQuaternion val = quatSection.Quaternions[quatIndex];
                xur.Logger?.Here().Verbose("Read quaternion value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUNamedFrame? TryReadNamedFrame(this XUR8 xur, int index)
        {
            try
            {
                INAMESection? nameSection = ((IXUR)xur).TryFindXURSectionByMagic<INAMESection>(INAMESection.ExpectedMagic);
                if (nameSection == null)
                {
                    xur.Logger?.Here().Error("NAME section was null, returning null.");
                    return null;
                }

                if (nameSection.NamedFrames.Count == 0 || nameSection.NamedFrames.Count <= index)
                {
                    xur.Logger?.Here().Error("Failed to read named frame as we got an invalid index of {0}. The named frames length is {1}. Returning null.", index, nameSection.NamedFrames.Count);
                    return null;
                }

                XUNamedFrame val = nameSection.NamedFrames[index];
                xur.Logger?.Here().Verbose("Read named frame value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static XUTimeline? TryReadTimeline(this XUR8 xur, BinaryReader reader, XUObject obj)
        {
            try
            {
                int objectNameStringIndex = (int)(reader.ReadPackedUInt());
                xur.Logger?.Here().Verbose("Read object name string index of {0:X8}.", objectNameStringIndex);

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                if (strnSection.Strings.Count == 0 || strnSection.Strings.Count <= objectNameStringIndex)
                {
                    xur.Logger?.Here().Error("Failed to read string as we got an invalid index of {0}. The strings length is {1}. Returning null.", objectNameStringIndex, strnSection.Strings.Count);
                    return null;
                }

                if (objectNameStringIndex < 0 || objectNameStringIndex > strnSection.Strings.Count - 1)
                {
                    xur.Logger?.Here().Error("String index of {0:X8} is invalid, must be between 0 and {1}, returning null.", objectNameStringIndex, strnSection.Strings.Count - 1);
                    return null;
                }

                string objectName = strnSection.Strings[objectNameStringIndex];
                xur.Logger?.Here().Verbose("Got an object name of {0}.", objectName);

                XUObject? elementObject = obj.TryFindChildById(objectName);
                if (elementObject == null)
                {
                    xur.Logger?.Here().Error("Failed to find object, returning null.");
                    return null;
                }

                xur.Logger?.Here().Verbose("Found object has a class name of {0}.", elementObject.ClassName);
                List<XUClass>? classList = XMLExtensionsManager.TryGetClassHierarchy(elementObject.ClassName);
                if (classList == null)
                {
                    Log.Error(string.Format("Failed to get {0} class hierarchy, returning false.", elementObject.ClassName));
                    return null;
                }
                classList.Reverse();

                uint propertyDefinitionsCount = reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Got a count of {0} property definitions.", propertyDefinitionsCount);

                List<XUPropertyDefinition> animatedPropertyDefinitions = new List<XUPropertyDefinition>();
                List<int> indexedPropertyIndexes = new List<int>();
                for (int i = 0; i < propertyDefinitionsCount; i++)
                {
                    byte packedByte = reader.ReadByte();
                    xur.Logger?.Here().Verbose("Read a packed byte {0:X8}", packedByte);
                    int classDepth = (packedByte & 0x7F);
                    xur.Logger?.Here().Verbose("Read a class depth of {0:X8}", classDepth);
                    bool isIndexed = ((packedByte & 0x80) != 0);
                    xur.Logger?.Here().Verbose("Is indexed is {0}", isIndexed);

                    byte classIndex = reader.ReadByte();
                    xur.Logger?.Here().Verbose("Read a class index of {0:X8}", classIndex);

                    if (classIndex < 0 || classIndex > classList.Count - 1)
                    {
                        xur.Logger?.Here().Error("Class index of {0:X8} is invalid, must be between 0 and {1}, returning null.", classIndex, classList.Count - 1);
                        return null;
                    }

                    byte propertyIndex = 0;
                    XUClass? classAtIndex = classList[classIndex];
                    for (int j = 0; j < classDepth; j++)
                    {
                        propertyIndex = reader.ReadByte();
                        xur.Logger?.Here().Verbose("Read a property index of {0:X8}", propertyIndex);

                        if (classAtIndex == null)
                        {
                            xur.Logger?.Here()?.Error("Class at index was null, the class lookup must have failed, returning null.");
                            return null;
                        }

                        XUPropertyDefinition thisPropDef = classAtIndex.PropertyDefinitions[propertyIndex];
                        xur.Logger?.Here().Verbose("Handling animated property for {0}.", thisPropDef.Name);

                        if (j != classDepth - 1)
                        {
                            switch (thisPropDef.Name)
                            {
                                case "Fill":
                                {
                                    xur.Logger?.Here()?.Verbose("Got a class depth of {0}, handling fill.", classDepth);
                                    classAtIndex = XMLExtensionsManager.TryGetClassByName("XuiFigureFill");
                                    break;
                                }

                                case "Gradient":
                                {
                                    xur.Logger?.Here()?.Verbose("Got a class depth of {0}, handling gradient.", classDepth);
                                    classAtIndex = XMLExtensionsManager.TryGetClassByName("XuiFigureFillGradient");
                                    break;
                                }

                                case "Stroke":
                                {
                                    xur.Logger?.Here()?.Verbose("Got a class depth of {0}, handling stroke.", classDepth);
                                    classAtIndex = XMLExtensionsManager.TryGetClassByName("XuiFigureStroke");
                                    break;
                                }
                                default:
                                {
                                    xur.Logger?.Here().Error("Unhandled compound class of {0}, returning null.", thisPropDef.Name);
                                    return null;
                                }
                            }
                        }
                    }

                    if (classAtIndex == null)
                    {
                        xur.Logger?.Here()?.Error("Class at index was null, the class lookup must have failed, returning null.");
                        return null;
                    }

                    if (propertyIndex < 0 || propertyIndex >= classAtIndex.PropertyDefinitions.Count)
                    {
                        xur.Logger?.Here().Error("Property index of {0:X8} is invalid, must be between 0 and {1}, returning null.", propertyIndex, classAtIndex.PropertyDefinitions.Count);
                        return null;
                    }

                    xur.Logger?.Here().Verbose("Property {0} of {1} is animated.", classAtIndex.PropertyDefinitions[propertyIndex].Name, classAtIndex.Name);
                    animatedPropertyDefinitions.Add(classAtIndex.PropertyDefinitions[propertyIndex]);

                    if (isIndexed)
                    {
                        int index = (int)reader.ReadPackedUInt();
                        xur.Logger?.Here()?.Verbose("Read a timeline compound index value of {0}", index);
                        indexedPropertyIndexes.Add(index);
                    }
                }

                int keyframesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Timeline for {0} has {1} keyframes.", objectName, keyframesCount);

                int keyframeBaseIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read a keyframe base index of {0:X8}", keyframeBaseIndex);

                IKEYDSection? keydSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYDSection>(IKEYDSection.ExpectedMagic);
                if (keydSection == null)
                {
                    xur.Logger?.Here().Error("KEYD section was null, returning null.");
                    return null;
                }

                IKEYPSection? keypSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYPSection>(IKEYPSection.ExpectedMagic);
                if (keypSection == null)
                {
                    xur.Logger?.Here().Error("KEYP section was null, returning null.");
                    return null;
                }

                List<XUKeyframe> keyframes = new List<XUKeyframe>();
                for (int keyframeIndex = 0; keyframeIndex < keyframesCount; keyframeIndex++)
                {
                    int targetDataIndex = keyframeBaseIndex + keyframeIndex;
                    if (targetDataIndex < 0 || targetDataIndex > keydSection.Keyframes.Count - 1)
                    {
                        xur.Logger?.Here().Error("Data index of {0:X8} is invalid, must be between 0 and {1}, returning null.", targetDataIndex, keydSection.Keyframes.Count - 1);
                        return null;
                    }
                    XURKeyframe keyframeData = keydSection.Keyframes[targetDataIndex];

                    List<XUProperty> animatedProperties = new List<XUProperty>();
                    int handledIndexedProperties = 0;
                    int animatedPropertyDefinitionIndex = 0;
                    foreach (XUPropertyDefinition animatedPropertyDefinition in animatedPropertyDefinitions)
                    {
                        int targetPropertyIndex = keyframeData.PropertyIndex + animatedPropertyDefinitionIndex;
                        if (targetPropertyIndex < 0 || targetPropertyIndex > keypSection.PropertyIndexes.Count - 1)
                        {
                            xur.Logger?.Here().Error("Property index of {0:X8} is invalid, must be between 0 and {1}, returning null.", targetPropertyIndex, keypSection.PropertyIndexes.Count - 1);
                            return null;
                        }
                        int thisPropertyIndex = (int)keypSection.PropertyIndexes[targetPropertyIndex];

                        xur.Logger?.Here().Verbose("Reading animated property {0}.", animatedPropertyDefinition.Name);
                        XUProperty? xuProperty = null;
                        switch(animatedPropertyDefinition.Type)
                        {
                            case XUPropertyDefinitionTypes.Integer:
                            {
                                xur.Logger?.Here().Verbose("Animated property is integer, using value {0}.", thisPropertyIndex);
                                xuProperty = new XUProperty(animatedPropertyDefinition, thisPropertyIndex);
                                break;
                            }
                            case XUPropertyDefinitionTypes.Unsigned:
                            {
                                xur.Logger?.Here().Verbose("Animated property is unsigned, using value {0}.", thisPropertyIndex);
                                xuProperty = new XUProperty(animatedPropertyDefinition, (uint)thisPropertyIndex);
                                break;
                            }
                            case XUPropertyDefinitionTypes.Bool:
                            {
                                xur.Logger?.Here().Verbose("Animated property is boolean, using value {0}.", thisPropertyIndex);
                                xuProperty = new XUProperty(animatedPropertyDefinition, thisPropertyIndex != 0);
                                break;
                            }
                            case XUPropertyDefinitionTypes.String:
                            {
                                xur.Logger?.Here().Verbose("Animated property is string, using value {0}.", thisPropertyIndex);
                                string? val = xur.TryReadStringProperty(reader, animatedPropertyDefinition, thisPropertyIndex);
                                if(val == null)
                                {
                                    xur.Logger?.Here().Error("Read string was null, returning null.");
                                    return null;
                                }

                                xuProperty = new XUProperty(animatedPropertyDefinition, val);
                                break;
                            }
                            case XUPropertyDefinitionTypes.Float:
                            {
                                xur.Logger?.Here().Verbose("Animated property is float, using value {0}.", thisPropertyIndex);
                                float? val = xur.TryReadFloatProperty(reader, animatedPropertyDefinition, thisPropertyIndex);
                                if (val == null)
                                {
                                    xur.Logger?.Here().Error("Read float was null, returning null.");
                                    return null;
                                }

                                xuProperty = new XUProperty(animatedPropertyDefinition, val);
                                break;
                            }
                            case XUPropertyDefinitionTypes.Colour:
                            {
                                xur.Logger?.Here().Verbose("Animated property is colour, using value {0}.", thisPropertyIndex);
                                XUColour? val = xur.TryReadColourProperty(reader, animatedPropertyDefinition, thisPropertyIndex);
                                if (val == null)
                                {
                                    xur.Logger?.Here().Error("Read colour was null, returning null.");
                                    return null;
                                }

                                xuProperty = new XUProperty(animatedPropertyDefinition, val);
                                break;
                            }
                            case XUPropertyDefinitionTypes.Vector:
                            {
                                xur.Logger?.Here().Verbose("Animated property is vector, using value {0}.", thisPropertyIndex);
                                XUVector? val = xur.TryReadVectorProperty(reader, animatedPropertyDefinition, thisPropertyIndex);
                                if (val == null)
                                {
                                    xur.Logger?.Here().Error("Read vector was null, returning null.");
                                    return null;
                                }

                                xuProperty = new XUProperty(animatedPropertyDefinition, val);
                                break;
                            }
                            case XUPropertyDefinitionTypes.Quaternion:
                            {
                                xur.Logger?.Here().Verbose("Animated property is quaternion, using value {0}.", thisPropertyIndex);
                                XUQuaternion? val = xur.TryReadQuaternionProperty(reader, animatedPropertyDefinition, thisPropertyIndex);
                                if (val == null)
                                {
                                    xur.Logger?.Here().Error("Read quaternion was null, returning null.");
                                    return null;
                                }

                                xuProperty = new XUProperty(animatedPropertyDefinition, val);
                                break;
                            }
                            default:
                            {
                                xur.Logger?.Here().Error("Unhandled property type {0} for animated property, returning null.", animatedPropertyDefinition.Type);
                                return null;
                            }
                        }

                        if (xuProperty == null)
                        {
                            xur.Logger?.Here().Error("Failed to read {0} property, returning null.", animatedPropertyDefinition.Name);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Animated property {0} has a value of {1} at keyframe {2}.", animatedPropertyDefinition.Name, xuProperty.Value, keyframeIndex);
                        if (animatedPropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            xuProperty = new XUProperty(xuProperty.PropertyDefinition, new List<object>() { xuProperty.Value });

                            if (handledIndexedProperties < 0 || handledIndexedProperties >= indexedPropertyIndexes.Count)
                            {
                                xur.Logger?.Here().Error("Indexed properties index of {0:X8} is invalid, must be between 0 and {1}, returning null.", handledIndexedProperties, indexedPropertyIndexes.Count);
                                return null;
                            }

                            int indexToUse = indexedPropertyIndexes[handledIndexedProperties];
                            xur.Logger?.Here().Verbose("The property {0} is indexed, using index of {1}.", animatedPropertyDefinition.Name, indexToUse);

                            bool found = false;
                            foreach (XUProperty addedAnimatedProperty in animatedProperties)
                            {
                                if (addedAnimatedProperty.PropertyDefinition == xuProperty.PropertyDefinition)
                                {
                                    if (addedAnimatedProperty.Value is List<object> addedList)
                                    {
                                        if (xuProperty.Value is List<object> readList)
                                        {
                                            addedList[indexToUse] = readList[0];
                                            handledIndexedProperties++;
                                            found = true;
                                            break;
                                        }
                                        else
                                        {
                                            xur.Logger?.Here().Error("Read animated property was not a list value type, returning null.");
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        xur.Logger?.Here().Error("Added animated property was not a list value type, returning null.");
                                        return null;
                                    }
                                }
                            }

                            if (found)
                            {
                                //Don't re-add the XUProperty again if we've just updated the existing one
                                animatedPropertyDefinitionIndex++;
                                continue;
                            }

                            //Not all indexed property values will be animated. We may have 6 StopPos for colours, and the 6 positions may change,
                            //But only 2 of the 6 positions may actually have different colours
                            //If this happens, we grab the property definition from the element we're adding and initialize our values to be the same
                            //Only overriding any index that is animated
                            List<object>? indexedValues = (List<object>?)elementObject.TryGetPropertyDefinitionValue(xuProperty.PropertyDefinition);
                            if (indexedValues == null)
                            {
                                xur.Logger?.Here().Error("Failed to find indexed values for property definition {0}, returning null.", xuProperty.PropertyDefinition.Name);
                                return null;
                            }

                            List<object?> values = new List<object?>();
                            for (int j = 0; j < indexedValues.Count; j++)
                            {
                                values.Add(indexedValues[j]);
                            }

                            values[indexToUse] = (xuProperty.Value as List<object>)[0];
                            animatedProperties.Add(new XUProperty(xuProperty.PropertyDefinition, values));
                            handledIndexedProperties++;
                            animatedPropertyDefinitionIndex++;
                        }
                        else
                        {
                            animatedProperties.Add(xuProperty);
                            animatedPropertyDefinitionIndex++;
                        }
                    }

                    keyframes.Add(new XUKeyframe(keyframeData, animatedProperties));
                }

                return new XUTimeline(objectName, keyframes);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
