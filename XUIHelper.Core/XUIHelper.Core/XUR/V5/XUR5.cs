using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR5 : XUR
    {
        public XUR5(string filePath, ILogger? logger = null) : base(filePath, new XUR5Header(), new XUR5SectionsTable(), logger)
        {

        }

        protected override IXURSection? TryCreateXURSectionForMagic(int Magic)
        {
            try
            {
                Logger = Logger?.ForContext(typeof(XUR5));

                Logger?.Here().Verbose("Trying to get XUR section for magic {0:X8}", Magic);

                switch (Magic)
                {
                    case ISTRNSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning STRN5 section.");
                        return new STRN5Section();
                    }
                    case IVECTSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning VECT5 section.");
                        return new VECT5Section();
                    }
                    case IQUATSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning QUAT5 section.");
                        return new QUAT5Section();
                    }
                    case ICUSTSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning CUST5 section.");
                        return new CUST5Section();
                    }
                    case IDATASection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning DATA5 section.");
                        return new DATA5Section();
                    }
                    default:
                    {
                        Logger?.Here().Error("Hit default case, unhandled magic.");
                        return null;
                    }
                }
            }
            catch (Exception ex) 
            {
                Logger?.Here().Error("Caught an exception when trying to get XUR section for magic {0:X8}, returning false. The exception is: {1}", Magic, ex);
                return null;
            }
        }

        protected override IXURCountHeader GetCountHeader()
        {
            return new XUR5CountHeader();
        }

        protected override bool HasCountHeader()
        {
            return (((XUR5Header)Header).Flags & 0x1) == 0x1;
        }

        protected override async Task<List<IXURSection>?> TryBuildSectionsFromObjectAsync(XUObject rootObject)
        {
            STRN5Section strnSection = new STRN5Section();
            if(!await strnSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build STRN5 section, returning null.");
                return null;
            }

            return null;
        }
    }
}
