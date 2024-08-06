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
using static StudioCore.Editors.TimeActEditor.AnimationBank;

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

    public PropertyHandler PropertyHandler;
    public TimeActDecorator Decorator;
    public EventGraph EventGraph;

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
        PropertyHandler = new PropertyHandler(EditorActionManager, this, Decorator);
        EventGraph = new EventGraph(EditorActionManager, this, SelectionHandler);

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

        if (Smithbox.ProjectType is ProjectType.Undefined or ProjectType.DS2 or ProjectType.DS2S)
        {
            ImGui.Begin("Editor##InvalidTaeEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!AnimationBank.IsLoaded)
            {
                TaskManager.Run(new TaskManager.LiveTask($"Setup Time Act Editor", TaskManager.RequeueType.None, false,
                () =>
                {
                    AnimationBank.LoadTimeActs();
                }));
            }

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

                ImGui.Text($"This editor is still loading.");

                ImGui.End();
            }
        }

        ActionSubMenu.Shortcuts();
        ToolSubMenu.Shortcuts();

        if (CFG.Current.Interface_TimeActEditor_ToolConfiguration)
        {
            ToolWindow.OnGui();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    // Respond to EditorQueue commands
    public void TimeActCommandLine(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 2)
        {
            if (initcmd[0] == "load")
            {
                var file = initcmd[1];
                var taeName = initcmd[2];
                var animationID = initcmd[3];

                // TODO
            }
        }
    }

    public void TimeActEditorShortcuts()
    {
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
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
                        SelectionHandler.FileContainerChange(info, binder, i);
                    }
                    TimeActUtils.DisplayTimeActFileAlias(info.Name);

                    SelectionHandler.ContextMenu.ContainerMenu(isSelected, info.Name);
                }
            }
        }

        if (!(Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6))
        {
            var title = "Objects";

            if(Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                title = "Assets";
            }

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
                            SelectionHandler.FileContainerChange(info, binder, i);
                        }
                        TimeActUtils.DisplayTimeActFileAlias(info.Name);

                        SelectionHandler.ContextMenu.ContainerMenu(isSelected, info.Name);
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
            InternalFileInfo info = SelectionHandler.ContainerInfo.InternalFiles[i];
            TAE entry = SelectionHandler.ContainerInfo.InternalFiles[i].TAE;

            if (TimeActFilters.TimeActFilter(SelectionHandler.ContainerInfo, entry))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActKey || 
                    SelectionHandler.TimeActMultiselect.IsMultiselected(i))
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@"{info.Name}##TimeAct{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActChange(entry, i);
                }

                if (CFG.Current.Interface_TimeActEditor_DisplayTimeActRow_AliasInfo)
                    TimeActUtils.DisplayTimeActAlias(SelectionHandler.ContainerInfo, entry.ID);

                SelectionHandler.ContextMenu.TimeActMenu(isSelected, entry.ID.ToString());
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
                    SelectionHandler.TimeActAnimationMultiselect.IsMultiselected(i))
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@" {entry.ID}##taeAnim{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    EventGraph.ResetGraph();
                    SelectionHandler.TimeActAnimationChange(entry, i);
                }

                if (CFG.Current.Interface_TimeActEditor_DisplayAnimRow_GeneratorInfo)
                    TimeActUtils.DisplayAnimationAlias(SelectionHandler, entry.ID);

                SelectionHandler.ContextMenu.TimeActAnimationMenu(isSelected, entry.ID.ToString());
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

        ImGui.Columns(2);

        // Animation
        ImGui.AlignTextToFramePadding();
        ImGui.Text("ID");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("AnimFileName");

        // Header
        if (anim.MiniHeader.Type == MiniHeaderType.Standard)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text("IsLoopByDefault");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("AllowDelayLoad");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("ImportsHKX");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("ImportHKXSourceAnimID");
        }

        if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
        {

            ImGui.AlignTextToFramePadding();
            ImGui.Text("ImportFromAnimID");
        }

        ImGui.NextColumn();

        // Animation
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{anim.ID}");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{anim.AnimFileName}");

        // Header
        if (anim.MiniHeader.Type == MiniHeaderType.Standard)
        {
            var header = anim.MiniHeader.GetClone() as AnimMiniHeader.Standard;

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{header.IsLoopByDefault}");

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{header.AllowDelayLoad}");

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{header.ImportsHKX}");

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{header.ImportHKXSourceAnimID}");
        }

        if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
        {
            var header = anim.MiniHeader.GetClone() as AnimMiniHeader.ImportOtherAnim;

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{header.ImportFromAnimID}");
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
                    SelectionHandler.TimeActEventMultiselect.IsMultiselected(i))
                {
                    isSelected = true;
                }

                if(SelectFirstEvent)
                {
                    SelectFirstEvent = false;
                    SelectionHandler.TimeActEventChange(evt, i);
                }

                if (ImGui.Selectable($@" {evt.TypeName}##taeEvent{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActEventChange(evt, i);
                }

                if(CFG.Current.Interface_TimeActEditor_DisplayEventRow_EnumInfo)
                    Decorator.DisplayEnumInfo(evt);

                if (CFG.Current.Interface_TimeActEditor_DisplayEventRow_ParamRefInfo)
                    Decorator.DisplayParamRefInfo(evt);

                if (CFG.Current.Interface_TimeActEditor_DisplayEventRow_DataAliasInfo)
                    Decorator.DisplayAliasEnumInfo(evt);

                if (CFG.Current.Interface_TimeActEditor_DisplayEventRow_ProjectEnumInfo)
                    Decorator.DisplayProjectEnumInfo(evt);

                SelectionHandler.ContextMenu.TimeActEventMenu(isSelected, i.ToString());
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

        // Type Column
        if (CFG.Current.Interface_TimeActEditor_DisplayPropertyType)
        {
            ImGui.Columns(3);

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

            ImGui.NextColumn();
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
        PropertyHandler.ValueSection(SelectionHandler);
        
        ImGui.Columns(1);

        ImGui.EndChild();

        ImGui.End();
    }


    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", KeyBindings.Current.Core_Undo.HintText, false,
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
            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ActionSubMenu.DisplayMenu();
        ToolSubMenu.DisplayMenu();

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
            AnimationBank.LoadTimeActs();

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
