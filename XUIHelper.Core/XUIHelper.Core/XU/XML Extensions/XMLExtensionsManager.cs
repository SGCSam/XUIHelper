using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XMLExtensionsManager
    {
        public static ILogger? Logger { get; private set; }

        private static string _CurrentGroup = string.Empty;

        private static Dictionary<string, List<XUIHelperExtensions>> _Groups = new Dictionary<string, List<XUIHelperExtensions>>();

        public static void Initialize(ILogger? logger = null) 
        {
            Logger = logger?.ForContext(typeof(XMLExtensionsManager));
        }

        public static async Task<bool> TryRegisterExtensionsGroupAsync(string extensionsGroupName, List<string> xmlExtensionFilePaths)
        {
            try
            {
                foreach (string xmlExtensionFilePath in xmlExtensionFilePaths)
                {
                    if(!await TryRegisterExtensionsGroupAsync(extensionsGroupName, xmlExtensionFilePath))
                    {
                        Logger?.Here().Error("Failed to register XML extensions from {0} into group {1}, returning false.", xmlExtensionFilePath, extensionsGroupName);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to register XML extensions group {0}, returning false. The exception is: {1}", extensionsGroupName, ex);
                return false;
            }
        }

        public static async Task<bool> TryRegisterExtensionsGroupAsync(string extensionsGroupName, string xmlExtensionsFilePath)
        {
            try
            {
                Logger?.Here().Verbose("Registering classes from {0} into group {1}.", xmlExtensionsFilePath, extensionsGroupName);

                string oldGroup = _CurrentGroup;
                SetCurrentGroup(extensionsGroupName);

                XUIHelperExtensions? registeredExtension = await TryRegisterXMLExtensionsAsync(xmlExtensionsFilePath);
                if (registeredExtension == null)
                {
                    Logger?.Here().Error("Registered extension for {0} was null, returning false.", xmlExtensionsFilePath);
                    return false;
                }

                _Groups[extensionsGroupName].Add(registeredExtension);

                if (!string.IsNullOrEmpty(oldGroup))
                {
                    SetCurrentGroup(oldGroup);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to register XML extensions group {0}, returning false. The exception is: {1}", extensionsGroupName, ex);
                return false;
            }
        }

        public static void SetCurrentGroup(string groupName)
        {
            if(!_Groups.ContainsKey(groupName))
            {
                _Groups[groupName] = new List<XUIHelperExtensions>();
            }

            _CurrentGroup = groupName;
        }

        public static XUClass? TryGetClassByName(string name)
        {
            if (!_Groups.ContainsKey(_CurrentGroup))
            {
                Logger?.Here().Error("Failed to get class {0} as the current group {1} is invalid, returning null.", name, _CurrentGroup);
                return null;
            }

            foreach(XUIHelperExtensions extension in _Groups[_CurrentGroup])
            {
                foreach (XUClass cla in extension.Extensions.Classes)
                {
                    if (cla.Name == name)
                    {
                        return cla;
                    }
                }
            }

            return null;
        }

        public static List<XUClass>? TryGetClassHierarchy(string derivedClass)
        {
            List<XUClass> hierarchy = new List<XUClass>();
            XUClass? thisClass = TryGetClassByName(derivedClass);

            if (thisClass == null || thisClass.Name == null)
            {
                Logger?.Here().Error("Failed to load {0} derived class when getting hierarchy. This may be a custom class that needs registered by an XML extension.", derivedClass);
                return null;
            }

            else
            {
                hierarchy.Add(thisClass);
            }

            while(true)
            {
                if (thisClass.BaseClassName == "(null)" || string.IsNullOrEmpty(thisClass.BaseClassName))
                {
                    break;
                }

                string baseClassName = thisClass.BaseClassName;
                thisClass = TryGetClassByName(baseClassName);

                if (thisClass == null || thisClass.Name == null)
                {
                    Logger?.Here().Error("Failed to load {0} base class when getting hierarchy. This may be a custom class that needs registered by an XML extension.", baseClassName);
                    return null;
                }

                else
                {
                    hierarchy.Add(thisClass);
                }
            }
            

            // Reverse the order so we start with the root class properties
            hierarchy.Reverse();
            return hierarchy;
        }

        private static async Task<XUIHelperExtensions?> TryRegisterXMLExtensionsAsync(string xmlExtensionFilePath)
        {
            try
            {
                if (!File.Exists(xmlExtensionFilePath))
                {
                    Logger?.Here().Error("Failed to register XML extensions as the file does not exist at {0}, returning null.", xmlExtensionFilePath);
                    return null;
                }

                if (!_Groups.ContainsKey(_CurrentGroup))
                {
                    Logger?.Here().Error("Failed to register XML extensions as the current group {0} is invalid, returning null.", _CurrentGroup);
                    return null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(XUIHelperExtensions));
                using (FileStream extensionsFileStream = new FileStream(xmlExtensionFilePath, FileMode.Open))
                {
                    XUIHelperExtensions? deserializedExtension = (XUIHelperExtensions?)serializer.Deserialize(extensionsFileStream);
                    if (deserializedExtension == null)
                    {
                        Logger?.Here().Error("Failed to register XML extensions at {0} as the deserialization has failed, returning null.", xmlExtensionFilePath);
                        return null;
                    }

                    foreach (XUIHelperExtensions existingExtension in _Groups[_CurrentGroup])
                    {
                        foreach (XUClass existingClass in existingExtension.Extensions.Classes)
                        {
                            foreach (XUClass deserializedClass in deserializedExtension.Extensions.Classes)
                            {
                                if (existingClass.Name == deserializedClass.Name)
                                {
                                    Logger?.Here().Error("Failed to register XML extensions at {0} as the class {1} is a duplicate, returning null.", xmlExtensionFilePath, existingClass.Name);
                                    return null;
                                }
                            }
                        }
                    }

                    foreach (XUClass deserializedClass in deserializedExtension.Extensions.Classes)
                    {
                        foreach (XUPropertyDefinition propertyDefinition in deserializedClass.PropertyDefinitions)
                        {
                            propertyDefinition.ParentClassName = deserializedClass.Name;
                        }
                    }

                    Logger?.Here().Verbose("Registered a total of {0} classes from {1}.", deserializedExtension.Extensions.Classes, xmlExtensionFilePath);
                    return deserializedExtension;
                }
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to register XML extensions at {0}, returning null. The exception is: {1}", xmlExtensionFilePath, ex);
                return null;
            }
        }
    }
}
