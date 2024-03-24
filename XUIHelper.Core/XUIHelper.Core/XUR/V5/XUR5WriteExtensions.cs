using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR5WriteExtensions
    {
        public static int? TryWriteProperty(this XUR5 xur, BinaryWriter writer, XUProperty property, object val)
        {
            try
            {
                int? bytesWritten = null;
                switch (property.PropertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        bytesWritten = TryWriteBoolProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        bytesWritten = TryWriteIntegerProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        bytesWritten = TryWriteUnsignedProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        bytesWritten = TryWriteStringProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        bytesWritten = TryWriteFloatProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        bytesWritten = TryWriteVectorProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        bytesWritten = TryWriteObjectProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        bytesWritten = TryWriteColourProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Custom:
                    {
                        bytesWritten = TryWriteCustomProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        bytesWritten = TryWriteQuaternionProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled property type of {0} when writing property {1}, returning null.", property.PropertyDefinition.Type, property.PropertyDefinition.Name);
                        return null;
                    }
                }

                if (bytesWritten == null)
                {
                    xur.Logger?.Here().Error("Bytes written was null when writing property {0}, an error must have occurred, returning null.", property.PropertyDefinition.Name);
                    return null;
                }

                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteBoolProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not boolean, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not bool boolVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as boolean had a non-boolean value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.Write(boolVal ? (byte)0x1 : (byte)0x0);
                xur.Logger?.Here().Verbose("Written {0} boolean property value of {1}.", propertyDefinition.Name, boolVal);
                return 1;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing boolean property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteIntegerProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not int intVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as integer had a non-integer value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.WriteInt32BE(intVal);
                xur.Logger?.Here().Verbose("Written {0} integer property value of {1}.", propertyDefinition.Name, intVal);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteUnsignedProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not uint unsignedVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as unsigned had a non-unsigned value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.WriteUInt32BE(unsignedVal);
                xur.Logger?.Here().Verbose("Written {0} unsigned property value of {1}.", propertyDefinition.Name, unsignedVal);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteStringProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not string stringVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as string had a non-string value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                short stringIndex = (short)(strnSection.Strings.IndexOf(stringVal));
                if (stringIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get string index for property {0} value {1}, returning null.", propertyDefinition.Name, stringVal);
                    return null;
                }

                writer.WriteInt16BE(stringIndex);
                xur.Logger?.Here().Verbose("Written {0} string property value of {1} as index {2}.", propertyDefinition.Name, stringVal, stringIndex);
                return 2;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteFloatProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not float floatVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as float had a non-float value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.WriteSingleBE(floatVal);
                xur.Logger?.Here().Verbose("Written {0} float property value of {1}.", propertyDefinition.Name, floatVal);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteVectorProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not XUVector vectorVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as vector had a non-vector value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                IVECTSection? vectSection = ((IXUR)xur).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
                if (vectSection == null)
                {
                    xur.Logger?.Here().Error("VECT section was null, returning null.");
                    return null;
                }

                int vectorIndex = vectSection.Vectors.IndexOf(vectorVal);
                if (vectorIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get vector index for property {0} value {1}, returning null.", propertyDefinition.Name, vectorVal);
                    return null;
                }

                writer.WriteInt32BE(vectorIndex);
                xur.Logger?.Here().Verbose("Written {0} vector property value of {1} as index {2}.", propertyDefinition.Name, vectorVal, vectorIndex);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteObjectProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not List<XUProperty> objectProperties)
                {
                    xur.Logger?.Here().Error("Property {0} marked as object had a non-object value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                XMLExtensionsManager? ext = XUIHelperCoreConstants.VersionedExtensions.GetValueOrDefault(0x5);
                if (ext == null)
                {
                    xur.Logger?.Here().Error("Failed to get extensions manager, returning null.");
                    return null;
                }

                int parentClassPropertyDepth = 0;
                XUClass? compoundClass = null;
                switch (propertyDefinition.Name)
                {
                    case "Fill":
                    {
                        xur.Logger?.Here()?.Verbose("Writing fill object.");
                        compoundClass = ext.TryGetClassByName("XuiFigureFill");
                        parentClassPropertyDepth = 5;   //4 for XuiElement, 1 for XuiFigure
                        break;
                    }

                    case "Gradient":
                    {
                        xur.Logger?.Here()?.Verbose("Writing gradient object.");
                        compoundClass = ext.TryGetClassByName("XuiFigureFillGradient");
                        parentClassPropertyDepth = 6;   //4 for XuiElement, 1 for XuiFigure, 1 for XuiFigureFill (XuiFigureFill is 1, not 2, since gradient is within the first 8 properties)
                        break;
                    }

                    case "Stroke":
                    {
                        xur.Logger?.Here()?.Verbose("Writing stroke object.");
                        compoundClass = ext.TryGetClassByName("XuiFigureStroke");
                        parentClassPropertyDepth = 6;   //4 for XuiElement, 1 for XuiFigure
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

                int bytesWritten = 0;

                short objectPropertiesCount = 0;
                foreach (XUProperty property in objectProperties)
                {
                    if (property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        short indexedPropertiesCount = (short)(property.Value as IList).Count;
                        xur.Logger?.Here().Verbose("Property {0} is indexed, incrementing count by list count of {1}.", property.PropertyDefinition.Name, indexedPropertiesCount);
                        objectPropertiesCount += indexedPropertiesCount;
                    }
                    else
                    {
                        xur.Logger?.Here().Verbose("Property {0} is not indexed, incrementing count by 1.", property.PropertyDefinition.Name);
                        objectPropertiesCount++;
                    }
                }

                xur.Logger?.Here().Verbose("Writing compound properties count of {0} for compound class {1}.", objectPropertiesCount, compoundClass.Name);
                writer.WriteInt16BE(objectPropertiesCount);
                bytesWritten += 2;

                int propertyMasksCount = Math.Max((int)Math.Ceiling(compoundClass.PropertyDefinitions.Count / 8.0f), 1);
                xur.Logger?.Here().Verbose("Compound class has {0:X8} property definitions, will have {1:X8} mask(s).", compoundClass.PropertyDefinitions.Count, propertyMasksCount);

                int packedByte = 0x0;
                packedByte |= (byte)propertyMasksCount;

                byte[] propertyMasks = new byte[propertyMasksCount];
                for (int i = 0; i < propertyMasksCount; i++)
                {
                    byte thisPropertyMask = 0x00;
                    List<XUPropertyDefinition> thisMaskPropertyDefinitions = compoundClass.PropertyDefinitions.Skip(i * 8).Take(8).ToList();

                    int propertyDefinitionIndex = 0;
                    foreach (XUPropertyDefinition compoundPropertyDefinition in thisMaskPropertyDefinitions)
                    {
                        foreach (XUProperty property in objectProperties)
                        {
                            if (compoundPropertyDefinition == property.PropertyDefinition)
                            {
                                thisPropertyMask |= (byte)(1 << propertyDefinitionIndex);
                                break;
                            }
                        }

                        propertyDefinitionIndex++;
                    }

                    xur.Logger?.Here().Verbose("Got a property mask of {0:X8} for mask index {1}.", thisPropertyMask, i);
                    propertyMasks[i] = thisPropertyMask;
                }

                packedByte |= (byte)(objectProperties.Count - 1 << 3);
                xur.Logger?.Here().Verbose("Writing packed byte of {0:X8} for class {1}.", packedByte, compoundClass.Name);
                writer.Write((byte)packedByte);
                bytesWritten++;

                Array.Reverse(propertyMasks);
                writer.Write(propertyMasks);
                bytesWritten += propertyMasks.Length;

                foreach (XUProperty property in objectProperties)
                {
                    xur.Logger?.Here().Verbose("Writing {0} compound property.", property.PropertyDefinition.Name);
                    if (property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        xur.Logger?.Here().Verbose("Compound property is indexed.");
                        List<object>? indexedPropertyValues = property.Value as List<object>;
                        if (indexedPropertyValues == null)
                        {
                            xur.Logger?.Here()?.Error("Indexed compound property value was not a list, returning null.");
                            return null;
                        }

                        writer.Write((byte)indexedPropertyValues.Count);
                        xur.Logger?.Here().Verbose("Wrote indexed property values count of {0:X8}.", indexedPropertyValues.Count);
                        bytesWritten++;

                        int indexCount = 0;
                        foreach (object indexedPropertyValue in indexedPropertyValues)
                        {
                            xur.Logger?.Here().Verbose("Writing {0} indexed compound property index {1}.", property.PropertyDefinition.Name, indexCount);
                            int? propertyBytesWritten = xur.TryWriteProperty(writer, property, indexedPropertyValue);
                            if (propertyBytesWritten == null)
                            {
                                xur.Logger?.Here().Error("Property bytes written was null for indexed compound property {0} at index {1}, returning null.", property.PropertyDefinition.Name, indexCount);
                                return null;
                            }

                            bytesWritten += propertyBytesWritten.Value;
                            indexCount++;
                        }
                    }
                    else
                    {
                        xur.Logger?.Here().Verbose("Compound property is not indexed.");
                        int? propertyBytesWritten = xur.TryWriteProperty(writer, property, property.Value);
                        if (propertyBytesWritten == null)
                        {
                            xur.Logger?.Here().Error("Property bytes written was null for compound property {0}, returning null.", property.PropertyDefinition.Name);
                            return null;
                        }

                        bytesWritten += propertyBytesWritten.Value;
                    }
                }

                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteColourProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not XUColour colourVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as colour had a non-colour value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.Write(colourVal.A);
                writer.Write(colourVal.R);
                writer.Write(colourVal.G);
                writer.Write(colourVal.B);
                xur.Logger?.Here().Verbose("Written {0} colour property value of {1}.", propertyDefinition.Name, colourVal);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteCustomProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not XUFigure figureVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as custom had a non-custom value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                ICUSTSection? custSection = ((IXUR)xur).TryFindXURSectionByMagic<ICUSTSection>(ICUSTSection.ExpectedMagic);
                if (custSection == null)
                {
                    xur.Logger?.Here().Error("CUST section was null, returning null.");
                    return null;
                }

                int custOffset = 0x00;
                foreach (XUFigure figure in custSection.Figures)
                {
                    if (figure == figureVal)
                    {
                        writer.WriteInt32BE(custOffset);
                        xur.Logger?.Here().Verbose("Written {0} custom property value of {1} as offset {2}.", propertyDefinition.Name, figureVal, custOffset);
                        return 4;
                    }
                    else
                    {
                        custOffset += (0x10 + (figure.Points.Count * 0x18));
                    }
                }

                xur.Logger?.Here().Error("Failed to find figure offset for writing custom property {0} with value {1}, returning null.", propertyDefinition.Name, figureVal);
                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteQuaternionProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not XUQuaternion quatVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as quaternion had a non-quaternion value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                IQUATSection? quatSection = ((IXUR)xur).TryFindXURSectionByMagic<IQUATSection>(IQUATSection.ExpectedMagic);
                if (quatSection == null)
                {
                    xur.Logger?.Here().Error("QUAT section was null, returning null.");
                    return null;
                }

                int quatIndex = quatSection.Quaternions.IndexOf(quatVal);
                if (quatIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get quaternion index for property {0} value {1}, returning null.", propertyDefinition.Name, quatVal);
                    return null;
                }

                writer.WriteInt32BE(quatIndex);
                xur.Logger?.Here().Verbose("Written {0} quaternion property value of {1} as index {2}.", propertyDefinition.Name, quatVal, quatIndex);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteNamedFrame(this XUR5 xur, BinaryWriter writer, XUNamedFrame namedFrame)
        {
            try
            {
                int bytesWritten = 0;

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                short nameIndex = (short)(strnSection.Strings.IndexOf(namedFrame.Name));
                if (nameIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get string index for named frame name {0}, returning null.", namedFrame.Name);
                    return null;
                }

                writer.WriteInt16BE(nameIndex);
                xur.Logger?.Here().Verbose("Written named frame name {0} as index {1}.", namedFrame.Name, nameIndex);
                bytesWritten += 2;

                writer.WriteInt32BE(namedFrame.Keyframe);
                xur.Logger?.Here().Verbose("Written named frame keyframe {0}.", namedFrame.Keyframe);
                bytesWritten += 4;

                writer.Write((byte)namedFrame.CommandType);
                xur.Logger?.Here().Verbose("Written named frame command type {0:X8}.", namedFrame.CommandType);
                bytesWritten++;

                if (string.IsNullOrEmpty(namedFrame.TargetParameter))
                {
                    writer.WriteInt16BE((short)0);
                    xur.Logger?.Here().Verbose("Target parameter isn't set, written index 0.");
                    bytesWritten += 2;
                    return bytesWritten;
                }
                else
                {
                    short parameterIndex = (short)(strnSection.Strings.IndexOf(namedFrame.TargetParameter));
                    if (parameterIndex == -1)
                    {
                        xur.Logger?.Here().Error("Failed to get string index for named frame parameter {0}, returning null.", namedFrame.TargetParameter);
                        return null;
                    }

                    writer.WriteInt16BE(parameterIndex);
                    xur.Logger?.Here().Verbose("Written named frame target parameter {0} as index {1}.", namedFrame.TargetParameter, parameterIndex);
                    bytesWritten += 2;
                    return bytesWritten;
                }
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static int? TryWriteTimeline(this XUR5 xur, BinaryWriter writer, XUObject parentObject, XUTimeline timeline)
        {
            try
            {
                int bytesWritten = 0;

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                short objectNameIndex = (short)(strnSection.Strings.IndexOf(timeline.ElementName));
                if (objectNameIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get string index for timeline object name {0}, returning null.", timeline.ElementName);
                    return null;
                }

                writer.WriteInt16BE(objectNameIndex);
                xur.Logger?.Here().Verbose("Written timeline object name {0} as index {1}.", timeline.ElementName, objectNameIndex);
                bytesWritten += 2;

                List<XUProperty> animatedProperties = timeline.Keyframes[0].Properties;
                short animatedPropertiesCount = 0;
                foreach (XUProperty property in animatedProperties)
                {
                    if (property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        short count = 0;
                        foreach (object? obj in property.Value as List<object?>)
                        { 
                            if(obj == null)
                            {
                                continue;
                            }

                            count++;
                        }

                        xur.Logger?.Here().Verbose("Animated property {0} is indexed, incrementing count by list count of {1}.", property.PropertyDefinition.Name, count);
                        animatedPropertiesCount += count;
                    }
                    else
                    {
                        xur.Logger?.Here().Verbose("Property {0} is not indexed, incrementing count by 1.", property.PropertyDefinition.Name);
                        animatedPropertiesCount++;
                    }
                }

                writer.WriteInt32BE(animatedPropertiesCount);
                xur.Logger?.Here().Verbose("Timeline has {0:X8} animated properties.", animatedPropertiesCount);
                bytesWritten += 4;

                XUObject? elementObject = parentObject.TryFindChildById(timeline.ElementName);
                if (elementObject == null)
                {
                    xur.Logger?.Here().Error("Failed to find object {0}, returning null.", timeline.ElementName);
                    return null;
                }

                List<XUClass>? classList = xur.ExtensionsManager?.TryGetClassHierarchy(elementObject.ClassName);
                if (classList == null)
                {
                    xur.Logger?.Here().Error(string.Format("Failed to get {0} class hierarchy, returning null.", elementObject.ClassName));
                    return null;
                }
                classList.Reverse();

                foreach (XUProperty animatedProperty in animatedProperties)
                {
                    int indexes = 1;
                    if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        List<object>? indexedPropertyValues = animatedProperty.Value as List<object>;
                        if (indexedPropertyValues == null)
                        {
                            xur.Logger?.Here()?.Error("Indexed property values for animated property {0} was not a list, returning null.", animatedProperty.PropertyDefinition.Name);
                            return null;
                        }

                        indexes = indexedPropertyValues.Count;
                    }

                    for(int i = 0; i < indexes; i++)
                    {
                        if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            List<object?>? indexedPropertyValues = animatedProperty.Value as List<object?>;
                            if (indexedPropertyValues == null)
                            {
                                xur.Logger?.Here()?.Error("Indexed property values for animated property {0} was not a list, returning null.", animatedProperty.PropertyDefinition.Name);
                                return null;
                            }

                            if (indexedPropertyValues[i] == null)
                            {
                                //This index isn't animated
                                continue;
                            }
                        }

                        byte packedByte = 0x00;
                        byte classDepth = 0;
                        if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureFillGradient")
                        {
                            xur.Logger?.Here().Verbose("Property {0} was part of gradient, setting class depth to 3.", animatedProperty.PropertyDefinition.Name);
                            classDepth = 0x03;
                        }
                        else if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureFill" || animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureStroke")
                        {
                            xur.Logger?.Here().Verbose("Property {0} was part of fill or stroke, setting class depth to 2.", animatedProperty.PropertyDefinition.Name);
                            classDepth = 0x02;
                        }
                        else
                        {
                            xur.Logger?.Here().Verbose("Property {0} was part of {1}, setting class depth to 1.", animatedProperty.PropertyDefinition.Name, animatedProperty.PropertyDefinition.ParentClassName);
                            classDepth = 0x01;
                        }

                        packedByte = classDepth;
                        if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            xur.Logger?.Here().Verbose("Property {0} is indexed, setting upper bit.", animatedProperty.PropertyDefinition.Name);
                            packedByte |= 1 << 7;
                        }

                        xur.Logger?.Here().Verbose("Writing packed byte of {0:X8}.", packedByte);
                        writer.Write(packedByte);
                        bytesWritten++;

                        XUClass? foundClass = null;
                        if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureFill")
                        {
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x01);
                            bytesWritten += 2;
                            foundClass = xur.ExtensionsManager?.TryGetClassByName(animatedProperty.PropertyDefinition.ParentClassName);
                        }
                        else if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureStroke")
                        {
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x01);
                            bytesWritten += 2;
                            foundClass = xur.ExtensionsManager?.TryGetClassByName(animatedProperty.PropertyDefinition.ParentClassName);
                        }
                        else if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureFillGradient")
                        {
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x01);
                            writer.Write((byte)0x03);
                            bytesWritten += 3;
                            foundClass = xur.ExtensionsManager?.TryGetClassByName(animatedProperty.PropertyDefinition.ParentClassName);
                        }
                        else
                        {
                            int? foundClassIndex = null;
                            int classIndex = 0;
                            foreach (XUClass xuClass in classList)
                            {
                                if (xuClass.Name == animatedProperty.PropertyDefinition.ParentClassName)
                                {
                                    foundClass = xuClass;
                                    foundClassIndex = classIndex;
                                    break;
                                }

                                classIndex++;
                            }

                            if (foundClassIndex == null)
                            {
                                xur.Logger?.Here().Error(string.Format("Failed to get class index for {0}, returning null.", animatedProperty.PropertyDefinition.ParentClassName));
                                return null;
                            }

                            xur.Logger?.Here().Verbose("Writing class index of {0:X8}.", foundClassIndex);
                            writer.Write((byte)foundClassIndex);
                            bytesWritten++;
                        }

                        if (foundClass == null)
                        {
                            xur.Logger?.Here().Error(string.Format("Failed to get class for {0}, returning null.", animatedProperty.PropertyDefinition.ParentClassName));
                            return null;
                        }

                        int? foundPropertyDefinitionIndex = null;
                        int propertyDefinitionIndex = 0;
                        foreach (XUPropertyDefinition propertyDefinition in foundClass.PropertyDefinitions)
                        {
                            if (propertyDefinition == animatedProperty.PropertyDefinition)
                            {
                                foundPropertyDefinitionIndex = propertyDefinitionIndex;
                                break;
                            }

                            propertyDefinitionIndex++;
                        }

                        if (foundPropertyDefinitionIndex == null)
                        {
                            xur.Logger?.Here().Error(string.Format("Failed to get property definition index for {0}, returning null.", animatedProperty.PropertyDefinition.Name));
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Writing property definition index of {0:X8}.", foundPropertyDefinitionIndex);
                        writer.Write((byte)foundPropertyDefinitionIndex);
                        bytesWritten++;

                        if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            xur.Logger?.Here().Verbose("Property {0} is indexed, writing index count of {1:X8}", animatedProperty.PropertyDefinition.Name, i);
                            writer.WriteInt32BE(i);
                            bytesWritten += 4;
                        }
                    }
                }

                writer.WriteInt32BE(timeline.Keyframes.Count);
                xur.Logger?.Here().Verbose("Timeline has {0:X8} keyframes.", timeline.Keyframes.Count);
                bytesWritten += 4;

                int keyframeIndex = 0;
                foreach (XUKeyframe keyframe in timeline.Keyframes)
                {
                    xur.Logger?.Here().Verbose("Writing keyframe index {0}.", keyframeIndex);

                    writer.WriteInt32BE(keyframe.Keyframe);
                    bytesWritten += 4;
                    xur.Logger?.Here().Verbose("Wrote keyframe {0}.", keyframe.Keyframe);

                    writer.Write((byte)keyframe.InterpolationType);
                    bytesWritten++;
                    xur.Logger?.Here().Verbose("Wrote interpolation type {0}.", keyframe.InterpolationType);

                    writer.Write((byte)keyframe.EaseIn);
                    bytesWritten++;
                    xur.Logger?.Here().Verbose("Wrote ease in of {0}.", keyframe.EaseIn);

                    writer.Write((byte)keyframe.EaseOut);
                    bytesWritten++;
                    xur.Logger?.Here().Verbose("Wrote ease out of {0}.", keyframe.EaseOut);

                    writer.Write((byte)keyframe.EaseScale);
                    bytesWritten++;
                    xur.Logger?.Here().Verbose("Wrote ease scale of {0}.", keyframe.EaseScale);

                    foreach (XUProperty animatedProperty in keyframe.Properties)
                    {
                        xur.Logger?.Here().Verbose("Writing {0} animated property.", animatedProperty.PropertyDefinition.Name);
                        if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            xur.Logger?.Here().Verbose("Animated property is indexed.");
                            List<object?>? indexedPropertyValues = animatedProperty.Value as List<object?>;
                            if (indexedPropertyValues == null)
                            {
                                xur.Logger?.Here()?.Error("Indexed animated property value was not a list, returning null.");
                                return null;
                            }

                            int indexCount = 0;
                            foreach (object? indexedPropertyValue in indexedPropertyValues)
                            {
                                if(indexedPropertyValue == null)
                                {
                                    //This index isn't animated
                                    continue;
                                }

                                xur.Logger?.Here().Verbose("Writing {0} indexed animated property index {1}.", animatedProperty.PropertyDefinition.Name, indexCount);
                                int? propertyBytesWritten = xur.TryWriteProperty(writer, animatedProperty, indexedPropertyValue);
                                if (propertyBytesWritten == null)
                                {
                                    xur.Logger?.Here().Error("Property bytes written was null for indexed animated property {0} at index {1}, returning null.", animatedProperty.PropertyDefinition.Name, indexCount);
                                    return null;
                                }

                                bytesWritten += propertyBytesWritten.Value;
                                indexCount++;
                            }
                        }
                        else
                        {
                            xur.Logger?.Here().Verbose("Animated property is not indexed.");
                            int? propertyBytesWritten = xur.TryWriteProperty(writer, animatedProperty, animatedProperty.Value);
                            if (propertyBytesWritten == null)
                            {
                                xur.Logger?.Here().Error("Property bytes written was null for animated property {0}, returning null.", animatedProperty.PropertyDefinition.Name);
                                return null;
                            }

                            bytesWritten += propertyBytesWritten.Value;
                        }
                    }

                    keyframeIndex++;
                }

                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
