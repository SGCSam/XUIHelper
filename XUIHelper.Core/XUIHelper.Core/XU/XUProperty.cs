using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUProperty
    {
        public XUPropertyDefinition PropertyDefinition { get; private set; }
        //Only used for properties flagged as indexed i.e StopColours
        public int? Index { get; private set; }
        public object? Value { get; private set; }

        public XUProperty(XUPropertyDefinition definition, object? value = null)
        {
            PropertyDefinition = definition;
            Index = -1;
            Value = value;
        }

        public XUProperty(XUPropertyDefinition definition, int index, object? value = null)
        {
            PropertyDefinition = definition;
            Index = index;
            Value = value;
        }
    }
}
