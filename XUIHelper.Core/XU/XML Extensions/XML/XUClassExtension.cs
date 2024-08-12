using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable, XmlType(TypeName = "XUIClassExtension")]
    public class XUClassExtension
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("XUIClass")]
        public List<XUClass> Classes { get; set; } = new List<XUClass>();

        public XUClassExtension(string version, List<XUClass> classes)
        {
            Version = version;
            Classes = classes;
        }

        public XUClassExtension() 
        {
            
        }
    }
}
