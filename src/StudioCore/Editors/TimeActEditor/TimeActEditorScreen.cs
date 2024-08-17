using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using static Silk.NET.Core.Native.WinString;
using static SoulsFormats.TAE.Animation;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Tools;
using static StudioCore.Editors.TimeActEditor.TimeActUtils;
using HKLib.hk2018.hkAsyncThreadPool;
using static SoulsFormats.TAE.Animation.AnimMiniHeader;
using static StudioCore.Editors.TimeActEditor.TimeActSelectionHandler;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ViewportActionManager ViewportEditorActionManager = new();
    public ActionManager EditorActionManager = new();

    public TimeActSelectionHandler SelectionHandler;

    public ViewportSelection _selection = new();
    public Universe _universe;
    public Rectangle Rect;
    public RenderScene RenderScene;
    public IViewport Viewport;
    public bool ViewportUsingKeyboard;
    public Sdl2Window Window;

    public TimeActCollectionPropertyHandler CollectionPropertyHandler;
    public TimeActFieldPropertyHandler FieldPropertyHandler;
    public TimeActDecorator Decorator;
    public TimeActEventGraph EventGraph;

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;

    public ActionSubMenu ActionSubMenu;
    public ActionHandler ActionHandler;

    public TimeActEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        // TODO: re-enable once animation stuff is worked on
        /*
        Rect = window.Bounds;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport(ViewportType.TimeActEditor, "TimeActEditor_ViewPort", device, RenderScene, ViewportEditorActionManager, _selection, Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(ViewportType.TimeActEditor, "TimeActEditor_ViewPort", ViewportEditorActionManager, _selection, Rect.Width, Rect.Height);
        }
        */

        //_universe = new Universe(RenderScene, _selection);

        SelectionHandler = new TimeActSelectionHandler(EditorActionManager, this);
        Decorator = new TimeActDecorator(EditorActionManager, this);
        FieldPropertyHandler = new TimeActFieldPropertyHandler(EditorActionManager, this, Decorator);
        CollectionPropertyHandler = new TimeActCollectionPropertyHandler(EditorActionManager, this, Decorator);
        EventGraph = new TimeActEventGraph(EditorActionManager, this, SelectionHandler);

        ActionHandler = new(this, EditorActionManager);
        ActionSubMenu = new(this, ActionHandler);
        ToolWindow = new(this, ActionHandler);
        ToolSubMenu = new(this, ActionHandler);
    }

    public string EditorName => "Time Act Editor##TimeActEditor";
    public string CommandEndpoint => "timeact";
    public string SaveType => "TAE";

    public void Init()
    {
        ShowSaveOption = true;

        // TAE implementation needs to be updated for AC6 (need to decompile AC6 DSAS to work out differences)
        if(Smithbox.ProjectType is ProjectType.AC6)
        {
            ShowSaveOption = false;
        }
    }

    public void Update(float dt)
    {
        //ViewportUsingKeyboard = Viewport.Update(Window, dt);
    }

    /*
    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
        Rect = window.Bounds;
    }
    */

    /*
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }
    */

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_TimeActEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType is ProjectType.Undefined)
        {
            ImGui.Begin("Editor##InvalidTaeEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            AnimationBank.Load();

            if (!TaskManager.AnyActiveTasks() && AnimationBank.IsLoaded)
            {
                //Viewport.OnGui();
                TimeActCommandLine(initcmd);
                TimeActEditorShortcuts();

                if (CFG.Current.Interface_TimeActEditor_ContainerFileList)
                {
                    TimeActContainerFileView();
                }

                if (CFG.Current.Interface_TimeActEditor_TimeActList)
                {
                    TimeActInternalFileView();
                }

                if (CFG.Current.Interface_TimeActEditor_AnimationList)
                {
                    TimeActAnimationView();
                }

                if (CFG.Current.Interface_TimeActEditor_AnimationProperties)
                {
                    TimeActAnimationPropertyView();
                }

                // TODO: find method to create event graph
                //TimeActAnimEventGraphView();

                if (CFG.Current.Interface_TimeActEditor_EventList)
                {
                    TimeActAnimEventView();
                }

                if (CFG.Current.Interface_TimeActEditor_EventProperties)
                {
                    TimeActEventPropertiesView();
                }
            }
            else
            {
                ImGui.Begin("Editor##LoadingTaeEditor");

                AnimationBank.DisplayLoadState();

                ImGui.End();
            }
        }

        ActionSubMenu.Shortcuts();
        ToolSubMenu.Shortcuts();

        if (CFG.Current.Interface_TimeActEditor_ToolConfiguration)
        {
            ToolWindow.OnGui();
        }

        CollectionPropertyHandler.OnGui();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    private bool FocusContainer = false;
    private bool FocusTimeAct = false;
    private bool FocusAnimation = false;
    private bool FocusEvent = false;

    // Respond to EditorQueue commands
    public void TimeActCommandLine(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 2)
        {
            if (initcmd[0] == "select")
            {
                SelectionHandler.ResetSelection();

                var containerType = initcmd[1];
                var containerIndex = initcmd[2];

                if (containerType == "chr")
                {
                    for (int i = 0; i < AnimationBank.FileChrBank.Count; i++)
                    {
                        var container = AnimationBank.FileChrBank.ElementAt(i);
                        var index = int.Parse(containerIndex);

                        if (i == index)
                        {
                            SelectionHandler.ContainerIndex = index;
                            SelectionHandler.ContainerKey = container.Key.Name;
                            SelectionHandler.ContainerInfo = container.Key;
                            SelectionHandler.ContainerBinder = container.Value;
                            FocusContainer = true;
                        }
                    }
                }

                if (containerType == "obj")
                {
                    for (int i = 0; i < AnimationBank.FileObjBank.Count; i++)
                    {
                        var container = AnimationBank.FileObjBank.ElementAt(i);
                        var index = int.Parse(containerIndex);

                        if (i == index)
                        {
                            SelectionHandler.ContainerIndex = index;
                            SelectionHandler.ContainerKey = container.Key.Name;
                            SelectionHandler.ContainerInfo = container.Key;
                            SelectionHandler.ContainerBinder = container.Value;
                            FocusContainer = true;
                        }
                    }
                }

                if (initcmd.Length > 3)
                {
                    var timeActIndex = initcmd[3];
                    var index = int.Parse(timeActIndex);

                    for (int i = 0; i < SelectionHandler.ContainerInfo.InternalFiles.Count; i++)
                    {
                        if(i == index)
                        {
                            SelectionHandler.CurrentTimeActKey = i;
                            SelectionHandler.CurrentTimeAct = SelectionHandler.ContainerInfo.InternalFiles[i].TAE;

                            SelectionHandler.TimeActMultiselect.StoredTimeActs.Add(i, SelectionHandler.ContainerInfo.InternalFiles[i].TAE);

                            FocusTimeAct = true;
                        }
                    }
                }

                if (initcmd.Length > 4)
                {
                    var animationIndex = initcmd[4];
                    var index = int.Parse(animationIndex);

                    for (int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
                    {
                        if (i == index)
                        {
                            SelectionHandler.CurrentTimeActAnimationIndex = i;
                            SelectionHandler.CurrentTimeActAnimation = SelectionHandler.CurrentTimeAct.Animations[i];

                            SelectionHandler.TimeActMultiselect.StoredAnimations.Add(i, SelectionHandler.CurrentTimeAct.Animations[i]);

                            FocusAnimation = true;
                        }
                    }
                }

                if (initcmd.Length > 5)
                {
                    var eventIndex = initcmd[5];
                    var index = int.Parse(eventIndex);

                    for (int i = 0; i < SelectionHandler.CurrentTimeActAnimation.Events.Count; i++)
                    {
                        if (i == index)
                        {
                            SelectionHandler.CurrentTimeActEventIndex = i;
                            SelectionHandler.CurrentTimeActEvent = SelectionHandler.CurrentTimeActAnimation.Events[i];

                            SelectionHandler.TimeActMultiselect.StoredEvents.Add(i, SelectionHandler.CurrentTimeActAnimation.Events[i]);

                            FocusEvent = true;
                        }
                    }
                }
            }
        }
    }

    public void TimeActEditorShortcuts()
    {
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        /*
        if (!ViewportUsingKeyboard && !ImGui.GetIO().WantCaptureKeyboard)
        {
            // F key frames the selection
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport))
            {
                HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
                var first = false;
                BoundingBox box = new();
                foreach (Entity s in selected)
                {
                    if (s.RenderSceneMesh != null)
                    {
                        if (!first)
                        {
                            box = s.RenderSceneMesh.GetBounds();
                            first = true;
                        }
                        else
                        {
                            box = BoundingBox.Combine(box, s.RenderSceneMesh.GetBounds());
                        }
                    }
                }

                if (first)
                {
                    Viewport.FrameBox(box);
                }
            }
        }
        */
    }

    public void TimeActContainerFileView()
    {
        // File List
        ImGui.Begin("Files##TimeActFileList");

        ImGui.InputText($"Search##fileContainerFilter", ref TimeActFilters._fileContainerFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("ContainerList");

        if (ImGui.CollapsingHeader("Characters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < AnimationBank.FileChrBank.Count; i++)
            {
                var info = AnimationBank.FileChrBank.ElementAt(i).Key;
                var binder = AnimationBank.FileChrBank.ElementAt(i).Value;

                if (TimeActFilters.FileContainerFilter(info))
                {
                    var isSelected = false;
                    if (i == SelectionHandler.ContainerIndex)
                    {
                        isSelected = true;
                    }

                    if (ImGui.Selectable($@" {info.Name}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        SelectionHandler.FileContainerChange(info, binder, i, FileContainerType.Character);
                    }
                    if (ImGui.IsItemVisible())
                    {
                        TimeActUtils.DisplayTimeActFileAlias(info.Name, AliasType.Character);
                    }

                    SelectionHandler.ContextMenu.ContainerMenu(isSelected, info.Name);

                    if(FocusContainer)
                    {
                        FocusContainer = false;

                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        var title = $"{AnimationBank.GetObjectTitle()}s";

        if (ImGui.CollapsingHeader(title))
        {
            for (int i = 0; i < AnimationBank.FileObjBank.Count; i++)
            {
                var info = AnimationBank.FileObjBank.ElementAt(i).Key;
                var binder = AnimationBank.FileObjBank.ElementAt(i).Value;

                if (TimeActFilters.FileContainerFilter(info))
                {
                    var isSelected = false;
                    if (i == SelectionHandler.ContainerIndex)
                    {
                        isSelected = true;
                    }

                    if (ImGui.Selectable($@" {info.Name}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        SelectionHandler.FileContainerChange(info, binder, i, FileContainerType.Object);
                    }
                    if (ImGui.IsItemVisible())
                    {
                        TimeActUtils.DisplayTimeActFileAlias(info.Name, AliasType.Asset);
                    }

                    SelectionHandler.ContextMenu.ContainerMenu(isSelected, info.Name);

                    if (FocusContainer)
                    {
                        FocusContainer = false;

                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }

    public void TimeActInternalFileView()
    {
        ImGui.Begin("Time Acts##TimeActList");

        if(!SelectionHandler.HasSelectedFileContainer())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActFilter", ref TimeActFilters._timeActFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("TimeActList");

        for (int i = 0; i < SelectionHandler.ContainerInfo.InternalFiles.Count; i++)
        {
            AnimationBank.InternalFileInfo info = SelectionHandler.ContainerInfo.InternalFiles[i];
            TAE entry = SelectionHandler.ContainerInfo.InternalFiles[i].TAE;

            if (TimeActFilters.TimeActFilter(SelectionHandler.ContainerInfo, entry))
            {
                // Ignore entries marked for removal
                if(info.MarkForRemoval)
                {
                    continue;
                }

                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActKey || 
                    SelectionHandler.TimeActMultiselect.IsTimeActSelected(i))
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@"{info.Name}##TimeAct{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActChange(entry, i);
                }

                if (ImGui.IsItemVisible())
                {
                    if (CFG.Current.TimeActEditor_DisplayTimeActRow_AliasInfo)
                        TimeActUtils.DisplayTimeActAlias(SelectionHandler.ContainerInfo, entry.ID);
                }

                SelectionHandler.ContextMenu.TimeActMenu(isSelected, entry.ID.ToString());

                if (FocusTimeAct)
                {
                    FocusTimeAct = false;

                    ImGui.SetScrollHereY();
                }
            }

        }
        ImGui.EndChild();

        ImGui.End();
    }

    
    public void TimeActAnimationView()
    {
        ImGui.Begin("Animations##TimeActAnimationList");

        if (!SelectionHandler.HasSelectedTimeAct())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActAnimationFilter", ref TimeActFilters._timeActAnimationFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("AnimationList");

        for (int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
        {
            TAE.Animation entry = SelectionHandler.CurrentTimeAct.Animations[i];

            if (TimeActFilters.TimeActAnimationFilter(SelectionHandler.ContainerInfo, entry))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActAnimationIndex ||
                    SelectionHandler.TimeActMultiselect.IsAnimationSelected(i))
                {
                    isSelected = true;
                }

                var displayName = $"{entry.ID}";
                if(CFG.Current.TimeActEditor_DisplayAnimFileName)
                {
                    displayName = $"{entry.ID} {entry.AnimFileName}";
                }

                if (ImGui.Selectable($@" {displayName}##taeAnim{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    EventGraph.ResetGraph();
                    SelectionHandler.TimeActAnimationChange(entry, i);
                }

                if (ImGui.IsItemVisible())
                {
                    if (CFG.Current.TimeActEditor_DisplayAnimRow_GeneratorInfo)
                        TimeActUtils.DisplayAnimationAlias(SelectionHandler, entry.ID);
                }

                SelectionHandler.ContextMenu.TimeActAnimationMenu(isSelected, entry.ID.ToString());

                if (FocusAnimation)
                {
                    FocusAnimation = false;

                    ImGui.SetScrollHereY();
                }
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }


    public void TimeActAnimationPropertyView()
    {
        ImGui.Begin("Animation Properties##TimeActAnimationProperties");

        if (!SelectionHandler.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        var anim = SelectionHandler.CurrentTimeActAnimation;

        if (SelectionHandler.CurrentTemporaryAnimHeader == null)
        {
            SelectionHandler.CurrentTemporaryAnimHeader = new TemporaryAnimHeader();
            SelectionHandler.CurrentTemporaryAnimHeader.CurrentType = anim.MiniHeader.Type;

            if(anim.MiniHeader.Type is MiniHeaderType.Standard)
            {
                SelectionHandler.CurrentTemporaryAnimHeader.IsLoopByDefault = anim.MiniHeader.IsLoopByDefault;
                SelectionHandler.CurrentTemporaryAnimHeader.ImportsHKX = anim.MiniHeader.ImportsHKX;
                SelectionHandler.CurrentTemporaryAnimHeader.AllowDelayLoad = anim.MiniHeader.AllowDelayLoad;
                SelectionHandler.CurrentTemporaryAnimHeader.ImportHKXSourceAnimID = anim.MiniHeader.ImportHKXSourceAnimID;
                SelectionHandler.CurrentTemporaryAnimHeader.ImportFromAnimID = 0;
                SelectionHandler.CurrentTemporaryAnimHeader.Unknown = -1;
            }
            if (anim.MiniHeader.Type is MiniHeaderType.ImportOtherAnim)
            {
                SelectionHandler.CurrentTemporaryAnimHeader.IsLoopByDefault = false;
                SelectionHandler.CurrentTemporaryAnimHeader.ImportsHKX = false;
                SelectionHandler.CurrentTemporaryAnimHeader.AllowDelayLoad = false;
                SelectionHandler.CurrentTemporaryAnimHeader.ImportHKXSourceAnimID = 0;
                SelectionHandler.CurrentTemporaryAnimHeader.ImportFromAnimID = anim.MiniHeader.ImportFromAnimID;
                SelectionHandler.CurrentTemporaryAnimHeader.Unknown = anim.MiniHeader.Unknown;
            }
        }

        FieldPropertyHandler.AnimationHeaderSection(SelectionHandler);

        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.Columns(3);
        }
        else
        {
            ImGui.Columns(2);
        }

        // Property Column
        ImGui.AlignTextToFramePadding();
        ImGui.Text("ID");
        ImguiUtils.ShowHoverTooltip("ID number of this animation.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        ImguiUtils.ShowHoverTooltip("The name of this animation entry.");

        if (SelectionHandler.CurrentTemporaryAnimHeader != null)
        {
            if (anim.MiniHeader.Type == MiniHeaderType.Standard)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Loop by Default");
                ImguiUtils.ShowHoverTooltip("Makes the animation loop by default. Only relevant for animations not controlled byESD or HKS such as ObjAct animations.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Allow Delay Load");
                ImguiUtils.ShowHoverTooltip("Whether to allow this animation to be loaded from delayload anibnds such as the c0000_cXXXX.anibnd player throw anibnds.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Imports HKX");
                ImguiUtils.ShowHoverTooltip("Whether to import the HKX (actual motion data) of the animation with the ID of referenced in Import HKX Source Anim ID.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Import HKX Source Anim ID");
                ImguiUtils.ShowHoverTooltip("Anim ID to import HKX from.");
            }

            if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Import Anim ID");
                ImguiUtils.ShowHoverTooltip("ID of animation from which to import motion dat and all events.");
            }
        }

        ImGui.NextColumn();

        // Value Column
        FieldPropertyHandler.AnimationValueSection(SelectionHandler);

        // Type Column

        // Type Column
        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("int");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("string");

            if (SelectionHandler.CurrentTemporaryAnimHeader != null)
            {
                if (anim.MiniHeader.Type == MiniHeaderType.Standard)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("b");

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("b");

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("b");

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("long");
                }

                if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("int");
                }
            }
        }

        ImGui.Columns(1);

        ImGui.End();
    }


    public bool SelectFirstEvent = false;

    public void TimeActAnimEventGraphView()
    {
        ImGui.Begin("Event Graph##TimeActAnimEventGraph");

        if (!SelectionHandler.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        EventGraph.Display();

        ImGui.End();
    }

    public void TimeActAnimEventView()
    {
        ImGui.Begin("Events##TimeActAnimEventList");

        if (!SelectionHandler.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventFilter", ref TimeActFilters._timeActEventFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventList");

        for (int i = 0; i < SelectionHandler.CurrentTimeActAnimation.Events.Count; i++)
        {
            TAE.Event evt = SelectionHandler.CurrentTimeActAnimation.Events[i];

            if (TimeActFilters.TimeActEventFilter(SelectionHandler.ContainerInfo, evt))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActEventIndex ||
                    SelectionHandler.TimeActMultiselect.IsEventSelected(i))
                {
                    isSelected = true;
                }

                if(SelectFirstEvent)
                {
                    SelectFirstEvent = false;
                    SelectionHandler.TimeActEventChange(evt, i);
                }

                var displayName = $"{evt.TypeName}";
                if(CFG.Current.TimeActEditor_DisplayEventID)
                {
                    displayName = $"[{evt.Type}] {displayName}";
                }
                if(CFG.Current.TimeActEditor_DisplayEventBank)
                {
                    displayName = $"<{SelectionHandler.CurrentTimeAct.EventBank}> {displayName}";
                }
                displayName = $" {displayName}";

                if (ImGui.Selectable($@"{displayName}##taeEvent{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActEventChange(evt, i);
                }

                if (ImGui.IsItemVisible())
                {
                    if (CFG.Current.TimeActEditor_DisplayEventRow_EnumInfo)
                        Decorator.DisplayEnumInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_ParamRefInfo)
                        Decorator.DisplayParamRefInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo)
                        Decorator.DisplayAliasEnumInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_ProjectEnumInfo)
                        Decorator.DisplayProjectEnumInfo(evt);
                }

                SelectionHandler.ContextMenu.TimeActEventMenu(isSelected, i.ToString());

                if (FocusEvent)
                {
                    FocusEvent = false;

                    ImGui.SetScrollHereY();
                }
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }

    public void TimeActEventPropertiesView()
    {
        ImGui.Begin("Properties##TimeActEventProperties");

        if (!SelectionHandler.HasSelectedTimeActEvent())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventPropertyFilter", ref TimeActFilters._timeActEventPropertyFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventPropertyList");

        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.Columns(3);
        }
        else
        {
            ImGui.Columns(2);
        }
        
        // Property Column
        ImGui.AlignTextToFramePadding();
        ImGui.Selectable($@"Start Time##taeEventProperty_StartTime", false);

        ImGui.AlignTextToFramePadding();
        ImGui.Selectable($@"End Time##taeEventProperty_StartTime", false);

        for (int i = 0; i < SelectionHandler.CurrentTimeActEvent.Parameters.ParameterValues.Count; i++)
        {
            var property = SelectionHandler.CurrentTimeActEvent.Parameters.ParameterValues.ElementAt(i).Key;

            if (TimeActFilters.TimeActEventPropertyFilter(SelectionHandler.ContainerInfo, property))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActEventPropertyIndex)
                {
                    isSelected = true;
                }

                ImGui.AlignTextToFramePadding();
                if (ImGui.Selectable($@"{property}##taeEventProperty{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActEventPropertyChange(property, i);
                }

                SelectionHandler.ContextMenu.TimeActEventPropertiesMenu(isSelected, i.ToString());

                Decorator.HandleNameColumn(property);
            }
        }

        ImGui.NextColumn();

        // Value Column
        FieldPropertyHandler.ValueSection(SelectionHandler);
        
        // Type Column
        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("f32");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("f32");

            for (int i = 0; i < SelectionHandler.CurrentTimeActEvent.Parameters.ParameterValues.Count; i++)
            {
                var property = SelectionHandler.CurrentTimeActEvent.Parameters.ParameterValues.ElementAt(i).Key;
                var propertyType = SelectionHandler.CurrentTimeActEvent.Parameters.GetParamValueType(property);

                if (TimeActFilters.TimeActEventPropertyFilter(SelectionHandler.ContainerInfo, property))
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{propertyType}");

                    Decorator.HandleTypeColumn(property);

                }
            }
        }

        ImGui.Columns(1);

        ImGui.EndChild();

        ImGui.End();
    }


    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", KeyBindings.Current.CORE_UndoAction.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", KeyBindings.Current.CORE_RedoAction.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionSubMenu.DisplayMenu();

        ImGui.Separator();

        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("TAE Files"))
            {
                CFG.Current.Interface_TimeActEditor_ContainerFileList = !CFG.Current.Interface_TimeActEditor_ContainerFileList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_ContainerFileList);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Time Acts"))
            {
                CFG.Current.Interface_TimeActEditor_TimeActList = !CFG.Current.Interface_TimeActEditor_TimeActList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_TimeActList);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Animations"))
            {
                CFG.Current.Interface_TimeActEditor_AnimationList = !CFG.Current.Interface_TimeActEditor_AnimationList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_AnimationList);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Animation Properties"))
            {
                CFG.Current.Interface_TimeActEditor_AnimationProperties = !CFG.Current.Interface_TimeActEditor_AnimationProperties;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_AnimationProperties);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Events"))
            {
                CFG.Current.Interface_TimeActEditor_EventList = !CFG.Current.Interface_TimeActEditor_EventList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_EventList);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Event Properties"))
            {
                CFG.Current.Interface_TimeActEditor_EventProperties = !CFG.Current.Interface_TimeActEditor_EventProperties;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_EventProperties);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_TimeActEditor_ToolConfiguration = !CFG.Current.Interface_TimeActEditor_ToolConfiguration;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_ToolConfiguration);

            /*
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport"))
            {
                CFG.Current.Interface_Editor_Viewport = !CFG.Current.Interface_Editor_Viewport;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_Editor_Viewport);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport Grid"))
            {
                CFG.Current.Interface_TimeActEditor_Viewport_Grid = !CFG.Current.Interface_TimeActEditor_Viewport_Grid;
                CFG.Current.TimeActEditor_Viewport_RegenerateMapGrid = true;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Viewport_Grid);
            */

            ImGui.EndMenu();
        }

    }

    public void OnProjectChanged()
    {
        SelectionHandler.OnProjectChanged();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            ActionSubMenu.OnProjectChanged();
        }

        if (AnimationBank.IsLoaded)
        {
            AnimationBank.IsLoaded = false;
            AnimationBank.IsTemplatesLoaded = false;
            AnimationBank.IsCharacterTimeActsLoaded = false;
            AnimationBank.IsObjectTimeActsLoaded = false;

            AnimationBank.Load();
        }
        ResetActionManager();
        //_universe.UnloadAll(true);
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeAct(SelectionHandler.ContainerInfo, SelectionHandler.ContainerBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeActs();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    /*
    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }
    */
}
