using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUBezierPoint
    {
        //Point is the X, Y location of some point of the shape
        //Control points one and two are for the bezier curve

        public XUPoint Point { get; private set; } = new XUPoint();
        public XUPoint ControlPointOne { get; private set; } = new XUPoint();
        public XUPoint ControlPointTwo { get; private set; } = new XUPoint();

        public XUBezierPoint(XUPoint point, XUPoint controlPointOne, XUPoint controlPointTwo)
        {
            Point = point;
            ControlPointOne = controlPointOne;
            ControlPointTwo = controlPointTwo;
        }

        public XUBezierPoint()
        {

        }

        public override string ToString()
        {
            return string.Format("Point: ({0}), Control Point One: ({1}), Control Point Two: ({2})", Point, ControlPointOne, ControlPointTwo);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not XUBezierPoint xuBezierPoint)
            {
                return false;
            }

            return xuBezierPoint.Point.Equals(Point) && xuBezierPoint.ControlPointOne.Equals(ControlPointOne) && xuBezierPoint.ControlPointTwo.Equals(ControlPointTwo);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Point.GetHashCode(), ControlPointOne.GetHashCode(), ControlPointTwo.GetHashCode());
        }
    }
}
