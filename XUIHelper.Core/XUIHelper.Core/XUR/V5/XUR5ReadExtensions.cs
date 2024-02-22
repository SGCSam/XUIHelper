using Serilog;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR5ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                switch(propertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        return TryReadBoolProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Integer: 
                    {
                        return TryReadIntegerProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        return TryReadUnsignedProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        return TryReadStringProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        return TryReadFloatProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        return TryReadVectorProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        return TryReadObjectProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        return TryReadColourProperty(xur, reader, propertyDefinition);
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled property type of {0} when reading property {1}, returning null.", propertyDefinition.Type, propertyDefinition.Name);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadBoolProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
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
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadIntegerProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
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
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadUnsignedProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
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
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadStringProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int stringIndex = reader.ReadInt16BE() - 1;
                xur.Logger?.Here()?.Verbose("Reading string, got string index of {0}", stringIndex);

                if (stringIndex <= 0x00)
                {
                    //This is VALID, NOT a bug/hack. Timelines who have animated string properties use index 0 to indicate an empty string.
                    //We may animate the Visual property for example - 1 keyframe may have no visual, the next may have a different visual
                    //So we can use 0x00 index instead of storing a blank string in the STRN table

                    //We also use less than, since the zero based index method of -1 takes does 0 - 1, so we'll get a negative number

                    xur.Logger?.Here()?.Verbose("String index was 0, returning empty string.");
                    return new XUProperty(propertyDefinition, string.Empty);
                }

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
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadFloatProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
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
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadVectorProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
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
                    xur.Logger?.Here().Error("Failed to read vector as we got an invalid index of {0}. The vectors length is {1}. Returning null.", vectSection, vectSection.Vectors.Count);
                    return null;
                }

                XUVector val = vectSection.Vectors[vectIndex];
                xur.Logger?.Here().Verbose("Read vector value of {0}.", val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadObjectProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XMLExtensionsManager? ext = XUIHelperCoreConstants.VersionedExtensions.GetValueOrDefault(0x5);
                if (ext == null)
                {
                    xur.Logger?.Here().Error("Failed to get extensions manager, returning null.");
                    return null;
                }

                int compoundClassIndex = reader.ReadInt16BE() - 1;
                XUClass? compoundClass = null;
                switch (compoundClassIndex)
                {
                    case 0:
                    {
                        xur.Logger?.Here()?.Verbose("Reading object, got a compound class index of {0}, treating as fill.", compoundClassIndex);
                        compoundClass = ext.TryGetClassByName("XuiFigureFill");
                        break;
                    }
                    case 1:
                    {
                        xur.Logger?.Here()?.Verbose("Reading object, got a compound class index of {0}, treating as gradient.", compoundClassIndex);
                        compoundClass = ext.TryGetClassByName("XuiFigureFillGradient");
                        break;
                    }
                    case 2:
                    {
                        xur.Logger?.Here()?.Verbose("Reading object, got a compound class index of {0}, treating as stroke.", compoundClassIndex);
                        compoundClass = ext.TryGetClassByName("XuiFigureStroke");
                        break;
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled compound class index of {0}, returning null.", compoundClassIndex);
                        return null;
                    }
                }

                if(compoundClass == null)
                {
                    xur.Logger?.Here()?.Verbose("Compound class was null, the class lookup must have failed, returning null.");
                    return null;
                }

                byte propertiesCount = reader.ReadByte();
                xur.Logger?.Here()?.Verbose("Compound class has {0:X8} properties.", propertiesCount);

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

                            int indexCount = 1;
                            if (maskedPropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                            {
                                indexCount = (int)reader.ReadPackedULong();
                                xur.Logger?.Here().Verbose("The property {0} is indexed and has an index count of {1}.", maskedPropertyDefinition.Name, indexCount);
                            }

                            for (int currentIndex = 0; currentIndex < indexCount; currentIndex++)
                            {
                                XUProperty? xuProperty = xur.TryReadProperty(reader, maskedPropertyDefinition);
                                if (xuProperty == null)
                                {
                                    xur.Logger?.Here().Error("Failed to read {0} property, returning null.", maskedPropertyDefinition.Name);
                                    return null;
                                }

                                compoundProperties.Add(xuProperty);
                            }
                        }

                        propertyIndex++;
                    }
                }

                return new XUProperty(propertyDefinition, compoundProperties);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadColourProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
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
                xur.Logger?.Here().Error("Read a colour, {0}.", colour);
                return new XUProperty(propertyDefinition, colour);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUNamedFrame? TryReadNamedFrame(this XUR5 xur, BinaryReader reader)
        {
            try
            {
                short namedFrameStringIndex = (short)(reader.ReadInt16BE() - 1);
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

                short targetParameterStringIndex = (short)(reader.ReadInt16BE() - 1);
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
                short objectNameStringIndex = (short)(reader.ReadInt16BE() - 1);
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

                XMLExtensionsManager? ext = XUIHelperCoreConstants.VersionedExtensions.GetValueOrDefault(0x5);
                if (ext == null)
                {
                    xur.Logger?.Here().Error("Failed to get extensions manager, returning null.");
                    return null;
                }

                xur.Logger?.Here().Verbose("Found object has a class name of {0}.", elementObject.ClassName);
                List<XUClass>? classList = ext.TryGetClassHierarchy(elementObject.ClassName);
                if (classList == null)
                {
                    Log.Error(string.Format("Failed to get {0} class hierarchy, returning false.", elementObject.ClassName));
                    return null;
                }
                classList.Reverse();

                int propertyDefinitionsCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Got a count of {0} property definitions.", propertyDefinitionsCount);

                List<XUPropertyDefinition> animatedPropertyDefinitions = new List<XUPropertyDefinition>();

                for (int i = 0; i < propertyDefinitionsCount; i++)
                {
                    byte propertyDepth = reader.ReadByte();
                    xur.Logger?.Here().Verbose("Read a property depth of {0:X8}", propertyDepth);
                    byte classDepth = reader.ReadByte();
                    xur.Logger?.Here().Verbose("Read a class depth of {0:X8}", classDepth);
                    byte propertyIndex = reader.ReadByte();
                    xur.Logger?.Here().Verbose("Read a property index of {0:X8}", propertyIndex);

                    if(propertyDepth == 0x1)
                    {
                        if(classDepth < 0 || classDepth > classList.Count - 1)
                        {
                            xur.Logger?.Here().Error("Class depth of {0:X8} is invalid, must be between 0 and {1}, returning null.", classDepth, classList.Count - 1);
                            return null;
                        }

                        XUClass classAtDepth = classList[classDepth];
                        if (propertyIndex < 0 || propertyIndex > classAtDepth.PropertyDefinitions.Count)
                        {
                            xur.Logger?.Here().Error("Property index of {0:X8} is invalid, must be between 0 and {1}, returning null.", propertyIndex, classAtDepth.PropertyDefinitions.Count);
                            return null;
                        }

                        XUPropertyDefinition thisPropDef = classAtDepth.PropertyDefinitions[propertyIndex];
                        xur.Logger?.Here().Verbose("Property {0} of {1} is animated.", thisPropDef.Name, classAtDepth.Name);
                        animatedPropertyDefinitions.Add(thisPropDef);
                    }
                    else
                    {
                        xur.Logger?.Here().Error("Unimplemented.");
                        return null;
                    }
                }

                int keyframesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Timeline for {0} has {1} keyframes.", objectName, keyframesCount);
                List<XUKeyframe> keyframes = new List<XUKeyframe>();

                for (int i = 0; i < keyframesCount; i++)
                {
                    List<XUProperty> animatedProperties = new List<XUProperty>();
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

                    foreach(XUPropertyDefinition animatedPropertyDefinition in animatedPropertyDefinitions)
                    {
                        xur.Logger?.Here().Verbose("Reading animated property {0}.", animatedPropertyDefinition.Name);
                        XUProperty? xuProperty = xur.TryReadProperty(reader, animatedPropertyDefinition);
                        if (xuProperty == null)
                        {
                            xur.Logger?.Here().Error("Failed to read {0} property, returning null.", animatedPropertyDefinition.Name);
                            return null;
                        }

                        xur.Logger?.Here().Verbose("Animated property {0} has a value of {1} at keyframe {2}.", animatedPropertyDefinition.Name, xuProperty.Value, keyframe);
                        animatedProperties.Add(xuProperty);
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
