using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public enum XUNamedFrameCommandTypes
    {
        Play,
        Stop,
        GoTo,
        GoToAndPlay,
        GoToAndStop,
        Count
    }

    public class XUNamedFrame
    {
        public string Name { get; private set; } = string.Empty;
        public int Keyframe { get; private set; }
        public XUNamedFrameCommandTypes CommandType { get; private set; }
        public string TargetParameter { get; private set; } = string.Empty;

        public XUNamedFrame(string name, int keyframe, XUNamedFrameCommandTypes commandType)
        {
            Name = name;
            Keyframe = keyframe;
            CommandType = commandType;
        }

        public XUNamedFrame(string name, int keyframe, XUNamedFrameCommandTypes commandType, string targetParam)
        {
            Name = name;
            Keyframe = keyframe;
            CommandType = commandType;
            TargetParameter = targetParam;
        }

        public override string ToString()
        {
            if(TargetParameter == string.Empty)
            {
                return string.Format("Name: {0}, Keyframe: {1}, Command: {2}", Name, Keyframe, CommandType);
            }
            else
            {
                return string.Format("Name: {0}, Keyframe: {1}, Command: {2}, Target: {3}", Name, Keyframe, CommandType, TargetParameter);
            }
        }
    }
}
