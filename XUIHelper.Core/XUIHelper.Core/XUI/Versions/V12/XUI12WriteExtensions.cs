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
    public static class XUR12WriteExtensions
    {
        public static List<XElement>? TryWriteProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                int indexCount = 1;
                bool isIndexed = property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed);
                if (isIndexed)
                {
                    indexCount = (property.Value as IList).Count;
                }

                List<XElement> retValue = new List<XElement>(); 
                for(int i = 0; i < indexCount; i++)
                {
                    object propertyVal;
                    if(isIndexed)
                    {
                        propertyVal = (property.Value as List<object>)[i];
                    }
                    else
                    {
                        propertyVal = property.Value;
                    }

                    XElement? readValue = null;
                    switch (property.PropertyDefinition.Type)
                    {
                        case XUPropertyDefinitionTypes.Bool:
                        {
                            readValue = TryWriteBoolProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Integer:
                        {
                            readValue = TryWriteIntegerProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Unsigned:
                        {
                            readValue = TryWriteUnsignedProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.String:
                        {
                            readValue = TryWriteStringProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Float:
                        {
                            readValue = TryWriteFloatProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Vector:
                        {
                            readValue = TryWriteVectorProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Object:
                        {
                            readValue = TryWriteObjectProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Colour:
                        {
                            readValue = TryWriteColourProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Custom:
                        {
                            readValue = TryWriteCustomProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Quaternion:
                        {
                            readValue = TryWriteQuaternionProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        default:
                        {
                            xui.Logger?.Here().Error("Unhandled property type of {0} when writing property {1}, returning null.", property.PropertyDefinition.Type, property.PropertyDefinition.Name);
                            return null;
                        }
                    }

                    if (readValue == null)
                    {
                        xui.Logger?.Here().Error("Written value was null when writing property {0}, an error must have occurred, returning null.", property.PropertyDefinition.Name);
                        return null;
                    }
                    else if (!isIndexed)
                    {
                        retValue = new List<XElement>() { readValue };
                        break;
                    }
                    else
                    {
                        readValue.SetAttributeValue("index", i);
                        retValue.Add(readValue);
                    }
                }

                return retValue;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteBoolProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing bool property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = (bool)val ? "true" : "false";
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteIntegerProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing integer property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((int)val).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteUnsignedProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing unsigned property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((uint)val).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteStringProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing string property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((string)val).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteFloatProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing float property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = Convert.ToDouble(val).ToString("0.000000");
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteVectorProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing vector property {0} with value {1}", propertyDefinition.Name, val);
                XUVector? vect = val as XUVector;
                if(vect == null)
                {
                    xui.Logger?.Here().Error("Property value was not a vector, returning null.");
                    return null;
                }

                retElement.Value = string.Join(",", new List<string>()
                {
                    ((double)vect.X).ToString("0.000000"),
                    ((double)vect.Y).ToString("0.000000"),
                    ((double)vect.Z).ToString("0.000000")
                }).TrimEnd();

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteObjectProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing object property {0}", propertyDefinition.Name);

                XElement parentPropertiesElement = new XElement("Properties");
                foreach(XUProperty compoundProperty in val as List<XUProperty>) 
                { 
                    List<XElement>? thisPropertyElements = TryWriteProperty(xui, compoundProperty);
                    if (thisPropertyElements == null)
                    {
                        xui.Logger?.Here().Error("Failed to write compound property {0}, an error must have occurred, returning null.", compoundProperty.PropertyDefinition.Name);
                        return null;
                    }

                    foreach (XElement propertyElement in thisPropertyElements)
                    {
                        parentPropertiesElement.Add(propertyElement);
                    }
                }

                retElement.Add(parentPropertiesElement);
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteColourProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing colour property {0} with value {1}", propertyDefinition.Name, val);
                XUColour? colour = val as XUColour;
                if (colour == null)
                {
                    xui.Logger?.Here().Error("Property value was not a colour, returning null.");
                    return null;
                }

                retElement.Value = string.Format("0x{0:x2}{1:x2}{2:x2}{3:x2}", colour.A, colour.R, colour.G, colour.B);
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteCustomProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing custom property {0} with value {1}", propertyDefinition.Name, val);
                XUFigure? figure = val as XUFigure;
                if (figure == null)
                {
                    xui.Logger?.Here().Error("Property value was not a figure, returning null.");
                    return null;
                }

                retElement.Value += figure.Points.Count;
                retElement.Value += ",";
                foreach(XUBezierPoint bezierPoint in figure.Points) 
                {
                    retElement.Value += ((double)bezierPoint.Point.X).ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += ((double)bezierPoint.Point.Y).ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += ((double)bezierPoint.ControlPointOne.X).ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += ((double)bezierPoint.ControlPointOne.Y).ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += ((double)bezierPoint.ControlPointTwo.X).ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += ((double)bezierPoint.ControlPointTwo.Y).ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += "0";
                    retElement.Value += ",";
                }

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteQuaternionProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing quaternion property {0} with value {1}", propertyDefinition.Name, val);
                XUQuaternion? quat = val as XUQuaternion;
                if (quat == null)
                {
                    xui.Logger?.Here().Error("Property value was not a quaternion, returning null.");
                    return null;
                }

                retElement.Value = string.Join(",", new List<string>()
                {
                    quat.X.ToString("0.000000"),
                    quat.Y.ToString("0.000000"),
                    quat.Z.ToString("0.000000"),
                    quat.W.ToString("0.000000")
                }).TrimEnd();

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteNamedFrame(this XUI12 xui, XUNamedFrame namedFrame)
        {
            try
            {
                xui.Logger?.Here().Verbose("Writing named frame with value {0}", namedFrame);

                XElement retElement = new XElement("NamedFrame");
                retElement.Add(new XElement("Name", namedFrame.Name));
                retElement.Add(new XElement("Time", namedFrame.Keyframe));

                if(namedFrame.CommandType == XUNamedFrameCommandTypes.Play)
                {
                    xui.Logger?.Here().Verbose("Command type is play, returning.");
                    return retElement;
                }
                else
                {
                    retElement.Add(new XElement("Command", namedFrame.CommandType.ToString().ToLower()));
                    if(namedFrame.CommandType == XUNamedFrameCommandTypes.Stop)
                    {
                        xui.Logger?.Here().Verbose("Command type is stop, returning.");
                        return retElement;
                    }

                    xui.Logger?.Here().Verbose("Command type is {0}, setting params as {1}.", namedFrame.CommandType, namedFrame.TargetParameter);
                    retElement.Add(new XElement("CommandParams", namedFrame.TargetParameter));
                    return retElement;
                }
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing named frame {0}, returning null. The exception is: {1}", namedFrame.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteTimeline(this XUI12 xui, XUTimeline timeline)
        {
            try
            {
                xui.Logger?.Here().Verbose("Writing timeline.");
                XElement retElement = new XElement("Timeline");
                retElement.Add(new XElement("Id", timeline.ElementName));

                foreach(XUProperty animatedProperty in timeline.Keyframes[0].Properties) 
                {
                    string elementPropertyDefinitionName = animatedProperty.PropertyDefinition.Name;
                    if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureFill")
                    {
                        elementPropertyDefinitionName = string.Format("Fill.{0}", animatedProperty.PropertyDefinition.Name);
                    }
                    else if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureFillGradient")
                    {
                        elementPropertyDefinitionName = string.Format("Fill.Gradient.{0}", animatedProperty.PropertyDefinition.Name);
                    }
                    else if (animatedProperty.PropertyDefinition.ParentClassName == "XuiFigureStroke")
                    {
                        elementPropertyDefinitionName = string.Format("Stroke.{0}", animatedProperty.PropertyDefinition.Name);
                    }

                    if (!animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        retElement.Add(new XElement("TimelineProp", elementPropertyDefinitionName));
                    }
                    else
                    {
                        int valueIndex = 0;
                        foreach(object val in animatedProperty.Value as List<object>)
                        {
                            XElement thisTimelinePropElement = new XElement("TimelineProp", elementPropertyDefinitionName);
                            thisTimelinePropElement.SetAttributeValue("index", valueIndex);
                            retElement.Add(thisTimelinePropElement);
                            valueIndex++;
                        }
                    }
                }
                
                foreach(XUKeyframe keyframe in timeline.Keyframes)
                {
                    XElement thisKeyframeElement = new XElement("KeyFrame");
                    thisKeyframeElement.Add(new XElement("Time", keyframe.Keyframe));
                    thisKeyframeElement.Add(new XElement("Interpolation", (int)keyframe.InterpolationType));

                    foreach(XUProperty animatedProperty in  keyframe.Properties)
                    {
                        List<XElement>? propertyElements = TryWriteProperty(xui, animatedProperty);
                        if (propertyElements == null)
                        {
                            xui.Logger?.Here().Verbose("Property elements was null when trying to write animated property {0} for keyframe {1} of timeline {2}, returning null.", animatedProperty.PropertyDefinition.Name, keyframe.Keyframe, timeline.ElementName);
                            return null;
                        }
                        else if (!animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed) && propertyElements.Count != 1)
                        {
                            xui.Logger?.Here().Verbose("Property elements had an unexpected count when trying to write animated property {0} for keyframe {1} of timeline {2}, returning null. Expected: 1, Actual: {3}", animatedProperty.PropertyDefinition.Name, keyframe.Keyframe, timeline.ElementName, propertyElements.Count);
                            return null;
                        }
                        else if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            int valueIndex = 0;
                            foreach (object val in animatedProperty.Value as List<object>)
                            {
                                thisKeyframeElement.Add(new XElement("Prop", propertyElements[valueIndex].Value));
                                valueIndex++;
                            }
                        }
                        else
                        {
                            thisKeyframeElement.Add(new XElement("Prop", propertyElements[0].Value));
                        }
                    }

                    retElement.Add(thisKeyframeElement);
                }

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
