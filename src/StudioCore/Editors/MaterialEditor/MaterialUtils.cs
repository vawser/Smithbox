using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

public class MaterialUtils
{
    public static bool SupportsMATBIN(ProjectEntry project)
    {
        if(project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            return true;
        }

        return false;
    }
}
