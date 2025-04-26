using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor.Utils
{
    public static class ModelEditorUtils
    {
        /// <summary>
        /// Whether or not the specified project type is supported.
        /// </summary>
        /// <param name="projectType">The project type to check.</param>
        /// <returns>Whether or not the <see cref="ProjectType"/> is supported.</returns>
        public static bool IsSupportedProjectType(ProjectType projectType)
        {
            return !(projectType is ProjectType.DES or ProjectType.AC4 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD);
        }
    }
}
