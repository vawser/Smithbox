using StudioCore.Core.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor.Utils;

public static class EsdUtils
{
    public static bool SupportsEditor()
    {
        if(Smithbox.ProjectType is ProjectType.AC6)
        {
            return true;
        }

        return false;
    }
}
