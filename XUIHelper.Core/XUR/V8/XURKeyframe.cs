using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XURKeyframe
    {
        public int Keyframe { get; private set; }
        public XUKeyframeInterpolationTypes InterpolationType { get; private set; }
        public byte EaseIn { get; private set; }
        public byte EaseOut { get; private set; }
        public byte EaseScale { get; private set; }
        public int PropertyIndex { get; private set; }

        public XURKeyframe(int keyframe, XUKeyframeInterpolationTypes interpolationType, byte easeIn, byte easeOut, byte easeScale, int propIndex)
        {
            Keyframe = keyframe;
            InterpolationType = interpolationType;
            EaseIn = easeIn;
            EaseOut = easeOut;
            EaseScale = easeScale;
            PropertyIndex = propIndex;
        }

        public XURKeyframe(XUKeyframe keyframe, int propIndex)
        {
            Keyframe = keyframe.Keyframe;
            InterpolationType = keyframe.InterpolationType;
            EaseIn = keyframe.EaseIn;
            EaseOut = keyframe.EaseOut;
            EaseScale = keyframe.EaseScale;
            PropertyIndex = propIndex;
        }

        public override string ToString()
        {
            return string.Format("Keyframe: {0}, Interp: {1}, In: {2}, Out: {3}, Scale: {4}, Property: {5}", Keyframe, InterpolationType, EaseIn, EaseOut, EaseScale, PropertyIndex);
        }

        public override bool Equals(object? obj)
        {
            if(obj is not XURKeyframe other)
            {
                return false;
            }

            return Keyframe == other.Keyframe &&
                InterpolationType == other.InterpolationType &&
                EaseIn == other.EaseIn &&
                EaseOut == other.EaseOut &&
                EaseScale == other.EaseScale &&
                PropertyIndex == other.PropertyIndex;
        }
    }
}
