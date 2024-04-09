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
        public class XUIHelperExtensionsGroupData
        {
            public string GroupName { get; private set; }
            public List<XUIHelperExtensionsFile> ExtensionsFiles { get; private set; } = new List<XUIHelperExtensionsFile>();

            public XUIHelperExtensionsGroupData(string groupName, List<XUIHelperExtensionsFile> extensionsFiles)
            {
                GroupName = groupName;
                ExtensionsFiles = extensionsFiles;
            }

            public XUIHelperExtensionsGroupData(string groupName)
            {
                GroupName = groupName;
                ExtensionsFiles = new List<XUIHelperExtensionsFile>();
            }
        }

        public class XUIHelperExtensionsFile
        {
            public string FilePath { get; private set; } = string.Empty;
            public XUIHelperExtensions Data { get; private set; }

            public XUIHelperExtensionsFile(string filePath, XUIHelperExtensions data)
            {
                FilePath = filePath;
                Data = data;
            }
        }

        public static ILogger? Logger { get; private set; }

        private static string _CurrentGroup = string.Empty;

        private static Dictionary<string, XUIHelperExtensionsGroupData> _Groups = new Dictionary<string, XUIHelperExtensionsGroupData>();

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

                if(_Groups.ContainsKey(extensionsGroupName))
                {
                    foreach (XUIHelperExtensionsFile extensionsFile in _Groups[extensionsGroupName].ExtensionsFiles)
                    {
                        if (extensionsFile.FilePath == xmlExtensionsFilePath)
                        {
                            Logger?.Here().Verbose("Already registered extensions from {0} for group {1}, won't re-register, returning true.", xmlExtensionsFilePath, extensionsGroupName);
                            return true;
                        }
                    }
                }

                string oldGroup = _CurrentGroup;
                SetCurrentGroup(extensionsGroupName);

                XUIHelperExtensions? registeredExtension = await TryRegisterXMLExtensionsAsync(xmlExtensionsFilePath);
                if (registeredExtension == null)
                {
                    Logger?.Here().Error("Registered extension for {0} was null, returning false.", xmlExtensionsFilePath);
                    return false;
                }

                _Groups[extensionsGroupName].ExtensionsFiles.Add(new XUIHelperExtensionsFile(xmlExtensionsFilePath, registeredExtension));

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
                _Groups[groupName] = new XUIHelperExtensionsGroupData(groupName);
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

            foreach(XUIHelperExtensionsFile extensionFile in _Groups[_CurrentGroup].ExtensionsFiles)
            {
                if(extensionFile.Data.Extensions == null)
                {
                    continue;
                }

                foreach (XUClass cla in extensionFile.Data.Extensions.Classes)
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

        public static bool? IsPropertyIgnored(XUPropertyDefinition propertyDefinition) 
        {
            try
            {
                if (!_Groups.ContainsKey(_CurrentGroup))
                {
                    Logger?.Here().Error("Failed to get check if property {0} is ignored as the current group {1} is invalid, returning null.", propertyDefinition.Name, _CurrentGroup);
                    return null;
                }

                foreach(XUIHelperExtensionsFile extensionFile in _Groups[_CurrentGroup].ExtensionsFiles)
                {
                    if (extensionFile.Data.IgnoreProperties == null)
                    {
                        continue;
                    }

                    foreach (XUIHelperIgnoreClass ignoredClass in extensionFile.Data.IgnoreProperties.IgnoredClasses)
                    {
                        if (propertyDefinition.ParentClassName != ignoredClass.ClassName)
                        {
                            continue;
                        }

                        foreach (XUIHelperIgnoreProperty ignoredProperty in ignoredClass.IgnornedProperties)
                        {
                            if (ignoredProperty.Value == propertyDefinition.Name)
                            {
                                return true;
                            }
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying check if property {0} was ignored, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
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

                    if(deserializedExtension.Extensions == null)
                    {
                        Logger?.Here().Error("Failed to register XML extensions at {0} as the extensions are null, returning null.", xmlExtensionFilePath);
                        return null;
                    }

                    if(deserializedExtension.IgnoreProperties != null)
                    {
                        HashSet<string> foundIgnoredClassNames = new HashSet<string>();
                        foreach (XUIHelperIgnoreClass ignoredClass in deserializedExtension.IgnoreProperties.IgnoredClasses)
                        {
                            if (foundIgnoredClassNames.Contains(ignoredClass.ClassName))
                            {
                                Logger?.Here().Error("Failed to add ignore class {0} in XML extensions at {1} as it was a duplicate, returning null.", ignoredClass.ClassName, xmlExtensionFilePath);
                                return null;
                            }

                            XUClass? foundClass = null;
                            foreach (XUClass deserializedClass in deserializedExtension.Extensions.Classes)
                            {
                                if (ignoredClass.ClassName == deserializedClass.Name)
                                {
                                    foundClass = deserializedClass;
                                    break;
                                }
                            }

                            if (foundClass == null)
                            {
                                Logger?.Here().Error("Failed to find ignore class {0} in XML extensions at {1}, returning null.", ignoredClass.ClassName, xmlExtensionFilePath);
                                return null;
                            }

                            foundIgnoredClassNames.Add(ignoredClass.ClassName);
                            Logger?.Here().Verbose("Handling ignored class {0}.", ignoredClass.ClassName);

                            HashSet<string> foundIgnoredPropertyNames = new HashSet<string>();
                            foreach (XUIHelperIgnoreProperty ignoredProperty in ignoredClass.IgnornedProperties)
                            {
                                if (foundIgnoredPropertyNames.Contains(ignoredProperty.Value))
                                {
                                    Logger?.Here().Error("Failed to add ignore class {0} in XML extensions at {1} as the property {2} was a duplicate, returning null.", ignoredClass.ClassName, xmlExtensionFilePath, ignoredProperty.Value);
                                    return null;
                                }

                                bool found = false;
                                foreach (XUPropertyDefinition propertyDefinition in foundClass.PropertyDefinitions)
                                {
                                    if (propertyDefinition.Name == ignoredProperty.Value)
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    Logger?.Here().Error("Failed to find property definition {0} from ignored class {1} in XML extensions at {2}, returning null.", ignoredProperty.Value, ignoredClass.ClassName, xmlExtensionFilePath);
                                    return null;
                                }

                                Logger?.Here().Verbose("Will ignore property {0} of {1}.", ignoredProperty.Value, ignoredClass.ClassName);
                                foundIgnoredPropertyNames.Add(ignoredProperty.Value);
                            }
                        }
                    }

                    foreach (XUIHelperExtensionsFile existingExtensionFile in _Groups[_CurrentGroup].ExtensionsFiles)
                    {
                        if(existingExtensionFile.Data.Extensions == null)
                        {
                            continue;
                        }

                        foreach (XUClass existingClass in existingExtensionFile.Data.Extensions.Classes)
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

                    Logger?.Here().Verbose("Registered a total of {0} classes from {1}.", deserializedExtension.Extensions.Classes.Count, xmlExtensionFilePath);
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
