using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public class XUClass
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("BaseClassName")]
        public string BaseClassName { get; set; }

        [XmlElement("PropDef")]
        public List<XUPropertyDefinition> PropertyDefinitions { get; set; } = new List<XUPropertyDefinition>();

        public XUClass(string name, string baseClassName, List<XUPropertyDefinition> propDefs)
        {
            Name = name;
            BaseClassName = baseClassName;
            PropertyDefinitions = propDefs;
        }

        public XUClass() 
        { 
        
        }
    }
}
