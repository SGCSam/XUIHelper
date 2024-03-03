using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUIWriteExtensions
    {
        public static XElement? TryWriteProperty(this IXUI xui, XUProperty property)
        {
            try
            {
                if (xui is XUI12 xui12)
                {
                    xui.Logger?.Here().Verbose("Trying to write XUI12 property {0}", property.PropertyDefinition.Name);
                    return xui12.TryWriteProperty(property);
                }

                xui.Logger?.Here().Error("Unhandled IXUI for property {0}, returning null.", property.PropertyDefinition.Name);
                return null;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when trying to write property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
