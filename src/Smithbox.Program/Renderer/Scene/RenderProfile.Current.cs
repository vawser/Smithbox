using StudioCore.Application;

namespace StudioCore.Renderer;

public sealed partial class RenderProfile
{
    /// <summary>The profile of the selected project, or a modern game's when none is selected</summary>
    public static RenderProfile Current =>
        For(Smithbox.Orchestrator?.SelectedProject?.Descriptor.ProjectType ?? ProjectType.Undefined);

    /// <summary>The profile of a project</summary>
    public static RenderProfile Of(ProjectEntry project) =>
        For(project?.Descriptor.ProjectType ?? ProjectType.Undefined);
}
