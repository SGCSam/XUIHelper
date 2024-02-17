using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUFigure
    {
        public XUPoint BoundingBox { get; private set; } = new XUPoint();
        public List<XUBezierPoint> Points { get; private set; } = new List<XUBezierPoint>();

        public XUFigure(XUPoint boundingBox, List<XUBezierPoint> points)
        {
            BoundingBox = boundingBox;
            Points = points;
        }

        public XUFigure()
        {

        }

        public override string ToString()
        {
            return string.Format("Bounding Box: ({0}), Points: ({1}),", BoundingBox.ToString(), string.Join(",", Points).TrimEnd());
        }
    }
}
