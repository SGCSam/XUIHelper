using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUObject
    {
        public string ClassName { get; set; } = string.Empty;
        public List<XUProperty> Properties { get; set; } = new List<XUProperty>();
        public List<XUObject> Children { get; set; } = new List<XUObject>();

        public XUObject(string className, List<XUProperty> properties, List<XUObject> children)
        {
            ClassName = className;
            Properties = properties;
            Children = children;
        }

        public XUObject() 
        {
            
        }
    }
}
