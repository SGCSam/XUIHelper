using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using XUIHelper.Core.Extensions;
using static System.Net.Mime.MediaTypeNames;

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
                        throw new NotImplementedException();
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
                if(strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                short stringIndex = (short)(strnSection.Strings.IndexOf(stringVal) + 1);
                if(stringIndex == 0)
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

                if(val is not float floatVal)
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
                        parentClassPropertyDepth = 5;   //4 for XuiElement, 1 for XuiFigure
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
                foreach(XUProperty property in objectProperties)
                {
                    if(property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
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

                int maxCompoundPropertyDepth = 0;
                int compoundPropertyDefinitionIndex = 0;
                foreach(XUPropertyDefinition compoundPropertyDefinition in compoundClass.PropertyDefinitions)
                {
                    foreach (XUProperty compoundProperty in objectProperties)
                    {
                        if(compoundPropertyDefinition == compoundProperty.PropertyDefinition)
                        {
                            maxCompoundPropertyDepth = compoundPropertyDefinitionIndex + 1;
                            break;
                        }
                    }

                    compoundPropertyDefinitionIndex++;
                }

                int hierarchicalPropertiesDepth = (8 * objectProperties.Count) - parentClassPropertyDepth - Math.Max((int)Math.Ceiling(maxCompoundPropertyDepth / 8.0f), 1);
                xur.Logger?.Here().Verbose("Wrote hierarchical compound properties depth of {0:X8}.", hierarchicalPropertiesDepth);
                writer.Write((byte)hierarchicalPropertiesDepth);
                bytesWritten++;

                int propertyMasksCount = Math.Max((int)Math.Ceiling(compoundClass.PropertyDefinitions.Count / 8.0f), 1);
                xur.Logger?.Here().Verbose("Compound class has {0:X8} property definitions, will have {1:X8} mask(s).", compoundClass.PropertyDefinitions.Count, propertyMasksCount);

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
                        if(indexedPropertyValues == null) 
                        {
                            xur.Logger?.Here()?.Error("Indexed compound property value was not a list, returning null.");
                            return null;
                        }

                        writer.Write((byte)indexedPropertyValues.Count);
                        xur.Logger?.Here().Verbose("Wrote indexed property values count of {0:X8}.", indexedPropertyValues.Count);
                        bytesWritten++;

                        int indexCount = 0;
                        foreach(object indexedPropertyValue in indexedPropertyValues)
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
    }
}
