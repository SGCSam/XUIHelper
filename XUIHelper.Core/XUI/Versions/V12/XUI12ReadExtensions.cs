using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using XUIHelper.Core.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace XUIHelper.Core
{
    public static class XUR12ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                object? value = null;
                switch (propertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        value = TryReadBoolProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        value = TryReadIntegerProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        value = TryReadUnsignedProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        value = TryReadStringProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        value = TryReadFloatProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        value = TryReadVectorProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        value = TryReadObjectProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        value = TryReadColourProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Custom:
                    {
                        value = TryReadCustomProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        value = TryReadQuaternionProperty(xui, propertyDefinition, element);
                        break;
                    }
                    default:
                    {
                        xui.Logger?.Here().Error("Unhandled property type of {0} when reading property {1}, returning null.", propertyDefinition.Type, propertyDefinition.Name);
                        return null;
                    }
                }

                if (value == null)
                {
                    xui.Logger?.Here().Error("Read value was null when reading property {0}, an error must have occurred, returning null.", propertyDefinition.Name);
                    return null;
                }

                return new XUProperty(propertyDefinition, value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static bool? TryReadBoolProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for bool property {1}.", element.Value, propertyDefinition.Name);
                switch(element.Value.ToLower())
                {
                    case "true":
                    {
                        return true;
                    }
                    case "false": 
                    {
                        return false;
                    }
                    default:
                    {
                        xui.Logger?.Here().Error("Unhandled case for value {0} when reading bool property {1}, returning null.", element.Value.ToLower(), propertyDefinition.Name);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryReadIntegerProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for integer property {1}.", element.Value, propertyDefinition.Name);
                return Convert.ToInt32(element.Value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static uint? TryReadUnsignedProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for unsigned property {1}.", element.Value, propertyDefinition.Name);
                return Convert.ToUInt32(element.Value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static string? TryReadStringProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for string property {1}.", element.Value, propertyDefinition.Name);
                return Convert.ToString(element.Value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static float? TryReadFloatProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for float property {1}.", element.Value, propertyDefinition.Name);
                return Convert.ToSingle(element.Value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUVector? TryReadVectorProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for vector property {1}.", element.Value, propertyDefinition.Name);
                string[] vectComponents = element.Value.Split(',');
                if(vectComponents.Length != 3)
                {
                    xui.Logger?.Here().Error("Failed to read vector property as we received an unexpected number of components, returning false. Expected: 3, Actual: {0}", vectComponents.Length); ;
                    return null;
                }

                float x = Convert.ToSingle(vectComponents[0]);
                float y = Convert.ToSingle(vectComponents[1]);
                float z = Convert.ToSingle(vectComponents[2]);
                return new XUVector(x, y, z);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static List<XUProperty>? TryReadObjectProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for object property {1}.", element.Value, propertyDefinition.Name);

                XUClass? compoundClass = null;
                switch (propertyDefinition.Name)
                {
                    case "Fill":
                    {
                        xui.Logger?.Here()?.Verbose("Reading fill object.");
                        compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureFill");
                        break;
                    }

                    case "Gradient":
                    {
                        xui.Logger?.Here()?.Verbose("Reading gradient object.");
                        compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureFillGradient");
                        break;
                    }

                    case "Stroke":
                    {
                        xui.Logger?.Here()?.Verbose("Reading stroke object.");
                        compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureStroke");
                        break;
                    }
                }

                if(compoundClass == null)
                {
                    xui.Logger?.Here().Error("Unhandled compound class of {0}, returning null.", propertyDefinition.Name);
                    return null;
                }

                XElement? parentPropertiesElement = element.Descendants("Properties").FirstOrDefault();
                if (parentPropertiesElement == null)
                {
                    xui.Logger?.Here().Error("Failed to find properties element, returning null.");
                    return null;
                }

                IEnumerable<XElement> childPropertyElements = parentPropertiesElement.Elements();
                xui.Logger?.Here().Verbose("Class {0} has {1} properties.", compoundClass.Name, childPropertyElements.Count());

                List<XUProperty> properties = new List<XUProperty>();
                foreach (XElement propertyElement in childPropertyElements)
                {
                    string propertyName = propertyElement.Name.ToString();
                    xui.Logger?.Here().Verbose("Handling property {0}", propertyName);

                    XUPropertyDefinition? compoundPropertyDefinition = compoundClass.PropertyDefinitions.Where(x => x.Name == propertyName).FirstOrDefault();
                    if (compoundPropertyDefinition == null)
                    {
                        xui.Logger?.Here().Error("Failed to find property definition for {0}, returning null.", propertyName);
                        return null;
                    }

                    xui.Logger?.Here().Verbose("Found property definition successfully in {0}, reading property...", compoundClass.Name);
                    XUProperty? readProperty = TryReadProperty(xui, compoundPropertyDefinition, propertyElement);
                    if (readProperty == null)
                    {
                        xui.Logger?.Here().Error("Read property was null, an error must have occurred, returning null.");
                        return null;
                    }
                    else if(compoundPropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        XUProperty? foundProperty = properties.Where(x => x.PropertyDefinition == compoundPropertyDefinition).FirstOrDefault();
                        if (foundProperty != null)
                        {
                            ((IList)foundProperty.Value).Add(readProperty.Value);
                        }
                        else
                        {
                            XUProperty indexedProperty = new XUProperty(compoundPropertyDefinition, new List<object>() { readProperty.Value });
                            properties.Add(indexedProperty);
                        }
                    }
                    else
                    {
                        properties.Add(readProperty);
                    }
                }

                return properties;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUColour? TryReadColourProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for colour property {1}.", element.Value, propertyDefinition.Name);
                System.Drawing.Color colour = System.Drawing.ColorTranslator.FromHtml(element.Value);
                return new XUColour(colour.A, colour.R, colour.G, colour.B);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUFigure? TryReadCustomProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for custom property {1}.", element.Value, propertyDefinition.Name);
                string[] points = element.Value.Split(",");
                if(points.Length <= 0) 
                {
                    xui.Logger?.Here().Error("Failed to get any points, returning null.");
                    return null;
                }

                int bezierPointsCount = Convert.ToInt32(points[0]);
                int expectedPointsLength = (6 * bezierPointsCount) + (bezierPointsCount + 2);    //3 XY points per point, plus each index, the initial point count itself and the trailing empty string
                xui.Logger?.Here().Verbose("Got a bezier points count of {0}, verifying...", points);
                if(points.Length != expectedPointsLength)
                {
                    xui.Logger?.Here().Error("Mismatch of points count, returning null. Expected: {0}, Actual: {1}", expectedPointsLength, points);
                    return null;
                }

                float largestX = float.MinValue;
                float largestY = float.MinValue;

                List<XUBezierPoint> bezierPoints = new List<XUBezierPoint>();
                for(int bezierPointIndex = 0; bezierPointIndex < bezierPointsCount; bezierPointIndex++)
                {
                    int stringIndex = (bezierPointIndex * 6) + (bezierPointIndex * 1) + 1;
                    xui.Logger?.Here().Verbose("Reading bezier point index {0}, got string index {1}.", bezierPointIndex, stringIndex);

                    float refX = Convert.ToSingle(points[stringIndex]);
                    float refY = Convert.ToSingle(points[stringIndex + 1]);
                    XUPoint bezierReferencePoint = new XUPoint(refX, refY);
                    xui.Logger?.Here().Verbose("Got a bezier reference point of {0}.", bezierReferencePoint);

                    float controlPointOneX = Convert.ToSingle(points[stringIndex + 2]);
                    float controlPointOneY = Convert.ToSingle(points[stringIndex + 3]);
                    XUPoint controlPointOne = new XUPoint(controlPointOneX, controlPointOneY);
                    xui.Logger?.Here().Verbose("Got control point one of {0}.", controlPointOne);

                    float controlPointTwoX = Convert.ToSingle(points[stringIndex + 4]);
                    float controlPointTwoY = Convert.ToSingle(points[stringIndex + 5]);
                    XUPoint controlPointTwo = new XUPoint(controlPointTwoX, controlPointTwoY);
                    xui.Logger?.Here().Verbose("Got control point two of {0}.", controlPointTwo);

                    bezierPoints.Add(new XUBezierPoint(bezierReferencePoint, controlPointOne, controlPointTwo));

                    //TODO: I'm far from sure that this is the correct logic for obtaining the bounding box...
                    if(refX > largestX)
                    {
                        largestX = refX;
                    }
                    if (controlPointOneX > largestX)
                    {
                        largestX = controlPointOneX;
                    }
                    if (controlPointTwoX > largestX)
                    {
                        largestX = controlPointTwoX;
                    }

                    if (refY > largestY)
                    {
                        largestY = refY;
                    }
                    if (controlPointOneY > largestY)
                    {
                        largestY = controlPointOneY;
                    }
                    if (controlPointTwoY > largestY)
                    {
                        largestY = controlPointTwoY;
                    }
                }

                if(bezierPoints.Count != bezierPointsCount)
                {
                    xui.Logger?.Here().Error("Mismatch of bezier points count, returning null. Expected: {0}, Actual: {1}", bezierPointsCount, bezierPoints.Count);
                    return null;
                }

                return new XUFigure(new XUPoint(largestX, largestY), bezierPoints);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUQuaternion? TryReadQuaternionProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for quaternion property {1}.", element.Value, propertyDefinition.Name);
                string[] quatComponents = element.Value.Split(',');
                if (quatComponents.Length != 4)
                {
                    xui.Logger?.Here().Error("Failed to read quaternion property as we received an unexpected number of components, returning false. Expected: 4, Actual: {0}", quatComponents.Length);
                    return null;
                }

                float x = Convert.ToSingle(quatComponents[0]);
                float y = Convert.ToSingle(quatComponents[1]);
                float z = Convert.ToSingle(quatComponents[2]);
                float w = Convert.ToSingle(quatComponents[3]);
                return new XUQuaternion(x, y, z, w);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUNamedFrame? TryReadNamedFrame(this XUI12 xui, XElement element)
        {
            try
            {
                string? name = element.Element("Name")?.Value;
                if(name == null)
                {
                    xui.Logger?.Here().Error("Name of named frame was null, returning null.");
                    return null;
                }
                xui.Logger?.Here().Verbose("Read a named frame name of {0}.", name);

                string? keyframeString = element.Element("Time")?.Value;
                if (keyframeString == null)
                {
                    xui.Logger?.Here().Error("Keyframe of named frame was null, returning null.");
                    return null;
                }
                xui.Logger?.Here().Verbose("Read a keyframe of {0}.", keyframeString);
                int keyframe = Convert.ToInt32(keyframeString);

                string? commandString = element.Element("Command")?.Value;
                if(commandString == null)
                {
                    xui.Logger?.Here().Verbose("There is no command string, treating as play.");
                    return new XUNamedFrame(name, keyframe, XUNamedFrameCommandTypes.Play);
                }

                XUNamedFrameCommandTypes? commandType = null;
                switch(commandString.ToLower())
                {
                    case "stop":
                    {
                        xui.Logger?.Here().Verbose("Returning keyframe as stop.");
                        return new XUNamedFrame(name, keyframe, XUNamedFrameCommandTypes.Stop);
                    }
                    case "goto":
                    {
                        xui.Logger?.Here().Verbose("Got a command type of Go To.");
                        commandType = XUNamedFrameCommandTypes.GoTo;
                        break;
                    }
                    case "gotoandplay":
                    {
                        xui.Logger?.Here().Verbose("Got a command type of Go To and Play.");
                        commandType = XUNamedFrameCommandTypes.GoToAndPlay;
                        break;
                    }
                    case "gotoandstop":
                    {
                        xui.Logger?.Here().Verbose("Got a command type of Go To and Stop.");
                        commandType = XUNamedFrameCommandTypes.GoToAndStop;
                        break;
                    }
                    default:
                    {
                        xui.Logger?.Here().Error("Unhandled command type of {0}, returning null.", commandString.ToLower());
                        return null;
                    }
                }

                if (commandType == null)
                {
                    xui.Logger?.Here().Error("Command type is null, returning null.");
                    return null;
                }

                string? target = element.Element("CommandParams")?.Value;
                if (target == null)
                {
                    xui.Logger?.Here().Error("Target is null, returning null.");
                    return null;
                }

                return new XUNamedFrame(name, keyframe, commandType.Value, target);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static XUTimeline? TryReadTimeline(this XUI12 xui, XElement element, XUObject obj)
        {
            try
            {
                string? id = element.Element("Id")?.Value;
                if (id == null)
                {
                    xui.Logger?.Here().Error("Failed to ID property, returning null.");
                    return null;
                }

                XUObject? animatedObject = obj.TryFindChildById(id);
                if (animatedObject == null)
                {
                    xui.Logger?.Here().Error("Failed to find animated object {0}, returning null.", id);
                    return null;
                }

                XUClass? animatedObjectClass = XMLExtensionsManager.TryGetClassByName(animatedObject.ClassName);
                if (animatedObjectClass == null)
                {
                    xui.Logger?.Here().Error("Failed to find animated object class {0}, returning null.", animatedObject.ClassName);
                    return null;
                }

                List<XUPropertyDefinition> animatedPropertyDefinitions = new List<XUPropertyDefinition>();
                IEnumerable<XElement> timelinePropertyElements = element.Elements("TimelineProp");
                foreach (XElement timelinePropertyElement in timelinePropertyElements)
                {
                    string propertyHierarchy = timelinePropertyElement.Value;
                    List<string> propertyNames = propertyHierarchy.Split('.').ToList();

                    Tuple<XUClass, XUPropertyDefinition?>? classPropertyTuple = new Tuple<XUClass, XUPropertyDefinition?>(animatedObjectClass, null);
                    XUClass propertyClass = animatedObjectClass;

                    int propertyNameIndex = 0;
                    foreach(string propertyName in propertyNames)
                    {
                        classPropertyTuple = GetPropertyDefinitionFromClass(xui, propertyName, propertyClass);
                        propertyNameIndex++;
                        if (classPropertyTuple == null)
                        {
                            xui.Logger?.Here().Error("Failed to get property definition for {0} from {1}, returning null.", propertyName, animatedObjectClass.Name);
                            return null;
                        }

                        if (classPropertyTuple.Item2 == null)
                        {
                            xui.Logger?.Here().Error("Property definition was null from {0}, returning null.", classPropertyTuple.Item1.Name);
                            return null;
                        }

                        //Bail here since we've found it and we don't go into the compound classes again
                        if(propertyNameIndex == propertyNames.Count)
                        {
                            break;
                        }

                        if(propertyNames.Count > 1)
                        {
                            XUClass? compoundClass = null;
                            switch (classPropertyTuple.Item2.Name)
                            {
                                case "Fill":
                                {
                                    xui.Logger?.Here().Verbose("Property {0} is compound, handling {1}, treating as fill.", propertyHierarchy, propertyName);
                                    compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureFill");
                                    break;
                                }

                                case "Gradient":
                                {
                                    xui.Logger?.Here().Verbose("Property {0} is compound, handling {1}, treating as gradient.", propertyHierarchy, propertyName);
                                    compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureFillGradient");
                                    break;
                                }

                                case "Stroke":
                                {
                                    xui.Logger?.Here().Verbose("Property {0} is compound, handling {1}, treating as stroke.", propertyHierarchy, propertyName);
                                    compoundClass = XMLExtensionsManager.TryGetClassByName("XuiFigureStroke");
                                    break;
                                }
                            }

                            if (compoundClass == null)
                            {
                                xui.Logger?.Here().Error("Compound class is null, returning null.");
                                return null;
                            }

                            propertyClass = compoundClass;
                        }
                    }

                    if (classPropertyTuple.Item2 == null)
                    {
                        xui.Logger?.Here().Error("Property definition was null from {0}, returning null.", classPropertyTuple.Item1.Name);
                        return null;
                    }

                    animatedPropertyDefinitions.Add(classPropertyTuple.Item2);
                }

                if(timelinePropertyElements.Count() != animatedPropertyDefinitions.Count)
                {
                    xui.Logger?.Here().Error("Mismatch between property elements and definitions, returning null. Expected: {0}, Actual: {1}", timelinePropertyElements.Count(), animatedPropertyDefinitions.Count);
                    return null;
                }

                List<XUKeyframe> keyframes = new List<XUKeyframe>();
                IEnumerable<XElement> keyframeElements = element.Elements("KeyFrame");
                foreach (XElement keyframeElement in keyframeElements)
                {
                    string? keyframeString = keyframeElement.Element("Time")?.Value;
                    if (keyframeString == null)
                    {
                        xui.Logger?.Here().Error("Failed to find keyframe, returning null.");
                        return null;
                    }
                    xui.Logger?.Here().Verbose("Got a keyframe of {0}.", keyframeString);
                    int keyframe = Convert.ToInt32(keyframeString);

                    string? interpolationString = keyframeElement.Element("Interpolation")?.Value;
                    if (interpolationString == null)
                    {
                        xui.Logger?.Here().Error("Failed to find interpolation, returning null.");
                        return null;
                    }
                    xui.Logger?.Here().Verbose("Got interpolation of {0}.", interpolationString);
                    XUKeyframeInterpolationTypes interpolationType = (XUKeyframeInterpolationTypes)(Convert.ToInt32(interpolationString));

                    byte easeIn = 0;
                    byte easeOut = 0;
                    byte easeScale = 50;

                    if(interpolationType == XUKeyframeInterpolationTypes.Ease)
                    {
                        string? easeInString = keyframeElement.Element("EaseIn")?.Value;
                        if (easeInString == null)
                        {
                            xui.Logger?.Here().Error("Failed to find ease in, returning null.");
                            return null;
                        }
                        xui.Logger?.Here().Verbose("Got ease in of {0}.", easeInString);

                        string? easeOutString = keyframeElement.Element("EaseOut")?.Value;
                        if (easeOutString == null)
                        {
                            xui.Logger?.Here().Error("Failed to find ease out, returning null.");
                            return null;
                        }
                        xui.Logger?.Here().Verbose("Got ease out of {0}.", easeOutString);

                        string? easeScaleString = keyframeElement.Element("EaseScale")?.Value;
                        if (easeScaleString == null)
                        {
                            xui.Logger?.Here().Error("Failed to find ease scale, returning null.");
                            return null;
                        }
                        xui.Logger?.Here().Verbose("Got ease scale of {0}.", easeScaleString);

                        easeIn = (byte)Convert.ToInt32(easeInString);
                        easeOut = (byte)Convert.ToInt32(easeOutString);
                        easeScale = (byte)Convert.ToInt32(easeScaleString);
                    }

                    List<XElement> animatedPropertyValueElements = keyframeElement.Elements("Prop").ToList();
                    if (animatedPropertyValueElements.Count != animatedPropertyDefinitions.Count)
                    {
                        xui.Logger?.Here().Error("Mismatch between property value elements and definitions, returning null. Expected: {0}, Actual: {1}", animatedPropertyValueElements.Count, animatedPropertyDefinitions.Count);
                        return null;
                    }

                    List<XUProperty> animatedProperties = new List<XUProperty>();
                    for(int propertyIndex = 0; propertyIndex < animatedPropertyDefinitions.Count; propertyIndex++) 
                    { 
                        XUPropertyDefinition thisAnimatedPropertyDefinition = animatedPropertyDefinitions[propertyIndex];
                        XElement thisAnimatedPropertyValueElement = animatedPropertyValueElements[propertyIndex];

                        xui.Logger?.Here().Verbose("Reading animated property {0}.", thisAnimatedPropertyDefinition.Name);
                        XUProperty? animatedProperty = TryReadProperty(xui, thisAnimatedPropertyDefinition, thisAnimatedPropertyValueElement);
                        if (animatedProperty == null)
                        {
                            xui.Logger?.Here().Error("Read animated property was null, an error must have occurred, returning null.");
                            return null;
                        }

                        if(thisAnimatedPropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            bool found = false;
                            foreach(XUProperty addedAnimatedProperty in animatedProperties)
                            {
                                if(addedAnimatedProperty.PropertyDefinition.Name == thisAnimatedPropertyDefinition.Name)
                                {
                                    (addedAnimatedProperty.Value as List<object>).Add(animatedProperty.Value);
                                    found = true;
                                    break;
                                }
                            }

                            if(!found)
                            {
                                animatedProperties.Add(new XUProperty(animatedProperty.PropertyDefinition, new List<object>() { animatedProperty.Value }));
                            }
                        }
                        else
                        {
                            animatedProperties.Add(animatedProperty);
                        }
                    }

                    keyframes.Add(new XUKeyframe(keyframe, interpolationType, easeIn, easeOut, easeScale, animatedProperties));
                }

                return new XUTimeline(id, keyframes);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        private static Tuple<XUClass, XUPropertyDefinition?>? GetPropertyDefinitionFromClass(XUI12 xui, string propertyName, XUClass propertyClass)
        {
            xui.Logger?.Here().Verbose("Attempting to find property definition for {0}", propertyName);

            List<XUClass>? classHierarchy = XMLExtensionsManager.TryGetClassHierarchy(propertyClass.Name);
            if (classHierarchy == null)
            {
                xui.Logger?.Here().Error("Failed to get class hierarchy for {0}, returning null.", propertyClass.Name);
                return null;
            }

            foreach (XUClass hierarchyClass in classHierarchy)
            {
                foreach (XUPropertyDefinition propertyDefinition in hierarchyClass.PropertyDefinitions)
                {
                    if (propertyDefinition.Name == propertyName)
                    {
                        return new Tuple<XUClass, XUPropertyDefinition>(hierarchyClass, propertyDefinition);
                    }
                }
            }

            return null;
        }
    }
}
