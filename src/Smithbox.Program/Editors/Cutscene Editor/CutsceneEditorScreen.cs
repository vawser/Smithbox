#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The Cutscene Editor (Cutscene Editor Analysis, phase 2: the viewer). Plays a cut of an MQB cutscene in a viewport:
/// its actors on their models, animated by the cut's Havok file, and its camera, which the viewport can look through.
/// Nothing is edited yet, so there is nothing to save.
/// </summary>
public class CutsceneEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public RenderScene? RenderScene;
    public ViewportSelection ViewportSelection = new();
    public ViewportActionManager ViewportActionManager = new();
    public CutsceneUniverse Universe;
    public CutsceneViewportWindow ViewportWindow;

    public CutscenePlayback Playback = new();

    public CutsceneListView ListView;
    public CutsceneTimelineView TimelineView;
    public CutscenePropertiesView PropertiesView;

    /// <summary>The cutscene being shown, and which of its cuts</summary>
    public CutsceneFile? SelectedFile { get; private set; }
    public int SelectedCut { get; private set; }

    /// <summary>The event picked in the timeline, shown in the properties</summary>
    public MQB.Event? SelectedEvent;
    public int SelectedTimeline = -1;

    /// <summary>The cut being shown, null until it is built</summary>
    public CutsceneScene? Scene { get; private set; }

    private Task<CutsceneScene?>? SceneTask;
    private (CutsceneFile File, int Cut)? SceneTaskFor;
    public bool IsBuilding => SceneTask is { IsCompleted: false };
    public string? BuildError { get; private set; }

    public CutsceneEditorScreen(ProjectEntry project)
    {
        Project = project;

        RenderScene = Smithbox.Instance._context.Device != null ? new RenderScene() : null;
        Universe = new CutsceneUniverse(this, project);
        ViewportWindow = new CutsceneViewportWindow(this, project);

        ListView = new CutsceneListView(this);
        TimelineView = new CutsceneTimelineView(this);
        PropertiesView = new CutscenePropertiesView(this);
    }

    public CutsceneData Data => Project.Handler.CutsceneData;

    public string EditorName => "Cutscene Editor";
    public string CommandEndpoint => "cutscene";
    public string SaveType => "Cutscene";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    // ---- selection

    /// <summary>Shows a cut of a cutscene, building it on a worker thread.</summary>
    public void Select(CutsceneFile file, int cut)
    {
        if (SelectedFile == file && SelectedCut == cut && (Scene != null || IsBuilding))
            return;

        SelectedFile = file;
        SelectedCut = cut;
        SelectedEvent = null;
        SelectedTimeline = -1;
        BuildError = null;

        var project = Project;
        SceneTaskFor = (file, cut);
        SceneTask = Task.Run<CutsceneScene?>(() =>
        {
            if (!file.Load(project))
                return null;

            var mqb = file.Mqb!;
            if (mqb.Cuts.Count == 0)
                return null;

            return CutsceneScene.Prepare(project, file, Math.Clamp(cut, 0, mqb.Cuts.Count - 1));
        });
    }

    public void SelectCut(int cut)
    {
        if (SelectedFile?.Mqb is { } mqb && cut >= 0 && cut < mqb.Cuts.Count)
            Select(SelectedFile, cut);
    }

    private void ApplyBuiltScene()
    {
        if (SceneTask is not { IsCompleted: true } task)
            return;

        var forSelection = SceneTaskFor;
        SceneTask = null;
        SceneTaskFor = null;

        CutsceneScene? built = task.IsCompletedSuccessfully ? task.Result : null;

        // Another cut was picked while this one was being built
        if (forSelection is not { } f || f.File != SelectedFile || f.Cut != SelectedCut)
        {
            built?.Dispose();
            return;
        }

        if (built == null)
        {
            BuildError = task.Exception?.GetBaseException().Message ?? SelectedFile?.LoadError ?? LOC.Get("CUTSCENE_Build_Failed");
            if (task.Exception != null)
                Smithbox.LogError(this, LOC.Get("CUTSCENE_Build_Failed"), task.Exception.GetBaseException());
            return;
        }

        ClearScene();
        Scene = built;
        Scene.StartLoads(Project, RenderScene);

        var keepPlaying = Playback.Playing;
        Playback.Restart();
        Playback.Playing = keepPlaying;

        ViewportWindow.OnSceneChanged();

        if (CFG.Current.CutsceneEditor_Diagnostics_Log)
            Smithbox.Log(this, Scene.Describe());
    }

    private void ClearScene()
    {
        Scene?.Dispose();
        Scene = null;
        ViewportWindow.OnSceneChanged();
    }

    /// <summary>Builds the cut again, for options that change how it is built (models on or off)</summary>
    public void Rebuild()
    {
        if (SelectedFile == null)
            return;

        var file = SelectedFile;
        var cut = SelectedCut;
        ClearScene();
        SelectedFile = null;
        Select(file, cut);
    }

    // ---- EditorScreen

    public void OnGUI(string[] commands)
    {
        HandleCommands(commands);

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();
            CameraMenu();

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();
                ImGui.Text(LOC.Get("SYS_OpenGL_Viewport_Blocked"));
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_CutsceneEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_CutsceneEditor);

        Window($"{LOC.Get("CUTSCENE_Window_List")}###cutsceneEditor_List", dsid, EditorFocusContext.CutsceneEditor_List, ListView.Display);
        Window($"{LOC.Get("CUTSCENE_Window_Timeline")}###cutsceneEditor_Timeline", dsid, EditorFocusContext.CutsceneEditor_Timeline, TimelineView.Display);
        Window($"{LOC.Get("CUTSCENE_Window_Properties")}###cutsceneEditor_Properties", dsid, EditorFocusContext.CutsceneEditor_Properties, PropertiesView.Display);

        // The viewport window sets the focus itself
        ViewportWindow.Display(dsid);
    }

    /// <summary>cutscene/select/&lt;name&gt;[/&lt;cut&gt;]: shows a cutscene by name</summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is not { Length: >= 2 } || commands[0] != "select")
            return;

        var file = Data.Files.FirstOrDefault(f => f.Name.Equals(commands[1], StringComparison.OrdinalIgnoreCase));
        if (file == null)
            return;

        var cut = commands.Length >= 3 && int.TryParse(commands[2], out var c) ? c : 0;
        Select(file, cut);
        ListView.ScrollToSelection = true;
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##cutsceneViewMenu"))
            return;

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Load_Models"), "", CFG.Current.CutsceneEditor_Load_Models))
        {
            CFG.Current.CutsceneEditor_Load_Models = !CFG.Current.CutsceneEditor_Load_Models;
            Rebuild();
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Load_Models_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Display_Camera_Path"), "", CFG.Current.CutsceneEditor_Display_Camera_Path))
            CFG.Current.CutsceneEditor_Display_Camera_Path = !CFG.Current.CutsceneEditor_Display_Camera_Path;

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Display_Camera"), "", CFG.Current.CutsceneEditor_Display_Camera))
            CFG.Current.CutsceneEditor_Display_Camera = !CFG.Current.CutsceneEditor_Display_Camera;

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Display_Markers"), "", CFG.Current.CutsceneEditor_Display_Markers))
            CFG.Current.CutsceneEditor_Display_Markers = !CFG.Current.CutsceneEditor_Display_Markers;
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Display_Markers_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Diagnostics"), "", CFG.Current.CutsceneEditor_Diagnostics_Log))
            CFG.Current.CutsceneEditor_Diagnostics_Log = !CFG.Current.CutsceneEditor_Diagnostics_Log;
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Diagnostics_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Log_Cut"), "", false, Scene != null))
            Smithbox.Log(this, Scene!.Describe());
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Log_Cut_TT"));

        ImGui.EndMenu();
    }

    private static readonly string[] AxisNames = ["+X", "-X", "+Y", "-Y", "+Z", "-Z"];

    private void CameraMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("CUTSCENE_Menu_Camera")}##cutsceneCameraMenu"))
            return;

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Look_Through"), "", CFG.Current.CutsceneEditor_Look_Through_Camera))
            CFG.Current.CutsceneEditor_Look_Through_Camera = !CFG.Current.CutsceneEditor_Look_Through_Camera;
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Look_Through_TT"));

        ImGui.Separator();

        AxisCombo(LOC.Get("CUTSCENE_Menu_Forward_Axis"), "##cutsceneForwardAxis", ref CFG.Current.CutsceneEditor_Camera_Forward_Axis);
        AxisCombo(LOC.Get("CUTSCENE_Menu_Up_Axis"), "##cutsceneUpAxis", ref CFG.Current.CutsceneEditor_Camera_Up_Axis);
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Axes_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Fov_Horizontal"), "", CFG.Current.CutsceneEditor_Camera_Fov_Is_Horizontal))
            CFG.Current.CutsceneEditor_Camera_Fov_Is_Horizontal = !CFG.Current.CutsceneEditor_Camera_Fov_Is_Horizontal;
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Fov_Horizontal_TT"));

        ImGui.EndMenu();
    }

    private static void AxisCombo(string label, string id, ref int value)
    {
        ImGui.Text(label);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(80 * DPI.UIScale());
        if (ImGui.BeginCombo(id, AxisNames[Math.Clamp(value, 0, 5)]))
        {
            for (var i = 0; i < AxisNames.Length; i++)
            {
                if (ImGui.Selectable(AxisNames[i], i == value))
                    value = i;
            }

            ImGui.EndCombo();
        }
    }

    private static void Window(string title, uint dockspace, EditorFocusContext focus, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_CutsceneEditor);
        if (ImGui.Begin(title, GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);
            draw();
        }

        ImGui.End();
    }

    public void Update(float dt)
    {
        ApplyBuiltScene();

        if (Scene != null)
        {
            Playback.Update(dt, Scene.Framerate, Scene.FrameCount);

            if (Playback.ReachedEnd && SelectedFile?.Mqb is { } mqb)
            {
                var next = SelectedCut + 1 < mqb.Cuts.Count ? SelectedCut + 1 : CFG.Current.CutsceneEditor_Loop ? 0 : -1;
                if (next >= 0)
                    SelectCut(next);
                else
                    Playback.Playing = false;
            }

            Scene?.Update(Playback.Frame);

            // The lighting the viewport borrows, once there is something to light (as the FX Editor does)
            Project.Handler.ViewportEnvironment.Update();
        }

        ViewportWindow.Update(dt);
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

    // Nothing is edited yet
    public void Save()
    {
    }

    public void SaveAll()
    {
    }

    public List<string> GetUnsavedFiles() => new();

    public void SaveUnsaved()
    {
    }

    public void Dispose()
    {
        ClearScene();
        ViewportWindow.Dispose();
    }
}
