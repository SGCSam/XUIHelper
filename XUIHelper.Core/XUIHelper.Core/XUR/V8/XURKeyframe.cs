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
        public int VectorIndex { get; private set; }
        public int PropertyIndex { get; private set; }
        public byte Unknown4 { get; private set; }
        public byte Unknown10 { get; private set; }
        public byte Unknown20 { get; private set; }

        public XURKeyframe(int keyframe, XUKeyframeInterpolationTypes interpolationType, byte easeIn, byte easeOut, byte easeScale, int vectIndex, int propIndex)
        {
            Keyframe = keyframe;
            InterpolationType = interpolationType;
            EaseIn = easeIn;
            EaseOut = easeOut;
            EaseScale = easeScale;
            VectorIndex = vectIndex;
            PropertyIndex = propIndex;
        }

        public XURKeyframe(int keyframe, XUKeyframeInterpolationTypes interpolationType, byte easeIn, byte easeOut, byte easeScale, int vectIndex, int propIndex, byte unk4, byte unk10, byte unk20)
        {
            Keyframe = keyframe;
            InterpolationType = interpolationType;
            EaseIn = easeIn;
            EaseOut = easeOut;
            EaseScale = easeScale;
            VectorIndex = vectIndex;
            PropertyIndex = propIndex;
            Unknown4 = unk4;
            Unknown10 = unk10;
            Unknown20 = unk20;
        }

        public XURKeyframe(XUKeyframe keyframe, int vectIndex, int propIndex)
        {
            Keyframe = keyframe.Keyframe;
            InterpolationType = keyframe.InterpolationType;
            EaseIn = keyframe.EaseIn;
            EaseOut = keyframe.EaseOut;
            EaseScale = keyframe.EaseScale;
            VectorIndex = vectIndex;
            PropertyIndex = propIndex;
        }

        public override string ToString()
        {
            return string.Format("Keyframe: {0}, Interp: {1}, In: {2}, Out: {3}, Scale: {4}, Vector: {5}, Property: {6}", Keyframe, InterpolationType, EaseIn, EaseOut, EaseScale, VectorIndex, PropertyIndex);
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
                VectorIndex == other.VectorIndex &&
                PropertyIndex == other.PropertyIndex;
        }
    }
}
