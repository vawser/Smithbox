using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The Projectile Editor's viewport scene: the simulation's lines and the bullets' effects, lit by the shared viewport
/// environment.
/// </summary>
public class ProjectileUniverse : IUniverse
{
    public ProjectileEditorView View;
    public ProjectEntry Project;

    public RenderScene RenderScene => View.RenderScene;
    public ViewportSelection ViewportSelection => View.ViewportSelection;
    public ViewportActionManager ViewportActionManager => View.ViewportActionManager;

    public ProjectileUniverse(ProjectileEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
}
