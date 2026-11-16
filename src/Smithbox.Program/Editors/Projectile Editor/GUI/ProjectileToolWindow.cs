#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The Projectile Editor's Tools window (see Claude/Documentation/Tool UI Styling.md):
/// General → Playback (with the simulation's setup), Simulation Filters.
/// </summary>
public class ProjectileToolWindow
{
    public ProjectilePlaybackTool PlaybackTool;
    public ProjectileSimulationFiltersTool SimulationFiltersTool;

    private readonly ToolSections Sections = new();

    public ProjectileToolWindow(ProjectileEditorView view)
    {
        PlaybackTool = new ProjectilePlaybackTool(view);
        SimulationFiltersTool = new ProjectileSimulationFiltersTool(view);
    }

    public void Display()
    {
        Sections.Begin();

        // General
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        if (CFG.Current.Interface_ProjectileEditor_Tool_Playback)
            PlaybackTool.OnToolWindow();

        if (CFG.Current.Interface_ProjectileEditor_Tool_SimulationFilters)
            SimulationFiltersTool.OnToolWindow();

        Sections.End();
    }
}
