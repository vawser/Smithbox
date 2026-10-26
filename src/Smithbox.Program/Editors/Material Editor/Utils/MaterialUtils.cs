using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

public static class MaterialUtils
{
    public static bool SupportsMATBIN(ProjectEntry project)
    {
        if(project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            return true;
        }

        return false;
    }
}
