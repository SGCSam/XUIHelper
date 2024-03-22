﻿using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR8WriteExtensions
    {
        public static int? TryWriteProperty(this XUR8 xur, BinaryWriter writer, XUProperty property, object val)
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

        public static int? TryWriteBoolProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
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

        public static int? TryWriteIntegerProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteUnsignedProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
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

                int unsignedBytesWritten = 0;
                writer.WritePackedUInt(unsignedVal, out unsignedBytesWritten);
                xur.Logger?.Here().Verbose("Written {0} unsigned property value of {1}, {2} bytes.", propertyDefinition.Name, unsignedVal, unsignedBytesWritten);
                return unsignedBytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteStringProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
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

                int stringIndex = strnSection.Strings.IndexOf(stringVal);
                if (stringIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get string index for {0} with value {1}, returning null.", propertyDefinition.Name, stringVal);
                    return null;
                }

                int stringIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)stringIndex, out stringIndexBytesWritten);
                xur.Logger?.Here().Verbose("Written {0} string property value of {1} as index {2}, {3} bytes.", propertyDefinition.Name, stringVal, stringIndex, stringIndexBytesWritten);
                return stringIndexBytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteFloatProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
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

                IFLOTSection? flotSection = ((IXUR)xur).TryFindXURSectionByMagic<IFLOTSection>(IFLOTSection.ExpectedMagic);
                if (flotSection == null)
                {
                    xur.Logger?.Here().Error("FLOT section was null, returning null.");
                    return null;
                }

                int floatIndex = flotSection.Floats.IndexOf(floatVal);
                if (floatIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get float index for {0} with value {1}, returning null.", propertyDefinition.Name, floatVal);
                    return null;
                }

                int floatIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)floatIndex, out floatIndexBytesWritten);
                xur.Logger?.Here().Verbose("Written {0} float property value of {1} as index {2}, {3} bytes.", propertyDefinition.Name, floatVal, floatIndex, floatIndexBytesWritten);
                return floatIndexBytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteVectorProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not XUVector vectVal)
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

                int vectIndex = vectSection.Vectors.IndexOf(vectVal);
                if (vectIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get vector index for {0} with value {1}, returning null.", propertyDefinition.Name, vectVal);
                    return null;
                }

                int vectIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)vectIndex, out vectIndexBytesWritten);
                xur.Logger?.Here().Verbose("Written {0} vector property value of {1} as index {2}, {3} bytes.", propertyDefinition.Name, vectVal, vectIndex, vectIndexBytesWritten);
                return vectIndexBytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteObjectProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteColourProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteCustomProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteQuaternionProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteNamedFrames(this XUR8 xur, BinaryWriter writer, List<XUNamedFrame> namedFrames)
        {
            try
            {
                INAMESection? nameSection = ((IXUR)xur).TryFindXURSectionByMagic<INAMESection>(INAMESection.ExpectedMagic);
                if (nameSection == null)
                {
                    xur.Logger?.Here().Error("NAME section was null, returning null.");
                    return null;
                }

                int baseIndex = namedFrames.GetSequenceIndex(namedFrames);
                if (baseIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get base index for named frames, returning null.");
                    return null;
                }

                int baseIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)baseIndex, out baseIndexBytesWritten);
                xur.Logger?.Here().Verbose("Written named frames as base index {0}, {1} bytes.", baseIndex, baseIndexBytesWritten);
                return baseIndexBytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static int? TryWriteTimeline(this XUR8 xur, BinaryWriter writer, XUObject parentObject, XUTimeline timeline)
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

                short objectNameIndex = (short)strnSection.Strings.IndexOf(timeline.ElementName);
                if (objectNameIndex == 0)
                {
                    xur.Logger?.Here().Error("Failed to get string index for timeline object name {0}, returning null.", timeline.ElementName);
                    return null;
                }

                int objectNameIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)objectNameIndex, out objectNameIndexBytesWritten);
                xur.Logger?.Here().Verbose("Written timeline object name {0} as index {1}, {2} bytes.", timeline.ElementName, objectNameIndex, objectNameIndexBytesWritten);
                bytesWritten += objectNameIndexBytesWritten;

                List<XUProperty> animatedProperties = timeline.Keyframes[0].Properties;
                uint animatedPropertiesCount = 0;
                foreach (XUProperty property in animatedProperties)
                {
                    if (property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        uint count = 0;
                        foreach (object? obj in property.Value as List<object?>)
                        {
                            if (obj == null)
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

                int animatedPropertiesCountBytesWritten = 0;
                writer.WritePackedUInt(animatedPropertiesCount, out animatedPropertiesCountBytesWritten);
                xur.Logger?.Here().Verbose("Written animated properties count of {0}, {1} bytes.", animatedPropertiesCount, animatedPropertiesCountBytesWritten);
                bytesWritten += animatedPropertiesCountBytesWritten;

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

                    for (int i = 0; i < indexes; i++)
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
                            int indexBytesWritten = 0;
                            writer.WritePackedUInt((uint)i, out indexBytesWritten);
                            xur.Logger?.Here().Verbose("Property {0} is indexed, wrote index count of {1:X8}, {2} bytes", animatedProperty.PropertyDefinition.Name, i, indexBytesWritten);
                            bytesWritten += indexBytesWritten;
                        }
                    }
                }

                int keyframeCountBytesWritten = 0;
                writer.WritePackedUInt((uint)timeline.Keyframes.Count, out keyframeCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote timeline keyframes count of {0:X8}, {1} bytes", timeline.Keyframes.Count, keyframeCountBytesWritten);
                bytesWritten += keyframeCountBytesWritten;

                IKEYDSection? keydSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYDSection>(IKEYDSection.ExpectedMagic);
                if (keydSection == null)
                {
                    xur.Logger?.Here().Error("KEYD section was null, returning null.");
                    return null;
                }

                int? baseIndex = ((KEYD8Section)keydSection).TryGetBaseIndexForTimelineKeyframes(timeline, xur.Logger);
                if (baseIndex == null)
                {
                    xur.Logger?.Here().Error("Base index was null, returning null.");
                    return null;
                }

                int baseIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)baseIndex, out baseIndexBytesWritten);
                xur.Logger?.Here().Verbose("Wrote keyframes base index of {0:X8}, {1} bytes", baseIndex, baseIndexBytesWritten);
                bytesWritten += baseIndexBytesWritten;

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
