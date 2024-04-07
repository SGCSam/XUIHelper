using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public class XUIHelperIgnoreProperties
    {
        [XmlElement("IgnoreClass")]
        public List<XUIHelperIgnoreClass> IgnoredClasses { get; set; } = new List<XUIHelperIgnoreClass>();

        public XUIHelperIgnoreProperties() 
        { 
        
        }
    }
}
