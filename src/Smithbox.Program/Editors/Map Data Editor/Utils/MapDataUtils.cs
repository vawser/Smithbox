using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapParamEditor;

public static class MapDataUtils
{
    public static bool SupportsMSB(ProjectEntry curProject)
    {
        return true;
    }

    public static bool SupportsENFL(ProjectEntry curProject)
    {
        if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            return true;
        }

        return false;
    }
}
