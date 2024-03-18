using Serilog;
using System;
using System.Collections;
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

        public int GetSharedCompoundPropertiesCount()
        {
            int retCount = 0;
            foreach (XURCompoundPropertyData item in CompoundPropertyDatas)
            {
                retCount += item.Properties.Count;
                foreach(XUProperty property in item.Properties)
                {
                    if(property.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if(property.Value is IList list)
                        {
                            retCount += list.Count + 1;
                        }
                        else
                        {
                            retCount++;
                        }
                    }
                }
            }

            return retCount;
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

        protected override bool HasReadableCountHeader()
        {
            return true;
        }

        protected override bool ShouldWriteCountHeader(XUObject rootObject)
        {
            return true;
        }

        protected override async Task<bool> TryBuildSectionsFromObjectAsync(XUObject rootObject)
        {
            STRN8Section strnSection = new STRN8Section();
            if (!await strnSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build STRN8 section, returning false.");
                return false;
            }
            else if (strnSection.Strings.Count > 0)
            {
                Logger?.Here().Verbose("Adding STRN8 section.");
                Sections.Add(strnSection);
            }
            else
            {
                Logger?.Here().Verbose("STRN8 section had no strings, not adding.");
            }

            VECT8Section vectSection = new VECT8Section();
            if (!await vectSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build VECT8 section, returning false.");
                return false;
            }
            else if (vectSection.Vectors.Count > 0)
            {
                Logger?.Here().Verbose("Adding VECT8 section.");
                Sections.Add(vectSection);
            }
            else
            {
                Logger?.Here().Verbose("VECT8 section had no vectors, not adding.");
            }

            QUAT8Section quatSection = new QUAT8Section();
            if (!await quatSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build QUAT8 section, returning false.");
                return false;
            }
            else if (quatSection.Quaternions.Count > 0)
            {
                Logger?.Here().Verbose("Adding QUAT8 section.");
                Sections.Add(quatSection);
            }
            else
            {
                Logger?.Here().Verbose("QUAT8 section had no quaternions, not adding.");
            }

            CUST8Section custSection = new CUST8Section();
            if (!await custSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build CUST8 section, returning false.");
                return false;
            }
            else if (custSection.Figures.Count > 0)
            {
                Logger?.Here().Verbose("Adding CUST8 section.");
                Sections.Add(custSection);
            }
            else
            {
                Logger?.Here().Verbose("CUST8 section had no figures, not adding.");
            }

            FLOT8Section flotSection = new FLOT8Section();
            if (!await flotSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build FLOT8 section, returning false.");
                return false;
            }
            else if (flotSection.Floats.Count > 0)
            {
                Logger?.Here().Verbose("Adding FLOT8 section.");
                Sections.Add(flotSection);
            }
            else
            {
                Logger?.Here().Verbose("FLOT8 section had no floats, not adding.");
            }

            COLR8Section colrSection = new COLR8Section();
            if (!await colrSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build COLR8 section, returning false.");
                return false;
            }
            else if (colrSection.Colours.Count > 0)
            {
                Logger?.Here().Verbose("Adding COLR8 section.");
                Sections.Add(colrSection);
            }
            else
            {
                Logger?.Here().Verbose("COLR8 section had no colours, not adding.");
            }

            KEYP8Section keypSection = new KEYP8Section();
            if (!await keypSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build KEYP8 section, returning false.");
                return false;
            }
            else if (keypSection.PropertyIndexes.Count > 0)
            {
                Logger?.Here().Verbose("Adding KEYP8 section.");
                Sections.Add(keypSection);
            }
            else
            {
                Logger?.Here().Verbose("KEYP8 section had no indexes, not adding.");
            }

            KEYD8Section keydSection = new KEYD8Section();
            if (!await keydSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build KEYD8 section, returning false.");
                return false;
            }
            else if (keydSection.Keyframes.Count > 0)
            {
                Logger?.Here().Verbose("Adding KEYD8 section.");
                Sections.Add(keydSection);
            }
            else
            {
                Logger?.Here().Verbose("KEYD8 section had no keyframes, not adding.");
            }

            NAME8Section nameSection = new NAME8Section();
            if (!await nameSection.TryBuildAsync(this, rootObject))
            {
                Logger?.Here().Error("Failed to build NAME8 section, returning false.");
                return false;
            }
            else if (nameSection.NamedFrames.Count > 0)
            {
                Logger?.Here().Verbose("Adding NAME8 section.");
                Sections.Add(nameSection);
            }
            else
            {
                Logger?.Here().Verbose("NAME8 section had no named frames, not adding.");
            }

            DATA5Section dataSection = new DATA5Section(rootObject);
            Sections.Add(dataSection);

            return true;
        }
    }
}
