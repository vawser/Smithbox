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

    public ActionManager ActionManager = new();

    /// <summary>The map behind the cut (phase 3)</summary>
    public CutsceneMapBackdrop Backdrop;

    /// <summary>The viewport gizmo on <see cref="SelectedPlacement" /></summary>
    public CutscenePlacementGizmoTarget PlacementGizmo;

    /// <summary>The cut placement (offset.sib) being moved, picked with its event in the timeline or the properties</summary>
    public CutscenePlacement? SelectedPlacement;

    /// <summary>Bumped by every edit, undo and redo: the timeline builds its rows again when it changes</summary>
    public int StructureVersion { get; private set; }

    /// <summary>Events of every resource type, from the project's cutscenes, to add new ones from</summary>
    public CutsceneTemplateIndex Templates;

    // The map chosen for each cutscene this session, over the one its name implies
    private readonly Dictionary<string, string> MapForCutscene = new(StringComparer.OrdinalIgnoreCase);

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
        Backdrop = new CutsceneMapBackdrop(project, () => RenderScene);
        PlacementGizmo = new CutscenePlacementGizmoTarget(this);
        Templates = new CutsceneTemplateIndex(project, () => Data?.Files ?? new List<CutsceneFile>());

        // An edit, undo or redo may have moved, added or taken away events
        ActionManager.ActionPerformed += OnEdited;

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
        SelectedPlacement = null;
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

        if (CFG.Current.CutsceneEditor_Load_Map)
            LoadMapFor(Scene.File);

        if (CFG.Current.CutsceneEditor_Diagnostics_Log)
            Smithbox.Log(this, Scene.Describe() + $"  map: {CurrentMapFor(Scene.File) ?? "none"} (loaded {Backdrop.LoadedMapId ?? "none"})");
    }

    // ---- map

    /// <summary>The map shown behind a cutscene: the one chosen for it, else the one its name implies if the project has it</summary>
    public string? CurrentMapFor(CutsceneFile file)
    {
        if (MapForCutscene.TryGetValue(file.Name, out var chosen))
            return chosen;

        var implied = CutsceneMapBackdrop.ImpliedMap(file.Name);
        return implied != null && Backdrop.HasMap(implied) ? implied : null;
    }

    /// <summary>Shows another map behind a cutscene, for the rest of the session</summary>
    public void SetMapFor(CutsceneFile file, string? mapId)
    {
        if (mapId == null)
        {
            MapForCutscene.Remove(file.Name);
            Backdrop.UnloadMap();
            return;
        }

        MapForCutscene[file.Name] = mapId;
        if (CFG.Current.CutsceneEditor_Load_Map)
            Backdrop.LoadMap(mapId);
    }

    private void LoadMapFor(CutsceneFile file)
    {
        var map = CurrentMapFor(file);
        if (map == null)
            Backdrop.UnloadMap();
        else
            Backdrop.LoadMap(map);
    }

    private void OnEdited()
    {
        StructureVersion++;
        Scene?.RefreshEvents();

        // A selected event that an undo took away
        if (SelectedEvent != null && Scene != null && CutsceneMqbOps.TimelineOf(Scene.Cut, SelectedEvent) < 0)
        {
            SelectedEvent = null;
            SelectedTimeline = -1;
        }
    }

    /// <summary>Runs an edit of the selected cutscene, so it can be undone</summary>
    public void Execute(EditorAction? action)
    {
        if (action != null)
            ActionManager.ExecuteAction(action);
    }

    /// <summary>Selects an event, and its cut's placement if the event is of an offset.sib or an HKT</summary>
    public void SelectEvent(MQB.Event e, int timeline)
    {
        SelectedEvent = e;
        SelectedTimeline = timeline;

        var mqb = SelectedFile?.Mqb;
        var resource = mqb != null && e.ResourceIndex >= 0 && e.ResourceIndex < mqb.Resources.Count ? mqb.Resources[e.ResourceIndex] : null;

        SelectedPlacement = resource?.Type switch
        {
            CutsceneResourceTypes.Offset => Scene?.Placements.Values.FirstOrDefault(p => p.OffsetEvent == e),
            CutsceneResourceTypes.Animation => Scene?.Placements.GetValueOrDefault(e.ResourceIndex),
            _ => null
        };
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
        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
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

    private void Shortcuts()
    {
        if (!FocusManager.IsInCutsceneEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        if (ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            ActionManager.UndoAction();

        if (ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            ActionManager.RedoAction();

        // Not while a value is being typed
        if (SelectedEvent is { } e && !ImGui.GetIO().WantTextInput)
        {
            if (InputManager.IsPressed(KeybindID.Delete))
                TimelineView.Delete(e);
            else if (InputManager.IsPressed(KeybindID.Duplicate))
                TimelineView.Duplicate(e, false);
        }
    }

    private void FileMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##cutsceneFileMenu"))
            return;

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##cutsceneSave", InputManager.GetHint(KeybindID.Save), false, SelectedFile?.IsDirty == true))
            Save();
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Save_TT"));

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##cutsceneSaveAll", "", false, Data.Files.Any(f => f.IsDirty)))
            SaveAll();
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_SaveAll_TT"));

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##cutsceneEditMenu"))
            return;

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##cutsceneUndo", InputManager.GetHint(KeybindID.Undo), false, ActionManager.CanUndo()))
            ActionManager.UndoAction();

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##cutsceneRedo", InputManager.GetHint(KeybindID.Redo), false, ActionManager.CanRedo()))
            ActionManager.RedoAction();

        ImGui.EndMenu();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##cutsceneViewMenu"))
            return;

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Load_Map"), "", CFG.Current.CutsceneEditor_Load_Map))
        {
            CFG.Current.CutsceneEditor_Load_Map = !CFG.Current.CutsceneEditor_Load_Map;
            if (!CFG.Current.CutsceneEditor_Load_Map)
                Backdrop.UnloadMap();
            else if (SelectedFile != null)
                LoadMapFor(SelectedFile);
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Load_Map_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Map_Assets"), "", CFG.Current.CutsceneEditor_Map_Load_Assets))
        {
            CFG.Current.CutsceneEditor_Map_Load_Assets = !CFG.Current.CutsceneEditor_Map_Load_Assets;

            // Read the map again, with or without its assets
            var map = Backdrop.LoadedMapId;
            Backdrop.UnloadMap();
            if (map != null)
                Backdrop.LoadMap(map);
        }

        ImGui.Separator();

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
        Backdrop.Update();

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

    /// <summary>Writes the selected cutscene's MQB into the project, if it has changes</summary>
    public void Save()
    {
        if (SelectedFile is { IsDirty: true } file)
            file.Save(Project);
    }

    public void SaveAll()
    {
        foreach (var file in Data.Files.Where(f => f.IsDirty))
            file.Save(Project);
    }

    public List<string> GetUnsavedFiles() => Data?.Files.Where(f => f.IsDirty).Select(f => f.Entry.Path).OrderBy(p => p).ToList() ?? new();

    public void SaveUnsaved() => SaveAll();

    public void Dispose()
    {
        ClearScene();
        Backdrop.UnloadMap();
        ViewportWindow.Dispose();
    }
}
