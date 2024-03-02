using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR8 : XUR
    {
        public List<XURCompoundPropertyData> CompoundPropertyDatas { get; set; } = new List<XURCompoundPropertyData>();
        public List<List<XUProperty>> ReadPropertiesLists { get; set; } = new List<List<XUProperty>>();

        public XUR8(string filePath, ILogger? logger = null) : base(filePath, new XUR8Header(), new XUR8SectionsTable(), logger)
        {

        }

        protected override IXURSection? TryCreateXURSectionForMagic(int Magic)
        {
            try
            {
                Logger = Logger?.ForContext(typeof(XUR8));

                Logger?.Here().Verbose("Trying to get XUR section for magic {0:X8}", Magic);

                switch (Magic)
                {
                    case ISTRNSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning STRN8 section.");
                        return new STRN8Section();
                    }
                    case IVECTSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning VECT8 section.");
                        return new VECT8Section();
                    }
                    case IQUATSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning QUAT8 section.");
                        return new QUAT8Section();
                    }
                    case ICUSTSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning CUST8 section.");
                        return new CUST8Section();
                    }
                    case IFLOTSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning FLOT8 section.");
                        return new FLOT8Section();
                    }
                    case ICOLRSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning COLR8 section.");
                        return new COLR8Section();
                    }
                    case IKEYPSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning KEYP8 section.");
                        return new KEYP8Section();
                    }
                    case IKEYDSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning KEYD8 section.");
                        return new KEYD8Section();
                    }
                    case INAMESection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning NAME8 section.");
                        return new NAME8Section();
                    }
                    case IDATASection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning DATA8 section.");
                        return new DATA8Section();
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
            return new XUR8CountHeader();
        }

        protected override bool HasCountHeader()
        {
            return true;
        }
    }
}
