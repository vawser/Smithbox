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

    /// <summary>The cut's subtitles in the viewport, and their voices while it plays</summary>
    public CutsceneSubtitles Subtitles;

    /// <summary>The cut's fades, laid over the viewport</summary>
    public CutsceneFades Fades;

    /// <summary>The actors' models, kept across the cuts of a cutscene so a cut only loads what earlier ones didn't</summary>
    public CutsceneModelPool Models = new();

    /// <summary>The Character Preview tool (shared with the Model and Time Act Editors): what the player-like actors wear</summary>
    public StudioCore.Editors.ModelEditor.ModelCharacterPreview CharacterPreview;

    /// <summary>What c0000 actors wear: the tool's choice, a bare body until one is made. Kept for the session.</summary>
    public CharacterAppearance? PlayerAppearance { get; private set; }

    /// <summary>The cut's sound events, played as it plays</summary>
    public CutsceneSoundPlayback Sounds;

    /// <summary>The cut's effect events (sfxId), drawn with the shared FX display as it plays</summary>
    public CutsceneEffects Effects;

    /// <summary>The viewport gizmo on <see cref="SelectedCameraKey" /></summary>
    public CutsceneCameraKeyGizmoTarget CameraKeyGizmo;

    /// <summary>The key picked in the properties' key table, of <see cref="SelectedCamera" />; -1 for none</summary>
    public int SelectedCameraKey = -1;

    /// <summary>The camera of the selected event, if it is a camera event</summary>
    public CutsceneCamera? SelectedCamera =>
        SelectedEvent is { } e ? Scene?.Cameras.FirstOrDefault(c => c.ResourceIndex == e.ResourceIndex) : null;

    /// <summary>Bumped by every edit, undo and redo: the timeline builds its rows again when it changes</summary>
    public int StructureVersion { get; private set; }

    /// <summary>Events of every resource type, from the project's cutscenes, to add new ones from</summary>
    public CutsceneTemplateIndex Templates;

    // The map chosen for each cutscene this session, over the one its name implies
    private readonly Dictionary<string, string> MapForCutscene = new(StringComparer.OrdinalIgnoreCase);

    public CutsceneListView ListView;
    public CutsceneTimelineView TimelineView;
    public CutscenePropertiesView PropertiesView;
    public CutsceneToolWindow ToolWindow;

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
        CameraKeyGizmo = new CutsceneCameraKeyGizmoTarget(this);
        Subtitles = new CutsceneSubtitles(this);
        Fades = new CutsceneFades(this);
        Sounds = new CutsceneSoundPlayback(this);
        Effects = new CutsceneEffects(this);
        CharacterPreview = new StudioCore.Editors.ModelEditor.ModelCharacterPreview(project, GetCharacterPreviewTarget);
        Templates = new CutsceneTemplateIndex(project, () => Data?.Files ?? new List<CutsceneFile>());

        // An edit, undo or redo may have moved, added or taken away events
        ActionManager.ActionPerformed += OnEdited;

        ListView = new CutsceneListView(this);
        TimelineView = new CutsceneTimelineView(this);
        PropertiesView = new CutscenePropertiesView(this);
        ToolWindow = new CutsceneToolWindow(this);
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
        SelectedCameraKey = -1;
        BuildError = null;

        SceneTaskFor = (file, cut);

        // The next cut of a play-through may already be read
        if (Prefetch is { } prefetch && prefetch.File == file && prefetch.Cut == cut)
        {
            SceneTask = prefetch.Task;
            Prefetch = null;
            return;
        }

        DropPrefetch();
        SceneTask = BuildTask(file, cut);
    }

    private Task<CutsceneScene?> BuildTask(CutsceneFile file, int cut)
    {
        var project = Project;

        // Read here, on the main thread: the map the actors' map parts are looked for in first
        var shownMap = MapForCutscene.TryGetValue(file.Name, out var chosen) ? chosen : CutsceneMapBackdrop.ImpliedMap(file.Name);

        return Task.Run<CutsceneScene?>(() =>
        {
            if (!file.Load(project))
                return null;

            var mqb = file.Mqb!;
            if (mqb.Cuts.Count == 0)
                return null;

            return CutsceneScene.Prepare(project, file, Math.Clamp(cut, 0, mqb.Cuts.Count - 1), shownMap);
        });
    }

    // The cut a play-through goes to next, read in the background while this one plays
    private (CutsceneFile File, int Cut, Task<CutsceneScene?> Task)? Prefetch;

    /// <summary>With Play Through on, starts reading the cut after the one shown, so moving on to it doesn't wait</summary>
    private void PrefetchNextCut()
    {
        if (!CFG.Current.CutsceneEditor_Continue_To_Next_Cut || SelectedFile?.Mqb is not { } mqb)
            return;

        var next = SelectedCut + 1 < mqb.Cuts.Count ? SelectedCut + 1 : CFG.Current.CutsceneEditor_Loop ? 0 : -1;
        if (next < 0 || next == SelectedCut || Prefetch is { } p && p.File == SelectedFile && p.Cut == next)
            return;

        DropPrefetch();
        Prefetch = (SelectedFile, next, BuildTask(SelectedFile, next));
    }

    /// <summary>Lets a background read go: its scene holds nothing loaded yet, so it is disposed once it ends</summary>
    private void DropPrefetch()
    {
        if (Prefetch is not { } prefetch)
            return;

        Prefetch = null;
        prefetch.Task.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
                t.Result?.Dispose();
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

        // The last cut's actors hand their models back, and this cut takes the ones it shares with them
        ClearScene();
        Models.BeginScene(built.File);
        Scene = built;
        Scene.StartLoads(Project, RenderScene, Models, EnsurePlayerAppearance());

        var keepPlaying = Playback.Playing;
        Playback.Restart();
        Playback.Playing = keepPlaying;

        ViewportWindow.OnSceneChanged();

        // The same map for every cut of a cutscene: LoadMap keeps the one drawn
        if (CFG.Current.CutsceneEditor_Load_Map)
            LoadMapFor(Scene.File);

        PrefetchNextCut();

        if (CFG.Current.CutsceneEditor_Diagnostics_Log)
            Smithbox.Log(this, Scene.Describe() + $"  map: {CurrentMapFor(Scene.File) ?? "none"} (loaded {Backdrop.LoadedMapId ?? "none"})" +
                               $"  models: {Models.Reused} kept from earlier cuts, {Models.Created} loaded, {Models.MeshCount} held");
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
        // A cut read before the edit would show the data as it was
        DropPrefetch();
        StructureVersion++;
        Scene?.RefreshEvents();

        RefreshCameras();

        // A selected event that an undo took away
        if (SelectedEvent != null && Scene != null && CutsceneMqbOps.TimelineOf(Scene.Cut, SelectedEvent) < 0)
        {
            SelectedEvent = null;
            SelectedTimeline = -1;
        }
    }

    /// <summary>
    /// Plays the cut's cameras from what changed: their keys after a key edit (or its undo), their clips after a save
    /// baked them into the cut's Havok file.
    /// </summary>
    public void RefreshCameras()
    {
        if (Scene == null)
            return;

        if (Scene.HavokVersionRead != Scene.File.HavokVersion)
        {
            DropPrefetch();
            Scene.ReloadCameras(Project);
            ViewportWindow.InvalidateLines();
        }

        if (Scene.KeysVersionRead != Scene.File.KeysVersion)
        {
            Scene.RefreshKeys();
            ViewportWindow.InvalidateLines();
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
        // Another resource's event: its camera's keys aren't the ones picked
        if (SelectedEvent?.ResourceIndex != e.ResourceIndex)
            SelectedCameraKey = -1;

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
        DropPrefetch();
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

        // Its own menu bar (View, Options), as the other editors' Tools windows have
        if (CFG.Current.Interface_CutsceneEditor_ToolWindow)
            Window($"{LOC.Get("CUTSCENE_Window_Tools")}###cutsceneEditor_Tools", dsid, EditorFocusContext.CutsceneEditor_Tools, ToolWindow.Display, GUI.GetMainWindowFlags());

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

        // Tools window (what is shown and played back is its Cutscene Filters tool)
        if (ImGui.MenuItem($"{LOC.Get("CUTSCENE_View_Toggle_Tools")}##cutsceneToggleTools"))
            CFG.Current.Interface_CutsceneEditor_ToolWindow = !CFG.Current.Interface_CutsceneEditor_ToolWindow;
        GUI.ShowActiveStatus(CFG.Current.Interface_CutsceneEditor_ToolWindow);

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Diagnostics"), "", CFG.Current.CutsceneEditor_Diagnostics_Log))
            CFG.Current.CutsceneEditor_Diagnostics_Log = !CFG.Current.CutsceneEditor_Diagnostics_Log;
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Diagnostics_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Log_Cut"), "", false, Scene != null))
            Smithbox.Log(this, Scene!.Describe());
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Log_Cut_TT"));

        ImGui.EndMenu();
    }

    // ---- the map's draw groups: what the player sees standing where the cut's camera starts

    private (CutsceneScene? Scene, string? Map, int Collision, bool Use, int Keys, string? Chosen) MapGroupsFor;

    // The collision chosen to stand on (where the camera's start is a poor guess), by cutscene and map, for the session.
    // Not by cut: the choice holds as the user moves between the cutscene's cuts, rather than going back to auto.
    private readonly Dictionary<(string Cutscene, string Map), string> ChosenCollisions = new();

    /// <summary>The collision chosen for the cutscene shown in the map shown, null to take the one under the camera</summary>
    public string? ChosenCollision =>
        SelectedFile != null && Backdrop.LoadedMapId != null &&
        ChosenCollisions.TryGetValue((SelectedFile.Name, Backdrop.LoadedMapId), out var name) ? name : null;

    /// <summary>Stands the cutscene shown on a collision of the map (null: the one under each cut camera's start again)</summary>
    public void SetChosenCollision(string? collision)
    {
        if (SelectedFile == null || Backdrop.LoadedMapId == null)
            return;

        var key = (SelectedFile.Name, Backdrop.LoadedMapId);
        if (collision == null)
            ChosenCollisions.Remove(key);
        else
            ChosenCollisions[key] = collision;
    }

    /// <summary>Where the cut's camera is at its first frame, in the map's space (null without a camera)</summary>
    public Vector3? CameraStart()
    {
        if (Scene?.ActiveCamera(0) is not { Skeleton: not null } camera)
            return null;

        return camera.World(0, Scene.RootOf(camera.Resource, 0)).Translation;
    }

    /// <summary>
    /// Draws the map's parts in the draw groups of the collision under the cut camera's start (cast straight down),
    /// and hides the map characters that cutscene actors stand for, whenever the cut, the map, its collision or the
    /// option change.
    /// </summary>
    private void UpdateMapGroups()
    {
        var chosen = ChosenCollision;
        var key = (Scene, Backdrop.LoadedMapId, Backdrop.CollisionVersion, CFG.Current.CutsceneEditor_Map_Use_Draw_Groups,
            Scene?.File.KeysVersion ?? 0, chosen);
        if (key == MapGroupsFor || Backdrop.LoadedMapId == null)
            return;

        MapGroupsFor = key;

        // The characters and assets the cut plays itself. An asset actor stands for the map part of its own name, as
        // 1285 of ER's do (AEG099_330_9000, the banner Godrick carries; Claude/Tools/CutsceneAssetCastProbe)
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var actor in Scene?.Actors ?? new List<CutsceneActor>())
        {
            if (actor.Cast is { } cast && cast.Map.Equals(Backdrop.LoadedMapId, StringComparison.OrdinalIgnoreCase))
                excluded.Add(cast.PartName);
            else if (actor.Model is { Kind: CutsceneModelKind.Asset or CutsceneModelKind.Object })
                excluded.Add(actor.Name);
        }

        Backdrop.ApplyGroups(CameraStart(), CFG.Current.CutsceneEditor_Map_Use_Draw_Groups, excluded, chosen);

        if (CFG.Current.CutsceneEditor_Diagnostics_Log)
            Smithbox.Log(this, LOC.Get("CUTSCENE_Map_Groups_Log", Backdrop.StandingOn ?? "-",
                Backdrop.ActiveDisplay?.Sum(w => System.Numerics.BitOperations.PopCount(w)) ?? 0,
                Backdrop.ShownCount, Backdrop.PartCount, excluded.Count));
    }

    /// <summary>The map's own characters: the map is read again, with or without them</summary>
    public void SetMapCharacters(bool on)
    {
        CFG.Current.CutsceneEditor_Map_Load_Characters = on;

        var map = Backdrop.LoadedMapId;
        Backdrop.UnloadMap();
        if (map != null)
            Backdrop.LoadMap(map);
    }

    // ---- the player: dressed by the Character Preview tool

    /// <summary>The player's appearance, the tool's current choice (a bare body) the first time it is asked for</summary>
    private CharacterAppearance? EnsurePlayerAppearance()
    {
        if (PlayerAppearance != null)
            return PlayerAppearance;

        try
        {
            PlayerAppearance = CharacterPreview.GetAppearance();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Player_Appearance_Failed"), e);
        }

        return PlayerAppearance;
    }

    // What the tool dresses: always c0000, so a look can be chosen before a cut with the player is shown
    private StudioCore.Editors.ModelEditor.CharacterPreviewTarget GetCharacterPreviewTarget() =>
        new(CharacterComposition.CompositeModel, EnsurePlayerAppearance()!, SetPlayerAppearance);

    /// <summary>Dresses the player-like actors of the cut shown, and of every cut after, in an appearance</summary>
    public void SetPlayerAppearance(CharacterAppearance appearance)
    {
        PlayerAppearance = appearance;
        Scene?.DressPlayers(appearance);

        // The parts of the look before are kept by no cut any more
        Models.DiscardUnused(CutscenePlayerParts.PoolPrefix);

        if (CFG.Current.CutsceneEditor_Diagnostics_Log)
            Smithbox.Log(this, LOC.Get("CUTSCENE_Player_Dressed", string.Join(", ", appearance.Parts.Select(p => p.Name))));
    }

    // ---- filters: what is shown and played back (the Cutscene Filters tool). The ones that load or play something
    // do what the change needs at once.

    /// <summary>The map behind the cut: loaded or let go</summary>
    public void SetLoadMap(bool on)
    {
        CFG.Current.CutsceneEditor_Load_Map = on;
        if (!on)
            Backdrop.UnloadMap();
        else if (SelectedFile != null)
            LoadMapFor(SelectedFile);
    }

    /// <summary>The map's assets: the map is read again, with or without them</summary>
    public void SetMapAssets(bool on)
    {
        CFG.Current.CutsceneEditor_Map_Load_Assets = on;

        var map = Backdrop.LoadedMapId;
        Backdrop.UnloadMap();
        if (map != null)
            Backdrop.LoadMap(map);
    }

    /// <summary>The actors' models: the cut is built again; turned off, the models kept for later cuts go too</summary>
    public void SetLoadModels(bool on)
    {
        CFG.Current.CutsceneEditor_Load_Models = on;
        Rebuild();

        if (!on)
            Models.Clear();
    }

    public void SetPlayVoices(bool on)
    {
        CFG.Current.CutsceneEditor_Play_Voices = on;
        if (!on)
            Subtitles.StopVoice();
    }

    public void SetDisplayEffects(bool on)
    {
        CFG.Current.CutsceneEditor_Display_Effects = on;
        if (!on)
            Effects.Clear();
    }

    public void SetPlaySounds(bool on)
    {
        CFG.Current.CutsceneEditor_Play_Sounds = on;
        if (!on)
            Sounds.StopAll();
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

    private static void Window(string title, uint dockspace, EditorFocusContext focus, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_CutsceneEditor);
        if (ImGui.Begin(title, flags ?? GUI.GetInnerWindowFlags()))
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
        UpdateMapGroups();

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
            Subtitles.Update(Scene, Playback.Frame, Playback.Playing);
            Sounds.Update(Scene, Playback.Frame, Playback.Playing);

            // The lighting the viewport borrows, once there is something to light (as the FX Editor does)
            Project.Handler.ViewportEnvironment.Update();
        }
        else
        {
            // No cut shown (another being built): its voice stops
            Subtitles.Update(null, 0, false);
            Sounds.Update(null, 0, false);
        }

        ViewportWindow.Update(dt);

        // After the viewport, so the effects face and sort from the camera it shows (the cut's, looking through it)
        var (eye, forward) = ViewportWindow.CameraEyeAndForward();
        Effects.Update(Scene, Playback.Frame, eye, forward);
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
        RefreshCameras();
    }

    public void SaveAll()
    {
        foreach (var file in Data.Files.Where(f => f.IsDirty))
            file.Save(Project);
        RefreshCameras();
    }

    public List<string> GetUnsavedFiles() => Data?.Files.Where(f => f.IsDirty).Select(f => f.Entry.Path).OrderBy(p => p).ToList() ?? new();

    public void SaveUnsaved() => SaveAll();

    public void Dispose()
    {
        DropPrefetch();
        ClearScene();
        Models.Dispose();
        Backdrop.UnloadMap();
        Subtitles.Dispose();
        Sounds.Dispose();
        Effects.Dispose();
        ViewportWindow.Dispose();
    }
}
