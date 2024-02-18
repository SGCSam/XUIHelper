using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public enum XUKeyframeInterpolationTypes
    { 
        Linear,
        None,
        Ease
    }

    public class XUKeyframe
    {
        public int Keyframe { get; private set; }
        public XUKeyframeInterpolationTypes InterpolationType { get; private set; }
        public byte EaseIn { get; private set; }
        public byte EaseOut { get; private set; }
        public byte EaseScale { get; private set; }

        public XUKeyframe(int keyframe, XUKeyframeInterpolationTypes interpolationType, byte easeIn, byte easeOut, byte easeScale)
        {
            Keyframe = keyframe;
            InterpolationType = interpolationType;
            EaseIn = easeIn;
            EaseOut = easeOut;
            EaseScale = easeScale;
        }

        public XUKeyframe()
        {

        }
    }
}
