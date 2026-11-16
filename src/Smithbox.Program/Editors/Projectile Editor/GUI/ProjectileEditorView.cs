#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The Projectile Editor's one view: the row list, the properties of the selected row and the rows it uses, the
/// simulation's settings and playback, and the viewport the simulation plays in.
/// </summary>
public class ProjectileEditorView
{
    public ProjectileEditorScreen Editor;
    public ProjectEntry Project;

    public RenderScene? RenderScene;
    public ViewportSelection ViewportSelection = new();
    public ViewportActionManager ViewportActionManager = new();
    public ProjectileUniverse Universe;
    public ProjectileViewportWindow ViewportWindow;

    public ProjectileSimulator Simulator;
    public ProjectileEffects Effects;

    public ProjectileListView ListView;
    public ProjectilePropertiesView PropertiesView;
    public ProjectileToolWindow ToolWindow;

    public ProjectileEditorView(ProjectileEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        RenderScene = Smithbox.Instance._context.Device != null ? new RenderScene() : null;
        Universe = new ProjectileUniverse(this, project);
        ViewportWindow = new ProjectileViewportWindow(this, project);

        Simulator = new ProjectileSimulator(project);
        Effects = new ProjectileEffects(this);

        ListView = new ProjectileListView(this);
        PropertiesView = new ProjectilePropertiesView(this);
        ToolWindow = new ProjectileToolWindow(this);
    }

    /// <summary>The param the bullets of the selected row are in: its own, or a spell's bullet param</summary>
    public Param? GetBulletParam()
    {
        if (Simulator.ParamName == null)
            return null;

        return Simulator.IsSpell
            ? ProjectileParams.GetSpellBulletParam(Project)
            : ProjectileParams.GetParam(Project, Simulator.ParamName);
    }

    /// <summary>Shows a row of a bullet or spell param. False if there is no such row.</summary>
    public bool Select(string paramName, int rowId)
    {
        if (!ProjectileParams.IsProjectileParam(Project, paramName))
            return false;

        if (ProjectileParams.GetParam(Project, paramName)?[rowId] is not { } row)
            return false;

        Select(paramName, row);
        return true;
    }

    public void Select(string paramName, Param.Row row)
    {
        var framing = Simulator.Row == null;

        ListView.ShowParam(paramName);
        ListView.SelectOnly(row);
        ListView.ScrollToSelection = true;
        Simulator.Select(paramName, row);

        // The first row shown is framed; after that the camera stays where it was put, unless set to follow
        if (framing || CFG.Current.ProjectileEditor_Frame_On_Select)
            ViewportWindow.FrameSimulation();
    }

    // ---- windows

    public void Display(uint dockspaceId)
    {
        if (CFG.Current.Interface_ProjectileEditor_List)
            Window($"{LOC.Get("PROJECTILE_Window_List")}{ListWindowId}", dockspaceId, EditorFocusContext.ProjectileEditor_List, ListView.Display);

        if (CFG.Current.Interface_ProjectileEditor_Properties)
            Window($"{LOC.Get("PROJECTILE_Window_Properties")}{PropertiesWindowId}", dockspaceId, EditorFocusContext.ProjectileEditor_Properties, PropertiesView.Display);

        // Playback and Simulation Filters
        if (CFG.Current.Interface_ProjectileEditor_ToolWindow)
            Window($"{LOC.Get("PROJECTILE_Window_Tools")}{ToolWindowId}", dockspaceId, EditorFocusContext.ProjectileEditor_Tools, ToolWindow.Display);

        // The viewport window sets the focus itself
        ViewportWindow.Display(dockspaceId);
    }

    public const string ListWindowId = "###projectileEditor_List";
    public const string PropertiesWindowId = "###projectileEditor_Properties";
    public const string ToolWindowId = "###projectileEditor_Tools";

    private static void Window(string title, uint dockspace, EditorFocusContext focus, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectileEditorView);

        if (ImGui.Begin(title, GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) ||
                ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                FocusManager.SetFocus(focus);
            }

            draw();
        }

        ImGui.End();
    }

    // ---- frame

    public void Update(float dt)
    {
        // The clock waits while the bullets' effects load (read last frame), so they don't stall as they first show
        Simulator.Update(Effects.IsLoading ? 0 : dt);

        // The lighting the viewport borrows, once there is something to light (as the FX Editor does)
        if (Simulator.Result != null)
            Project.Handler.ViewportEnvironment.Update();

        ViewportWindow.Update(dt);

        // After the viewport, so the effects face and sort from the camera it shows
        var (eye, forward) = ViewportWindow.CameraEyeAndForward();
        Effects.Update(eye, forward);
    }

    /// <summary>Stops playing when the editor isn't shown, as nothing updates it then</summary>
    public void Suspend()
    {
        Simulator.Playing = false;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewportWindow.Viewport is VulkanViewport { Visible: true })
            ViewportWindow.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ViewportWindow.InputCaptured();
    }

    public void Dispose()
    {
        Effects.Dispose();
        ViewportWindow.Dispose();
    }
}
