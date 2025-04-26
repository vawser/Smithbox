using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor.Utils;

public static class CutsceneUtils
{
    public static bool SupportsEditor()
    {
        if (Smithbox.ProjectType is
            ProjectType.DS1 or
            ProjectType.DS1R or
            ProjectType.BB or
            ProjectType.DS2S or
            ProjectType.DS2 or
            ProjectType.AC4 or
            ProjectType.ACFA)
            return false;

        return true;
    }
}
