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
        public object? Value { get; private set; }

        public XUProperty(XUPropertyDefinition definition, object? value = null)
        {
            PropertyDefinition = definition;
            Value = value;
        }
    }
}
