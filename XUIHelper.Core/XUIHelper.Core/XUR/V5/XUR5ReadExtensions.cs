using Serilog;
using Serilog.Core;
using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR5ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, bool readIndex = true)
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
                for(int i = 0; i < indexCount; i++)
                {
                    object? value = null;

                    if(indexCount > 1)
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

                    if(value == null)
                    {
                        xur.Logger?.Here().Error("Read value was null when reading property {0}, an error must have occurred, returning null.", propertyDefinition.Name);
                        return null;
                    }

                    if(!propertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
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

        public static bool? TryReadBoolProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if(propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                bool val = reader.ReadByte() > 0 ? true : false;
                xur.Logger?.Here().Verbose("Read {0} boolean property value of {1}.", propertyDefinition.Name, val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryReadIntegerProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int val = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read {0} integer property value of {1}.", propertyDefinition.Name, val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static uint? TryReadUnsignedProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                uint val = reader.ReadUInt32BE();
                xur.Logger?.Here().Verbose("Read {0} unsigned property value of {1}.", propertyDefinition.Name, val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static string? TryReadStringProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int stringIndex = reader.ReadInt16BE();
                xur.Logger?.Here()?.Verbose("Reading string, got string index of {0}", stringIndex);

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                if (strnSection.Strings.Count == 0 || strnSection.Strings.Count <= stringIndex)
                {
                    xur.Logger?.Here().Error("Failed to read string as we got an invalid index of {0}. The strings length is {1}. Returning null.", stringIndex, strnSection.Strings.Count);
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

        public static float? TryReadFloatProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                float val = reader.ReadSingleBE();
                xur.Logger?.Here().Verbose("Read {0} float property value of {1}.", propertyDefinition.Name, val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUVector? TryReadVectorProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int vectIndex = reader.ReadInt32BE();
                xur.Logger?.Here()?.Verbose("Reading vector, got vector index of {0}", vectIndex);

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

        public static List<XUProperty>? TryReadObjectProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int compoundPropertyValuesCount = reader.ReadInt16BE();
                xur.Logger?.Here()?.Verbose("Got a compound properties value count of {0}.", compoundPropertyValuesCount);

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

                if(compoundClass == null)
                {
                    xur.Logger?.Here()?.Error("Compound class was null, the class lookup must have failed, returning null.");
                    return null;
                }

                byte hierarchicalPropertiesDepth = reader.ReadByte();
                xur.Logger?.Here()?.Verbose("Compound class has a hierarchical properties depth of {0:X8}.", hierarchicalPropertiesDepth);

                int propertyMasksCount = Math.Max((int)Math.Ceiling(compoundClass.PropertyDefinitions.Count / 8.0f), 1);
                xur.Logger?.Here().Verbose("Compound class has {0:X8} property definitions, will have {1:X8} mask(s).", compoundClass.PropertyDefinitions.Count, propertyMasksCount);

                byte[] propertyMasks = new byte[propertyMasksCount];
                for (int i = 0; i < propertyMasksCount; i++)
                {
                    byte readMask = reader.ReadByte();
                    propertyMasks[i] = readMask;
                    xur.Logger?.Here().Verbose("Read property mask {0:X8}.", readMask);
                }
                Array.Reverse(propertyMasks);

                List<XUProperty> compoundProperties = new List<XUProperty>();
                for (int i = 0; i < propertyMasksCount; i++)
                {
                    byte thisPropertyMask = propertyMasks[i];
                    xur.Logger?.Here().Verbose("Handling property mask {0:X8} for compound class {1}.", thisPropertyMask, compoundClass.Name);

                    if (thisPropertyMask == 0x00)
                    {
                        xur.Logger?.Here().Verbose("Property mask is 0, continuing.");
                        continue;
                    }

                    int propertyIndex = 0;
                    List<XUPropertyDefinition> thisMaskPropertyDefinitions = compoundClass.PropertyDefinitions.Skip(i * 8).Take(8).ToList();
                    foreach (XUPropertyDefinition maskedPropertyDefinition in thisMaskPropertyDefinitions)
                    {
                        int flag = 1 << propertyIndex;

                        if ((thisPropertyMask & flag) == flag)
                        {
                            xur.Logger?.Here().Verbose("Reading {0} property.", maskedPropertyDefinition.Name);
                            XUProperty? xuProperty = xur.TryReadProperty(reader, maskedPropertyDefinition);
                            if (xuProperty == null)
                            {
                                xur.Logger?.Here().Error("Failed to read {0} property, returning null.", maskedPropertyDefinition.Name);
                                return null;
                            }

                            compoundProperties.Add(xuProperty);
                        }

                        propertyIndex++;
                    }
                }

                int actualCompoundPropertyValuesCount = 0;
                foreach (XUProperty compoundProperty in compoundProperties)
                {
                    if(compoundProperty.Value is List<object> valList)
                    {
                        actualCompoundPropertyValuesCount += valList.Count;
                    }
                    else if(compoundProperty.Value is not null)
                    {
                        actualCompoundPropertyValuesCount++;
                    }
                }

                if (compoundPropertyValuesCount != actualCompoundPropertyValuesCount)
                {
                    xur.Logger?.Here()?.Error("Mismatch between compound properties value count. Expected: {0}, Actual: {1}, returning null.", compoundPropertyValuesCount, actualCompoundPropertyValuesCount);
                    return null;
                }

                return compoundProperties;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUColour? TryReadColourProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                byte a = reader.ReadByte();
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();

                XUColour colour = new XUColour(a, r, g, b);
                xur.Logger?.Here().Verbose("Read a colour, {0}.", colour);
                return colour;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUFigure? TryReadCustomProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int custOffset = reader.ReadInt32BE();
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

        public static XUQuaternion? TryReadQuaternionProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int quatIndex = reader.ReadInt32BE();
                xur.Logger?.Here()?.Verbose("Reading quaternion, got quaternion index of {0}", quatIndex);

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

        public static XUNamedFrame? TryReadNamedFrame(this XUR5 xur, BinaryReader reader)
        {
            try
            {
                short namedFrameStringIndex = (short)(reader.ReadInt16BE());
                xur.Logger?.Here().Verbose("Read named frame string index of {0:X8}.", namedFrameStringIndex);

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                if (strnSection.Strings.Count == 0 || strnSection.Strings.Count <= namedFrameStringIndex)
                {
                    xur.Logger?.Here().Error("Failed to read string as we got an invalid index of {0}. The strings length is {1}. Returning null.", namedFrameStringIndex, strnSection.Strings.Count);
                    return null;
                }

                if (namedFrameStringIndex < 0 || namedFrameStringIndex > strnSection.Strings.Count - 1)
                {
                    xur.Logger?.Here().Error("String index of {0:X8} is invalid, must be between 0 and {1}, returning null.", namedFrameStringIndex, strnSection.Strings.Count - 1);
                    return null;
                }

                string namedFrameName = strnSection.Strings[namedFrameStringIndex];
                xur.Logger?.Here().Verbose("Got a named frame of {0}.", namedFrameName);

                int namedFrameKeyframe = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Got a named frame keyframe of {0}.", namedFrameKeyframe);

                byte namedFrameCommandByte = reader.ReadByte();
                xur.Logger?.Here().Verbose("Got a named frame command byte of {0:X8}.", namedFrameCommandByte);

                if (!Enum.IsDefined(typeof(XUNamedFrameCommandTypes), (int)namedFrameCommandByte))
                {
                    xur.Logger?.Here().Error("Command byte of {0:X8} is not a valid command, returning null.", namedFrameCommandByte);
                    return null;
                }

                XUNamedFrameCommandTypes namedFrameCommandType = (XUNamedFrameCommandTypes)namedFrameCommandByte;
                xur.Logger?.Here().Verbose("Got a named frame command type of {0}.", namedFrameCommandType);

                short targetParameterStringIndex = (short)(reader.ReadInt16BE());
                xur.Logger?.Here().Verbose("Read target parameter string index of {0:X8}.", targetParameterStringIndex);

                if (targetParameterStringIndex < -1 || targetParameterStringIndex > strnSection.Strings.Count - 1)
                {
                    xur.Logger?.Here().Error("String index of {0:X8} is invalid, must be between 0 and {1}, returning null.", targetParameterStringIndex, strnSection.Strings.Count - 1);
                    return null;
                }

                if(targetParameterStringIndex == -1)
                {
                    //Not an error, only goto commands support parameters but XUR5 includes the string index as 0 anyway if non-goto
                    xur.Logger?.Here().Verbose("The command type {0} has a target parameter string index of 0, it must not support parameters.", namedFrameCommandType);
                    return new XUNamedFrame(namedFrameName, namedFrameKeyframe, namedFrameCommandType);
                }
                else
                {
                    string targetParameter = strnSection.Strings[targetParameterStringIndex];
                    xur.Logger?.Here().Verbose("Got a target parameter of {0}.", targetParameter);
                    return new XUNamedFrame(namedFrameName, namedFrameKeyframe, namedFrameCommandType, targetParameter);
                }
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static XUTimeline? TryReadTimeline(this XUR5 xur, BinaryReader reader, XUObject obj)
        {
            try
            {
                short objectNameStringIndex = (short)(reader.ReadInt16BE());
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

                int propertyDefinitionsCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Got a count of {0} property definitions.", propertyDefinitionsCount);

                int maxIndex = 0;
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
                        int index = reader.ReadInt32BE();
                        xur.Logger?.Here()?.Verbose("Read a timeline compound index value of {0}", index);
                        indexedPropertyIndexes.Add(index);

                        if(index > maxIndex)
                        {
                            maxIndex = index;
                        }
                    }
                }

                int keyframesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Timeline for {0} has {1} keyframes.", objectName, keyframesCount);
                List<XUKeyframe> keyframes = new List<XUKeyframe>();

                for (int i = 0; i < keyframesCount; i++)
                {
                    List<XUProperty> animatedProperties = new List<XUProperty>();
                    int handledIndexedProperties = 0;

                    int keyframe = reader.ReadInt32BE();
                    byte interpolationTypeByte = reader.ReadByte();
                    byte easeIn = reader.ReadByte();
                    byte easeOut = reader.ReadByte();
                    byte easeScale = reader.ReadByte();

                    if (!Enum.IsDefined(typeof(XUKeyframeInterpolationTypes), (int)interpolationTypeByte))
                    {
                        xur.Logger?.Here().Error("Interpolation type byte of {0:X8} is not a valid type, returning null.", interpolationTypeByte);
                        return null;
                    }

                    XUKeyframeInterpolationTypes interpolationType = (XUKeyframeInterpolationTypes)interpolationTypeByte;
                    xur.Logger?.Here().Verbose("Keyframe {0}: Interpolation Type {1}, Ease In: {2}, Ease Out: {3}, Scale {4}.", keyframe, interpolationTypeByte, easeIn, easeOut, easeScale);

                    foreach (XUPropertyDefinition animatedPropertyDefinition in animatedPropertyDefinitions)
                    {
                        xur.Logger?.Here().Verbose("Reading animated property {0}.", animatedPropertyDefinition.Name);
                        XUProperty? xuProperty = xur.TryReadProperty(reader, animatedPropertyDefinition, false);
                        if (xuProperty == null)
                        {
                            xur.Logger?.Here().Error("Failed to read {0} property, returning null.", animatedPropertyDefinition.Name);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Animated property {0} has a value of {1} at keyframe {2}.", animatedPropertyDefinition.Name, xuProperty.Value, keyframe);
                        if (animatedPropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            if(handledIndexedProperties < 0 || handledIndexedProperties >= indexedPropertyIndexes.Count)
                            {
                                xur.Logger?.Here().Error("Indexed properties index of {0:X8} is invalid, must be between 0 and {1}, returning null.", handledIndexedProperties, indexedPropertyIndexes.Count);
                                return null;
                            }

                            int indexToUse = indexedPropertyIndexes[handledIndexedProperties];
                            xur.Logger?.Here().Verbose("The property {0} is indexed, using index of {1}.", animatedPropertyDefinition.Name, indexToUse);

                            bool found = false;
                            foreach(XUProperty addedAnimatedProperty in animatedProperties)
                            {
                                if(addedAnimatedProperty.PropertyDefinition == xuProperty.PropertyDefinition)
                                {
                                    if(addedAnimatedProperty.Value is List<object> addedList)
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

                            if(found)
                            {
                                //Don't re-add the XUProperty again if we've just updated the existing one
                                continue;
                            }

                            List<object?> values = new List<object?>();
                            for(int j = 0; j <= maxIndex; j++)
                            {
                                values.Add(null);
                            }

                            values[indexToUse] = (xuProperty.Value as List<object>)[0];
                            animatedProperties.Add(new XUProperty(xuProperty.PropertyDefinition, values));
                            handledIndexedProperties++;
                        }
                        else
                        {
                            animatedProperties.Add(xuProperty);
                        }
                    }

                    XUKeyframe thisKeyframe = new XUKeyframe(keyframe, interpolationType, easeIn, easeOut, easeScale, animatedProperties);
                    keyframes.Add(thisKeyframe);
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
