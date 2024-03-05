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

        protected override bool HasReadableCountHeader()
        {
            return (((XUR5Header)Header).Flags & 0x1) == 0x1;
        }

        protected override bool ShouldWriteCountHeader(XUObject rootObject)
        {
            return rootObject.GetTotalObjectsCount() > 0x8D
                || rootObject.GetTotalPropertiesCount() > 0x235
                || rootObject.GetPropertiesArrayCount() > 0xC9;
        }

        protected override async Task<List<IXURSection>?> TryBuildSectionsFromObjectAsync(XUObject rootObject)
        {
            List<IXURSection> retList = new List<IXURSection>();

            STRN5Section strnSection = new STRN5Section();
            if(!await strnSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build STRN5 section, returning null.");
                return null;
            }
            else if(strnSection.Strings.Count > 0)
            {
                Logger?.Here().Verbose("Adding STRN5 section.");
                retList.Add(strnSection);
            }
            else
            {
                Logger?.Here().Verbose("STRN5 section had no strings, not adding.");
            }

            VECT5Section vectSection = new VECT5Section();
            if (!await vectSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build VECT5 section, returning null.");
                return null;
            }
            else if (vectSection.Vectors.Count > 0)
            {
                Logger?.Here().Verbose("Adding VECT5 section.");
                retList.Add(vectSection);
            }
            else
            {
                Logger?.Here().Verbose("VECT5 section had no vectors, not adding.");
            }

            QUAT5Section quatSection = new QUAT5Section();
            if (!await quatSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build QUAT5 section, returning null.");
                return null;
            }
            else if (quatSection.Quaternions.Count > 0)
            {
                Logger?.Here().Verbose("Adding QUAT5 section.");
                retList.Add(quatSection);
            }
            else
            {
                Logger?.Here().Verbose("QUAT5 section had no quaternions, not adding.");
            }

            CUST5Section custSection = new CUST5Section();
            if (!await custSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build CUST5 section, returning null.");
                return null;
            }
            else if (custSection.Figures.Count > 0)
            {
                Logger?.Here().Verbose("Adding CUST5 section.");
                retList.Add(custSection);
            }
            else
            {
                Logger?.Here().Verbose("CUST5 section had no figures, not adding.");
            }

            return retList;
        }
    }
}
