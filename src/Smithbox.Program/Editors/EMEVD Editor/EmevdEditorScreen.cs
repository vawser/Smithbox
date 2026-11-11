using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The EMEVD Editor: event scripts as Scratch-style blocks, using Smithbox's own instruction layouts
/// (Assets/EMEVD/Layouts). P1: read-only flat blocks; see Claude/Documentation/EMEVD Block Editor Analysis.md for
/// the phases.
/// </summary>
public class EmevdEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public EmevdEditorView View;

    public EmevdEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new EmevdEditorView(this, project);
    }

    public string EditorName => "EMEVD Editor##EmevdEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    private EmevdBank Bank => Project.Handler.EmevdData?.PrimaryBank;

    public void OnGUI(string[] commands)
    {
        MonitorShortcuts();
        ParseCommands(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_EmevdEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_EmevdEditor);

        View.Display(dsid);
    }

    /// <summary>
    /// emevd/select/&lt;file&gt;[/&lt;event id&gt;[/&lt;instruction index&gt;]], e.g. emevd/select/m10_00_00_00/10000/3
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        if (commands == null || commands.Length < 2 || commands[0] != "select" || Bank == null)
            return;

        var file = Bank.FindFile(commands[1]);
        if (file == null)
            return;

        View.Selection.SelectFile(file);
        View.FileList.ScrollToSelection = true;

        if (commands.Length > 2 && long.TryParse(commands[2], out var id) && file.EnsureLoaded())
        {
            var ev = file.FindEvent(id);
            var index = commands.Length > 3 && int.TryParse(commands[3], out var i) ? i : -1;

            if (ev != null)
                View.Selection.JumpTo(file, ev, index);
        }
    }

    private void MonitorShortcuts()
    {
        if (!FocusManager.IsInEmevdEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        if (View.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            View.ActionManager.UndoAction();

        if (View.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            View.ActionManager.RedoAction();

        // Block edits: from the Script window, and not while typing in a box (Delete edits the text)
        if (FocusManager.Focus != EditorFocusContext.EmevdEditor_Script || ImGui.GetIO().WantTextInput)
            return;

        var s = View.Structure;

        if (InputManager.IsPressed(KeybindID.Delete))
            s.Delete();
        if (InputManager.IsPressed(KeybindID.Duplicate))
            s.Duplicate();
        if (InputManager.IsPressed(KeybindID.Copy))
            s.Copy();
        if (InputManager.IsPressed(KeybindID.Paste))
            s.Paste();
        if (InputManager.IsPressed(KeybindID.Reorder_Up))
            s.Step(false);
        if (InputManager.IsPressed(KeybindID.Reorder_Down))
            s.Step(true);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EMEVD_Menubar_Save_TT"));

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.EmevdEditor_Allow_Unmodified_Save = !CFG.Current.EmevdEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.EmevdEditor_Allow_Unmodified_Save);

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.EmevdEditor_Allow_Automatic_Save = !CFG.Current.EmevdEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.EmevdEditor_Allow_Automatic_Save);

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)}", false, View.ActionManager.CanUndo()))
            {
                View.ActionManager.UndoAction();
            }

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)}", false, View.ActionManager.CanRedo()))
            {
                View.ActionManager.RedoAction();
            }

            ImGui.Separator();
            EmevdBlockView.EditItems(View.Structure);

            ImGui.Separator();
            if (ImGui.BeginMenu($"{LOC.Get("EMEVD_Event_Menu")}##emevdEventSubmenu"))
            {
                View.EventCommands.MenuItems();
                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_View_Toggle_Structured")}##structuredToggle"))
            {
                CFG.Current.EmevdEditor_Structured_View = !CFG.Current.EmevdEditor_Structured_View;
            }
            GUI.Tooltip(LOC.Get("EMEVD_View_Toggle_Structured_TT"));
            GUI.ShowActiveStatus(CFG.Current.EmevdEditor_Structured_View);

            if (ImGui.MenuItem($"{LOC.Get("EMEVD_View_Toggle_Arg_Names")}##argNamesToggle"))
            {
                CFG.Current.EmevdEditor_Show_Arg_Names = !CFG.Current.EmevdEditor_Show_Arg_Names;
            }
            GUI.Tooltip(LOC.Get("EMEVD_View_Toggle_Arg_Names_TT"));
            GUI.ShowActiveStatus(CFG.Current.EmevdEditor_Show_Arg_Names);

            ImGui.Separator();

            var structured = CFG.Current.EmevdEditor_Structured_View && View.BlockView.CurrentScript != null;
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_View_Collapse_All")}##collapseAll", "", false, structured))
                View.BlockView.SetAllCollapsed(true);
            GUI.Tooltip(LOC.Get("EMEVD_View_Collapse_All_TT"));

            if (ImGui.MenuItem($"{LOC.Get("EMEVD_View_Expand_All")}##expandAll", "", false, structured))
                View.BlockView.SetAllCollapsed(false);
            GUI.Tooltip(LOC.Get("EMEVD_View_Expand_All_TT"));

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("EMEVD_View_Toggle_Tools")}##toolWindowToggle"))
            {
                CFG.Current.Interface_EmevdEditor_ToolWindow = !CFG.Current.Interface_EmevdEditor_ToolWindow;
            }
            GUI.Tooltip(LOC.Get("EMEVD_View_Toggle_Tools_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_ToolWindow);

            ImGui.EndMenu();
        }
    }

    public List<string> GetUnsavedFiles()
    {
        return Bank?.GetDirtyFiles() ?? new();
    }

    public void SaveUnsaved()
    {
        Bank?.SaveAllDirty();
        Smithbox.Instance.SaveConfiguration();
    }

    public void Save()
    {
        Save(false);
    }

    /// <summary>
    /// Saves every .emevd with unsaved changes. With Allow Unmodified Save, the selected file is written even if it
    /// has none (useful to check a save reproduces the game's file).
    /// </summary>
    public void Save(bool autoSave)
    {
        var bank = Bank;

        if (bank != null && (!autoSave || CFG.Current.EmevdEditor_Allow_Automatic_Save))
        {
            var selected = View.Selection.SelectedFile;

            if (!autoSave && selected is { IsLoaded: true, IsDirty: false } && CFG.Current.EmevdEditor_Allow_Unmodified_Save)
                selected.IsDirty = true;

            if (bank.GetDirtyFiles().Count == 0)
            {
                if (!autoSave)
                    Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", selected?.Name ?? LOC.Get("EMEVD_Editor_Stub")));
            }
            else
            {
                bank.SaveAllDirty();
            }
        }

        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
