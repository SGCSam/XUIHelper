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
    public static class XUIReadExtensions
    {
        public static XUProperty? TryReadProperty(this IXUI xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (xui is XUI12 xui12)
                {
                    xui.Logger?.Here().Verbose("Trying to read XUI12 property {0}", propertyDefinition.Name);
                    return xui12.TryReadProperty(propertyDefinition, element);
                }

                xui.Logger?.Here().Error("Unhandled IXUI for property {0}, returning null.", propertyDefinition.Name);
                return null;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when trying to read property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUNamedFrame? TryReadNamedFrame(this IXUI xui, XElement element)
        {
            try
            {
                if (xui is XUI12 xui12)
                {
                    xui.Logger?.Here().Verbose("Trying to read XUI12 named frame.");
                    return xui12.TryReadNamedFrame(element);
                }

                xui.Logger?.Here().Error("Unhandled IXUI for named frame, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when trying to read named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static XUTimeline? TryReadTimeline(this IXUI xui, XElement element, XUObject obj)
        {
            try
            {
                if (xui is XUI12 xui12)
                {
                    xui.Logger?.Here().Verbose("Trying to read XUI12 timeline.");
                    return xui12.TryReadTimeline(element, obj);
                }

                xui.Logger?.Here().Error("Unhandled IXUI for timeline, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when trying to read timline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
