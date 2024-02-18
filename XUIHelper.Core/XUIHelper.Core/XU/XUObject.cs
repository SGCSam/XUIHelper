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

        public XUObject() 
        {
            
        }
    }
}
