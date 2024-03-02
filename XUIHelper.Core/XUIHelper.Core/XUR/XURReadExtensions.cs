using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XURReadExtensions
    {
        public static XUProperty? TryReadProperty(this IXUR xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (xur is XUR5 xur5)
                {
                    xur.Logger?.Here().Verbose("Trying to read XUR5 property {0}", propertyDefinition.Name);
                    return xur5.TryReadProperty(reader, propertyDefinition);
                }
                else if(xur is XUR8 xur8)
                {
                    xur.Logger?.Here().Verbose("Trying to read XUR8 property {0}", propertyDefinition.Name);
                    return xur8.TryReadProperty(reader, propertyDefinition);
                }

                xur.Logger?.Here().Error("Unhandled IXUR for property {0}, returning null.", propertyDefinition.Name);
                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to read property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUNamedFrame? TryReadNamedFrame(this IXUR xur, BinaryReader reader)
        {
            try
            {
                if (xur is XUR5 xur5)
                {
                    xur.Logger?.Here().Verbose("Trying to read XUR5 named frame.");
                    return xur5.TryReadNamedFrame(reader);
                }

                xur.Logger?.Here().Error("Unhandled IXUR for named frame, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to read named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static XUTimeline? TryReadTimeline(this IXUR xur, BinaryReader reader, XUObject obj)
        {
            try
            {
                if (xur is XUR5 xur5)
                {
                    xur.Logger?.Here().Verbose("Trying to read XUR5 timeline.");
                    return xur5.TryReadTimeline(reader, obj);
                }

                xur.Logger?.Here().Error("Unhandled IXUR for timeline, returning null.");
                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to read timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
