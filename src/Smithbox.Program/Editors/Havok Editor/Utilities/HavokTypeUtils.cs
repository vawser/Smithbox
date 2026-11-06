using Havok;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public static class HavokTypeUtils
{
    /// <summary>
    /// The unified Havok library's profile for the project's game (its registry overlay and write defaults), or null
    /// if the game has none. The SDK of each file still comes from the file.
    /// </summary>
    public static HavokProfile GetProfile(ProjectEntry project)
    {
        return HavokProfile.FromGame(project.Descriptor.ProjectType.ToString());
    }

    /// <summary>
    /// The SDK of the project's own Havok files (a few files of another SDK can still turn up, e.g. ER's 2016 files)
    /// </summary>
    public static HavokSdk GetSdk(ProjectEntry project)
    {
        return GetProfile(project)?.Sdk ?? HavokSdk.Unknown;
    }

    /// <summary>
    /// The value of a field of a Havok object by its name (<c>m_name</c>), for code that is the same for every SDK's
    /// classes. Null if the object has no such field.
    /// </summary>
    public static object GetFieldValue(object havokObject, string fieldName)
    {
        return havokObject?.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(havokObject);
    }

    /// <summary>
    /// The m_name of a Havok object, or "" if it has none
    /// </summary>
    public static string GetName(object havokObject)
    {
        return GetFieldValue(havokObject, "m_name") as string ?? "";
    }

    /// <summary>
    /// Whether the object's class is the Havok class of this name (in any SDK's class set), or derives from it
    /// </summary>
    public static bool IsHavokClass(object havokObject, string className)
    {
        for (var type = havokObject?.GetType(); type != null && type != typeof(object); type = type.BaseType)
        {
            if (type.Name == className)
                return true;
        }

        return false;
    }
}
