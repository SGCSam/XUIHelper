using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XURBuildExtensions
    {
        public static async Task<List<T>?> TryBuildPropertyTypeAsync<T>(this XUObject xuObject, IXUR xur)
        {
            try
            {
                XUPropertyDefinitionTypes? propertyType = null;
                if(typeof(T) == typeof(bool))
                {
                    propertyType = XUPropertyDefinitionTypes.Bool;
                }
                else if(typeof(T) == typeof(int))
                {
                    propertyType = XUPropertyDefinitionTypes.Integer;
                }
                else if (typeof(T) == typeof(uint))
                {
                    propertyType = XUPropertyDefinitionTypes.Unsigned;
                }
                else if (typeof(T) == typeof(float))
                {
                    propertyType = XUPropertyDefinitionTypes.Float;
                }
                else if (typeof(T) == typeof(XUColour))
                {
                    propertyType = XUPropertyDefinitionTypes.Colour;
                }
                else if (typeof(T) == typeof(XUVector))
                {
                    propertyType = XUPropertyDefinitionTypes.Vector;
                }
                else if (typeof(T) == typeof(XUQuaternion))
                {
                    propertyType = XUPropertyDefinitionTypes.Quaternion;
                }
                else if (typeof(T) == typeof(XUFigure))
                {
                    propertyType = XUPropertyDefinitionTypes.Custom;
                }

                if(propertyType == null)
                {
                    xur.Logger?.Here().Error("Unhandled property for type, returning null.");
                    return null;
                }

                HashSet<T> builtTypes = new HashSet<T>();
                if (!TryBuildTypesFromObject(xur, xuObject, propertyType.Value, ref builtTypes))
                {
                    xur.Logger?.Here().Error("Failed to build {0}, returning null.", propertyType.Value);
                    return null;
                }

                return builtTypes.ToList();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when building types, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        private static bool TryBuildTypesFromObject<T>(IXUR xur, XUObject xuObject, XUPropertyDefinitionTypes propertyType, ref HashSet<T> builtTypes)
        {
            try
            {
                if (!TryBuildTypesFromProperties<T>(xur, xuObject.Properties, propertyType, ref builtTypes))
                {
                    xur.Logger?.Here().Error("Failed to build types from properties for {0}, returning false.", xuObject.ClassName);
                    return false;
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildTypesFromObject<T>(xur, childObject, propertyType, ref builtTypes))
                    {
                        xur.Logger?.Here().Error("Failed to get types for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach (XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == propertyType)
                            {
                                if (animatedProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                                {
                                    int valueIndex = 0;
                                    foreach (object? valueObj in animatedProperty.Value as List<object?>)
                                    {
                                        if (animatedProperty.Value == null)
                                        {
                                            //This index isn't animated
                                            continue;
                                        }

                                        if (valueObj is not T valueType)
                                        {
                                            xur.Logger?.Here().Error("Animated indexed child property {0} at index {1} marked as {2} had a value of {3}, returning false.", animatedProperty.PropertyDefinition.Name, valueIndex, propertyType, valueObj);
                                            return false;
                                        }

                                        if (builtTypes.Add(valueType))
                                        {
                                            xur.Logger?.Here().Verbose("Added {0} animated indexed property value index {1}, {2}.", animatedProperty.PropertyDefinition.Name, valueIndex, valueType);
                                        }

                                        valueIndex++;
                                    }
                                }
                                else
                                {
                                    if (animatedProperty.Value is not T valueType)
                                    {
                                        xur.Logger?.Here().Error("Animated property {0} marked as {1} had a value of {2}, returning false.", animatedProperty.PropertyDefinition.Name, propertyType, animatedProperty.Value);
                                        return false;
                                    }

                                    if (builtTypes.Add(valueType))
                                    {
                                        xur.Logger?.Here().Verbose("Added {0} animated property value {1}.", animatedProperty.PropertyDefinition.Name, valueType);
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build types for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private static bool TryBuildTypesFromProperties<T>(IXUR xur, List<XUProperty> properties, XUPropertyDefinitionTypes propertyType, ref HashSet<T> builtTypes)
        {
            try
            {
                foreach (XUProperty childProperty in properties)
                {
                    if (childProperty.PropertyDefinition.Type == propertyType)
                    {
                        if (childProperty.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                        {
                            int valueIndex = 0;
                            foreach (object valueObj in childProperty.Value as List<object>)
                            {
                                if (valueObj is not T valueType)
                                {
                                    xur.Logger?.Here().Error("Indexed child property {0} at index {1} marked as {2} had a value of {3}, returning false.", childProperty.PropertyDefinition.Name, valueIndex, propertyType, valueObj);
                                    return false;
                                }

                                if (builtTypes.Add(valueType))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} indexed property value index {1}, {2}.", childProperty.PropertyDefinition.Name, valueIndex, valueType);
                                }

                                valueIndex++;
                            }
                        }
                        else
                        {
                            if (childProperty.Value is not T valueType)
                            {
                                xur.Logger?.Here().Error("Child property {0} marked as {1} had a value of {2}, returning false.", childProperty.PropertyDefinition.Name, propertyType, childProperty.Value);
                                return false;
                            }

                            if (builtTypes.Add(valueType))
                            {
                                xur.Logger?.Here().Verbose("Added {0} property value {1}.", childProperty.PropertyDefinition.Name, valueType);
                            }
                        }
                    }
                    else if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if (!TryBuildTypesFromProperties<T>(xur, childProperty.Value as List<XUProperty>, propertyType, ref builtTypes))
                        {
                            xur.Logger?.Here().Error("Failed to build types for child compound properties, returning false.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build types from properties, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
