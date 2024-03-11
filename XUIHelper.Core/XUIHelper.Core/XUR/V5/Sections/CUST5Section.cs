using Microsoft.VisualBasic;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class CUST5Section : ICUSTSection
    {
        public int Magic { get { return ICUSTSection.ExpectedMagic; } }

        public List<XUFigure> Figures { get; private set; } = new List<XUFigure>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(CUST5Section));
                xur.Logger?.Here().Verbose("Reading CUST5 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ICUSTSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading customs from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                int custIndex = 0;
                for (bytesRead = 0; bytesRead < entry.Length;)
                {
                    xur.Logger?.Here().Verbose("Reading custom at index {0}, offset {1:X8}", custIndex, bytesRead);

                    int dataLength = reader.ReadInt32BE();
                    xur.Logger?.Here().Verbose("Got a custom data length of {0:X8}", dataLength);
                    bytesRead += 4;

                    int dataRead = 0;
                    float boundingBoxX = reader.ReadSingleBE();
                    float boundingBoxY = reader.ReadSingleBE();
                    dataRead += 8;
                    XUPoint boundingBox = new XUPoint(boundingBoxX, boundingBoxY);
                    xur.Logger?.Here().Verbose("Got a bounding box of {0}", boundingBox);

                    int bezierPointsCount = reader.ReadInt32BE();
                    dataRead += 4;
                    xur.Logger?.Here().Verbose("Got a bezier points count of {0:X8}", bezierPointsCount);

                    List<XUBezierPoint> bezierPoints = new List<XUBezierPoint>();
                    for (int pointsIndex = 0; pointsIndex < bezierPointsCount; pointsIndex++)
                    {
                        xur.Logger?.Here().Verbose("Reading bezier point index {0}", pointsIndex);

                        float vectX = reader.ReadSingleBE();
                        float vectY = reader.ReadSingleBE();
                        dataRead += 8;
                        XUPoint vectPoint = new XUPoint(vectX, vectY);
                        xur.Logger?.Here().Verbose("Got vector point of {0}", vectPoint);

                        float oneX = reader.ReadSingleBE();
                        float oneY = reader.ReadSingleBE();
                        dataRead += 8;
                        XUPoint controlOnePoint = new XUPoint(oneX, oneY);
                        xur.Logger?.Here().Verbose("Got control one point of {0}", controlOnePoint);

                        float twoX = reader.ReadSingleBE();
                        float twoY = reader.ReadSingleBE();
                        dataRead += 8;
                        XUPoint controlTwoPoint = new XUPoint(twoX, twoY);
                        xur.Logger?.Here().Verbose("Got control two point of {0}", controlTwoPoint);

                        bezierPoints.Add(new XUBezierPoint(vectPoint, controlOnePoint, controlTwoPoint));
                    }

                    if(dataRead != dataLength)
                    {
                        xur.Logger?.Here().Error("Mismatch between the amount of data read and the length, returning false. Expected: {0:X8}, Actual: {1:X8}", dataLength, dataRead);
                        return false;
                    }

                    bytesRead += dataRead;
                    Figures.Add(new XUFigure(boundingBox, bezierPoints));
                    xur.Logger?.Here().Verbose("Read custom at index {0} successfully!", custIndex);
                    custIndex++;
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading CUST5 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Building CUST5 figures.");
                HashSet<XUFigure> builtFigs = new HashSet<XUFigure>();
                if (!TryBuildFiguresFromObject(xur, xuObject, ref builtFigs))
                {
                    xur.Logger?.Here().Error("Failed to build figures, returning null.");
                    return false;
                }

                Figures = builtFigs.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} CUST5 figures successfully!", Figures.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build CUST5 figures, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildFiguresFromObject(IXUR xur, XUObject xuObject, ref HashSet<XUFigure> builtFigs)
        {
            try
            {
                if (!TryBuildFiguresFromProperties(xur, xuObject.Properties, ref builtFigs))
                {
                    xur.Logger?.Here().Error("Failed to build figures from properties for {0}, returning false.", xuObject.ClassName);
                    return false;
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildFiguresFromObject(xur, childObject, ref builtFigs))
                    {
                        xur.Logger?.Here().Error("Failed to get figures for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        foreach (XUProperty animatedProperty in childKeyframe.Properties)
                        {
                            if (animatedProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Custom)
                            {
                                if (animatedProperty.Value is not XUFigure valueFigure)
                                {
                                    xur.Logger?.Here().Error("Animated property {0} marked as custom had a non-custom value of {1}, returning false.", animatedProperty.PropertyDefinition.Name, animatedProperty.Value);
                                    return false;
                                }

                                if (builtFigs.Add(valueFigure))
                                {
                                    xur.Logger?.Here().Verbose("Added {0} animated property value figure {1}.", animatedProperty.PropertyDefinition.Name, valueFigure);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build CUST5 figures for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        private bool TryBuildFiguresFromProperties(IXUR xur, List<XUProperty> properties, ref HashSet<XUFigure> builtFigs)
        {
            try
            {
                foreach (XUProperty childProperty in properties)
                {
                    if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Custom)
                    {
                        if (childProperty.Value is not XUFigure valueFigure)
                        {
                            xur.Logger?.Here().Error("Child property {0} marked as custom had a non-custom value of {1}, returning false.", childProperty.PropertyDefinition.Name, childProperty.Value);
                            return false;
                        }

                        if (builtFigs.Add(valueFigure))
                        {
                            xur.Logger?.Here().Verbose("Added {0} property value figure {1}.", childProperty.PropertyDefinition.Name, valueFigure);
                        }
                    }
                    else if (childProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Object)
                    {
                        if (!TryBuildFiguresFromProperties(xur, childProperty.Value as List<XUProperty>, ref builtFigs))
                        {
                            xur.Logger?.Here().Error("Failed to build figures for child compound properties, returning false.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build CUST5 figures from properties, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(CUST5Section));
                xur.Logger?.Here().Verbose("Writing CUST section.");

                int bytesWritten = 0;
                int figuresWritten = 0;
                foreach (XUFigure figure in Figures)
                {
                    int thisDataLength = 12 + (24 * figure.Points.Count);
                    writer.WriteInt32BE(thisDataLength);
                    xur.Logger?.Here().Verbose("Wrote a data length of {0:X8} for figure index {1} that has a total of {2} points.", thisDataLength, figuresWritten, figure.Points.Count);

                    writer.WriteSingleBE(figure.BoundingBox.X);
                    writer.WriteSingleBE(figure.BoundingBox.Y);
                    writer.WriteInt32BE(figure.Points.Count);
                    bytesWritten += 16;

                    foreach(XUBezierPoint bezierPoint in figure.Points)
                    {
                        writer.WriteSingleBE(bezierPoint.Point.X);
                        writer.WriteSingleBE(bezierPoint.Point.Y);
                        writer.WriteSingleBE(bezierPoint.ControlPointOne.X);
                        writer.WriteSingleBE(bezierPoint.ControlPointOne.Y);
                        writer.WriteSingleBE(bezierPoint.ControlPointTwo.X);
                        writer.WriteSingleBE(bezierPoint.ControlPointTwo.Y);
                        bytesWritten += 24;
                    }

                    xur.Logger?.Here().Verbose("Wrote figure index {0}: {1}.", figuresWritten, figure);
                    figuresWritten++;
                }

                xur.Logger?.Here().Verbose("Wrote a total of {0} CUST5 customs as {1:X8} bytes successfully!", Figures.Count, bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing CUST5 section, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
