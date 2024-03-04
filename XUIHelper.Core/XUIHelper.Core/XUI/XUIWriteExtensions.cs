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
        public static List<XElement>? TryWriteProperty(this IXUI xui, XUProperty property)
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

        public static XElement? TryWriteNamedFrame(this IXUI xui, XUNamedFrame namedFrame)
        {
            try
            {
                if (xui is XUI12 xui12)
                {
                    xui.Logger?.Here().Verbose("Trying to write XUI12 named frame {0}", namedFrame);
                    return xui12.TryWriteNamedFrame(namedFrame);
                }

                xui.Logger?.Here().Error("Unhandled IXUI for named frame, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when trying to write named frame {0}, returning null. The exception is: {1}", namedFrame, ex);
                return null;
            }
        }

        public static XElement? TryWriteTimeline(this IXUI xui, XUTimeline timeline)
        {
            try
            {
                if (xui is XUI12 xui12)
                {
                    xui.Logger?.Here().Verbose("Trying to write XUI12 timeline.");
                    return xui12.TryWriteTimeline(timeline);
                }

                xui.Logger?.Here().Error("Unhandled IXUI for timeline, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when trying to write timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
