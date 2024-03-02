using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace XUIHelper.Core
{
    public class XUObject
    {
        public string ClassName { get; set; } = string.Empty;
        public List<XUProperty> Properties { get; set; } = new List<XUProperty>();
        public List<XUObject> Children { get; set; } = new List<XUObject>();
        public List<XUNamedFrame> NamedFrames { get; set; } = new List<XUNamedFrame>();
        public List<XUTimeline> Timelines { get; set; } = new List<XUTimeline>();

        public XUObject(string className, List<XUProperty> properties, List<XUObject> children, List<XUNamedFrame> namedFrames, List<XUTimeline> timelines)
        {
            ClassName = className;
            Properties = properties;
            Children = children;
            NamedFrames = namedFrames;
            Timelines = timelines;
        }

        public XUObject(string className)
        {
            ClassName = className;
        }

        public XUObject? TryFindChildById(string id)
        {
            foreach (XUObject child in Children)
            {
                foreach (XUProperty property in child.Properties)
                {
                    if (property.PropertyDefinition.Name == "Id")
                    {
                        if (property.Value?.ToString() == id)
                        {
                            return child;
                        }
                    }
                }
            }

            return null;
        }

        public int GetTotalObjectsCount()
        {
            int retCount = 1;
            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetTotalObjectsCount();
            }

            return retCount;
        }

        public int GetPropertiesArrayCount()
        {
            int retCount = 1;
            foreach (XUProperty childProperty in Properties)
            {
                retCount += childProperty.GetCompoundPropertiesCount();
            }

            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetPropertiesArrayCount();
            }

            return retCount;
        }

        public int GetTotalPropertiesCount()
        {
            int retCount = 0;
            if (Properties.Count > 0)
            {
                foreach (XUProperty childProperty in Properties)
                {
                    retCount += childProperty.GetChildValuesCount();
                }
            }

            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetTotalPropertiesCount();
            }

            return retCount;
        }

        public int GetKeyframesCount()
        {
            int retCount = 0;
            foreach(XUTimeline childTimeline in Timelines)
            {
                retCount += childTimeline.Keyframes.Count;
            }

            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetKeyframesCount();
            }

            return retCount;
        }

        public int GetTotalKeyframePropertiesCount()
        {
            int retCount = 0;
            foreach(XUTimeline childTimeline in Timelines)
            {
                foreach(XUKeyframe childKeyframe in childTimeline.Keyframes)
                {
                    foreach(XUProperty animatedProperty in childKeyframe.Properties)
                    {
                        if (animatedProperty.Value is IList list)
                        {
                            //Indexed properties, such as StopColor, will have a list of values for each stop point
                            retCount += list.Count;
                        }
                        else
                        {
                            retCount++;
                        }
                    }
                }
            }

            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetTotalKeyframePropertiesCount();
            }

            return retCount;
        }

        public int GetTimelinesCount()
        {
            int retCount = Timelines.Count;
            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetTimelinesCount();
            }

            return retCount;
        }

        public int GetNamedFramesCount()
        {
            int retCount = NamedFrames.Count;
            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetNamedFramesCount();
            }

            return retCount;
        }

        public int GetObjectsWithChildrenCount()
        {
            int retCount = 0;
            if (Children.Count > 0)
            {
                retCount++;
                foreach(XUObject childObject in Children)
                {
                    retCount += childObject.GetObjectsWithChildrenCount();
                }
            }

            return retCount;
        }

        public int GetKeyframePropertyDefinitionsCount()
        {
            int retCount = 0;
            foreach(XUTimeline childTimeline in Timelines)
            {
                HashSet<XUPropertyDefinition> knownDefs = new HashSet<XUPropertyDefinition>();
                foreach(XUKeyframe childKeyframe in childTimeline.Keyframes)
                {
                    foreach (XUProperty keyframeProperty in childKeyframe.Properties)
                    {
                        if(knownDefs.Contains(keyframeProperty.PropertyDefinition))
                        {
                            continue;
                        }

                        if(keyframeProperty.Value is IList list)
                        {
                            //Indexed properties, such as StopColor, will have a list of values for each stop point
                            retCount += list.Count;
                        }
                        else
                        {
                            retCount++;
                        }

                        knownDefs.Add(keyframeProperty.PropertyDefinition);
                    }
                }
            }

            foreach(XUObject childObject in Children)
            {
                retCount += childObject.GetKeyframePropertyDefinitionsCount();
            }

            return retCount;
        }

        public int? TryGetTotalKeyframePropertyDefinitionsClassDepth(int extensionVersion, ILogger? logger = null)
        {
            int retDepth = 0;

            foreach(XUTimeline timeline in Timelines) 
            {
                XUObject? animatedChild = TryFindChildById(timeline.ElementName);
                if(animatedChild == null) 
                {
                    logger?.Here().Error("Failed to find animated child {0}, returning null.", timeline.ElementName);
                    return null;
                }

                foreach(XUProperty animatedProperty in timeline.Keyframes[0].Properties)
                {
                    int? gotDepth = TryGetDepthOfPropertyDefinition(animatedProperty.PropertyDefinition, animatedChild.ClassName, extensionVersion, 1, logger);
                    if (gotDepth == null)
                    {
                        logger?.Here().Error("Failed to get property definition depth for {0}, returning null.", animatedProperty.PropertyDefinition.Name);
                        return null;
                    }

                    if (animatedProperty.Value is IList list) 
                    {
                        //If we've got StopColor, it'll have a depth of 3, but we need to multiply by how many colours we have
                        retDepth += (gotDepth.Value * list.Count);
                    }
                    else
                    {
                        retDepth += gotDepth.Value;
                    }
                }
            }

            foreach(XUObject childObject in Children)
            {
                int? childDepth = childObject.TryGetTotalKeyframePropertyDefinitionsClassDepth(extensionVersion, logger);
                if(childDepth == null)
                {
                    logger?.Here().Error("Child depth for class {0} was null, returning null.", childObject.ClassName);
                    return null;
                }

                retDepth += childDepth.Value;
            }

            return retDepth;
        }

        public int? TryGetDepthOfPropertyDefinition(XUPropertyDefinition propertyDefinition, string className, int extensionVersion, int depth, ILogger? logger = null)
        {
            if(!XUIHelperCoreConstants.VersionedExtensions.ContainsKey(extensionVersion))
            {
                logger?.Here().Error("Failed to find extensions with version {0}, returning null.", extensionVersion);
                return null;
            }

            XMLExtensionsManager manager = XUIHelperCoreConstants.VersionedExtensions[extensionVersion];
            List<XUClass>? classHierarchy = manager.TryGetClassHierarchy(className);
            if (classHierarchy == null)
            {
                logger?.Here().Error("Failed to get class hierarchy for {0}, returning null.", className);
                return null;
            }

            foreach (XUClass hierarchyClass in classHierarchy)
            {
                foreach(XUPropertyDefinition classPropertyDefinition in hierarchyClass.PropertyDefinitions)
                {
                    if(classPropertyDefinition == propertyDefinition)
                    {
                        return depth;
                    }
                    else if(classPropertyDefinition.Type == XUPropertyDefinitionTypes.Object) 
                    {
                        XUClass? compoundClass;
                        switch (classPropertyDefinition.Name)
                        {
                            case "Fill":
                            {
                                compoundClass = manager.TryGetClassByName("XuiFigureFill");
                                break;
                            }

                            case "Gradient":
                            {
                                compoundClass = manager.TryGetClassByName("XuiFigureFillGradient");
                                break;
                            }

                            case "Stroke":
                            {
                                compoundClass = manager.TryGetClassByName("XuiFigureStroke");
                                break;
                            }
                            default:
                            {
                                logger?.Here().Error("Unhandled compound class of {0}, returning null.", classPropertyDefinition.Name);
                                return null;
                            }
                        }

                        if (compoundClass == null)
                        {
                            logger?.Here().Error("Compound class was null, returning null.");
                            return null;
                        }

                        int? foundDepth = TryGetDepthOfPropertyDefinition(propertyDefinition, compoundClass.Name, extensionVersion, depth + 1, logger);
                        if(foundDepth != null) 
                        {
                            return foundDepth.Value;
                        }
                    }
                }
            }

            return null;
        }

        public int GetTotalTimelinePropertyDefinitionsClassDepth()
        {
            int retDepth = 0;
            foreach (XUTimeline timeline in Timelines)
            {
                foreach (XUProperty property in timeline.Keyframes[0].Properties)
                {
                    if (property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        retDepth += ((IList)property.Value).Count;
                    }
                    else
                    {
                        retDepth++;
                    }
                }
            }

            foreach (XUObject childObject in Children)
            {
                retDepth += childObject.GetTotalTimelinePropertyDefinitionsClassDepth();
            }

            return retDepth;
        }
    }
}
