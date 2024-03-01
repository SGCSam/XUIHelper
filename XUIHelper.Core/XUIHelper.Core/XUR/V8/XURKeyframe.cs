using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XURKeyframe
    {
        public ulong Keyframe { get; private set; }
        public XUKeyframeInterpolationTypes InterpolationType { get; private set; }
        public byte EaseIn { get; private set; }
        public byte EaseOut { get; private set; }
        public byte EaseScale { get; private set; }
        public ulong VectorIndex { get; private set; }
        public ulong PropertyIndex { get; private set; } 

        public XURKeyframe(ulong keyframe, XUKeyframeInterpolationTypes interpolationType, byte easeIn, byte easeOut, byte easeScale, ulong vectIndex, ulong propIndex)
        {
            Keyframe = keyframe;
            InterpolationType = interpolationType;
            EaseIn = easeIn;
            EaseOut = easeOut;
            EaseScale = easeScale;
            PropertyIndex = propIndex;
        }

        public override string ToString()
        {
            return string.Format("Keyframe: {0}, Interp: {1}, In: {2}, Out: {3}, Scale: {4}, Vector: {5}, Property: {6}", Keyframe, InterpolationType, EaseIn, EaseOut, EaseScale, VectorIndex, PropertyIndex);
        }
    }
}
