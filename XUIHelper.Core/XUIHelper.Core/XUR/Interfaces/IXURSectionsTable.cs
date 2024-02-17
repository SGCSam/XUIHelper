﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXURSectionsTable : IXURReadable
    {
        short SectionsCount { get; }

        List<XURSectionTableEntry> Entries { get; }
    }
}
