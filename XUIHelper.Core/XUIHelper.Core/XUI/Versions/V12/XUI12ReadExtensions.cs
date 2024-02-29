using Serilog;
using Serilog.Core;
using System;
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

                if (xui.ExtensionsManager == null)
                {
                    xui.Logger?.Here().Error("Failed to get extensions manager, returning null.");
                    return null;
                }

                XUClass? compoundClass = null;
                switch (propertyDefinition.Name)
                {
                    case "Fill":
                    {
                        xui.Logger?.Here()?.Verbose("Reading fill object.");
                        compoundClass = xui.ExtensionsManager.TryGetClassByName("XuiFigureFill");
                        break;
                    }

                    case "Gradient":
                    {
                        xui.Logger?.Here()?.Verbose("Reading gradient object.");
                        compoundClass = xui.ExtensionsManager.TryGetClassByName("XuiFigureFillGradient");
                        break;
                    }

                    case "Stroke":
                    {
                        xui.Logger?.Here()?.Verbose("Reading stroke object.");
                        compoundClass = xui.ExtensionsManager.TryGetClassByName("XuiFigureStroke");
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

                    properties.Add(readProperty);
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
                }

                return new XUFigure(new XUPoint(), bezierPoints);
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
    }
}
