using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialUtils
{
    public static bool SupportsMATBIN(ProjectEntry project)
    {
        if(project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            return true;
        }

        return false;
    }
}
