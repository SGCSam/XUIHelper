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
        GotoAndStop,
        Count
    }

    public class XUNamedFrame
    {
        public string Name { get; private set; } = string.Empty;
        public int Keyframe { get; private set; }
        public XUNamedFrameCommandTypes CommandType { get; private set; }

        public XUNamedFrame(string name, int keyframe, XUNamedFrameCommandTypes commandType)
        {
            Name = name;
            Keyframe = keyframe;
            CommandType = commandType;
        }
    }
}
