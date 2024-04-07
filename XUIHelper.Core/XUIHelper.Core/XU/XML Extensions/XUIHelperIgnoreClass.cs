using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public class XUIHelperIgnoreClass
    {
        [XmlAttribute("ClassName")]
        public string ClassName { get; set; }

        [XmlElement("IgnoreProperty")]
        public List<XUIHelperIgnoreProperty> IgnornedProperties { get; set; } = new List<XUIHelperIgnoreProperty>();

        public XUIHelperIgnoreClass()
        {

        }
    }
}
