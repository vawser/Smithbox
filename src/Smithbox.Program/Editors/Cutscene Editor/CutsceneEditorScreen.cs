#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The Cutscene Editor (Cutscene Editor Analysis). Each view (<see cref="CutsceneEditorView" />) plays a cut of an MQB
/// cutscene in its own viewport: its actors on their models, animated by the cut's Havok file, and its camera, which the
/// viewport can look through. The screen keeps what the views share: the menus, saving, commands, the clipboard, the
/// event templates and the session's map and collision choices.
/// </summary>
public class CutsceneEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public CutsceneViewHandler ViewHandler;

    /// <summary>The view the menus, shortcuts and commands act on</summary>
    public CutsceneEditorView? ActiveView => ViewHandler.ActiveView;

    /// <summary>Events of every resource type, from the project's cutscenes, to add new ones from</summary>
    public CutsceneTemplateIndex Templates;

    /// <summary>The map chosen for each cutscene this session, over the one its name implies</summary>
    public readonly Dictionary<string, string> MapForCutscene = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The collision chosen to stand each cutscene on, by cutscene and map, for the session</summary>
    public readonly Dictionary<(string Cutscene, string Map), string> ChosenCollisions = new();

    /// <summary>The cutscene copied in a list, to paste as a new one under another name</summary>
    public CutsceneFile? CopiedFile;

    /// <summary>The cut copied in a list, with its resources and Havok files as they were, to paste into any cutscene</summary>
    public CutsceneCutClipboard? CopiedCut;

    // The view last updated: a view that stops being the active one stops playing
    private CutsceneEditorView? UpdatedView;

    public CutsceneEditorScreen(ProjectEntry project)
    {
        Project = project;

        Templates = new CutsceneTemplateIndex(project, () => Data?.Files ?? new List<CutsceneFile>());

        ViewHandler = new CutsceneViewHandler(this, project);
    }

    public CutsceneData Data => Project.Handler.CutsceneData;

    public string EditorName => "Cutscene Editor";
    public string CommandEndpoint => "cutscene";
    public string SaveType => "Cutscene";
    public string WindowName => "";
    public bool HasDocked { get; set; }

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

            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_CutsceneEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_CutsceneEditor);

        ViewHandler.HandleViews(dsid);
    }

    /// <summary>cutscene/select/&lt;name&gt;[/&lt;cut&gt;]: shows a cutscene by name, in the active view</summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is not { Length: >= 2 } || commands[0] != "select" || ActiveView is not { } view)
            return;

        var file = Data.Files.FirstOrDefault(f => f.Name.Equals(commands[1], StringComparison.OrdinalIgnoreCase));
        if (file == null)
            return;

        var cut = commands.Length >= 3 && int.TryParse(commands[2], out var c) ? c : 0;
        view.Select(file, cut);
        view.ListView.ScrollToSelection = true;
    }

    private void Shortcuts()
    {
        if (!FocusManager.IsInCutsceneEditor() || ActiveView is not { } view)
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        if (view.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            view.ActionManager.UndoAction();

        if (view.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            view.ActionManager.RedoAction();

        view.Shortcuts();
    }

    private void FileMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##cutsceneFileMenu"))
            return;

        var view = ActiveView;

        if (ImGui.MenuItem($"{LOC.Get("CUTSCENE_File_New")}##cutsceneNew", "", false, Data.Files.Count > 0 && view != null))
            view!.ListView.NewFile();
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_New_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##cutsceneSave", InputManager.GetHint(KeybindID.Save), false, view?.SelectedFile?.IsDirty == true || Data.HasFileRemovals))
            Save();
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Save_TT"));

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##cutsceneSaveAll", "", false, Data.Files.Any(f => f.IsDirty) || Data.HasFileRemovals))
            SaveAll();
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_SaveAll_TT"));

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##cutsceneEditMenu"))
            return;

        if (ActiveView is { } view)
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##cutsceneUndo", InputManager.GetHint(KeybindID.Undo), false, view.ActionManager.CanUndo()))
                view.ActionManager.UndoAction();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##cutsceneRedo", InputManager.GetHint(KeybindID.Redo), false, view.ActionManager.CanRedo()))
                view.ActionManager.RedoAction();
        }

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

        var scene = ActiveView?.Scene;
        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Menu_Log_Cut"), "", false, scene != null))
            Smithbox.Log(this, scene!.Describe());
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Log_Cut_TT"));

        ImGui.Separator();

        ViewHandler.DisplayMenu();

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

    // ---- viewport

    public void Update(float dt)
    {
        // Only the active view's viewport takes input and plays, as in the Time Act Editor
        var view = ActiveView;

        // The view left stops playing, or its voice and sounds would run on with nothing updating them
        if (UpdatedView != view)
        {
            if (UpdatedView != null && ViewHandler.OpenViews.Contains(UpdatedView))
                UpdatedView.Suspend();
            UpdatedView = view;
        }

        view?.Update(dt);
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewHandler.ViewToClose == null)
        {
            foreach (var view in ViewHandler.OpenViews)
                view.Draw(device, cl);
        }

        // Done here so the list isn't changed while the views are drawn
        if (ViewHandler.ViewToClose != null)
        {
            if (UpdatedView == ViewHandler.ViewToClose)
                UpdatedView = null;

            ViewHandler.RemoveView(ViewHandler.ViewToClose);
            ViewHandler.ViewToClose = null;
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ActiveView?.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ActiveView?.InputCaptured() ?? false;
    }

    // ---- saving

    /// <summary>
    /// Writes the active view's cutscene into the project, if it has changes, and moves the project files of deleted,
    /// renamed and reverted cutscenes into the project's backup folder
    /// </summary>
    public void Save() => SaveFiles(ActiveView?.SelectedFile is { IsDirty: true } file ? [file] : []);

    public void SaveAll() => SaveFiles(Data.Files.Where(f => f.IsDirty).ToList());

    public void SaveFiles(List<CutsceneFile> files)
    {
        // Undoing a file added, renamed or deleted after it is written would leave the list and the project apart
        var filesChange = Data.HasFileRemovals || files.Any(f => f.IsNew);
        var reverting = ViewHandler.OpenViews.Where(v => v.SelectedFile?.PendingRevert == true).ToHashSet();

        // First, so a new file may take a path a removed one leaves
        Data.ApplyRemovals();

        foreach (var file in files)
            file.Save(Project);

        foreach (var view in ViewHandler.OpenViews.ToList())
            view.AfterSave(filesChange, reverting.Contains(view));
    }

    public List<string> GetUnsavedFiles() => Data?.Files.Where(f => f.IsDirty || f.PendingRevert).Select(f => f.Entry.Path)
        .Concat(Data.PendingDeletes.Select(d => d.Path)).Distinct().OrderBy(p => p).ToList() ?? new();

    public void SaveUnsaved() => SaveAll();

    public void Dispose()
    {
        ViewHandler.DisposeViews();
    }
}
