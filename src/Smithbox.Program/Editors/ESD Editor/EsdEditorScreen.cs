using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The ESD Editor: EzState machines (NPC talk, character, menu, DS2 AI/event scripts) as blocks, with names from
/// Smithbox's catalogue (Assets/ESD/Catalogues, from ESDLang's documentation, used with permission). P2: the state
/// view edits values, targets, commands, transitions and states, with undo and save; see
/// Claude/Documentation/ESD Block Editor Analysis.md for the phases.
/// </summary>
public class EsdEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public EsdEditorView View;

    public EsdEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new EsdEditorView(this, project);
    }

    public string EditorName => "ESD Editor##EsdEditor";
    public string CommandEndpoint => "esd";
    public string SaveType => "ESD";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    private EsdBank Bank => Project.Handler.EsdData?.PrimaryBank;

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

        var dsid = ImGui.GetID("DockSpace_EsdEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_EsdEditor);

        View.Display(dsid);
    }

    /// <summary>
    /// esd/select/&lt;container&gt;[/&lt;esd&gt;[/&lt;machine id&gt;]], e.g.
    /// esd/select/m10_00_00_00.talkesdbnd/t100000/1 or esd/select/c0000//1 for a loose file
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        if (commands == null || commands.Length < 2 || commands[0] != "select" || Bank == null)
            return;

        var container = Bank.FindContainer(commands[1]);
        if (container == null || !container.EnsureLoaded())
            return;

        View.Selection.SelectContainer(container);
        View.FileList.ScrollToSelection = true;

        if (commands.Length > 2 && !string.IsNullOrEmpty(commands[2]))
        {
            var file = container.Files.FirstOrDefault(f => f.Name.Equals(commands[2], StringComparison.OrdinalIgnoreCase));
            if (file != null)
                View.Selection.SelectFile(file);
        }

        if (commands.Length > 3 && long.TryParse(commands[3], out var machine))
            View.Selection.SelectMachine(machine);
    }

    private void MonitorShortcuts()
    {
        if (!FocusManager.IsInEsdEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        if (View.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            View.ActionManager.UndoAction();

        if (View.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            View.ActionManager.RedoAction();

        // Block edits: from the States window, and not while typing in a box (Delete edits the text)
        if (FocusManager.Focus != EditorFocusContext.EsdEditor_States || ImGui.GetIO().WantTextInput)
            return;

        if (InputManager.IsPressed(KeybindID.Delete))
            View.Commands.Delete();
        if (InputManager.IsPressed(KeybindID.Duplicate))
            View.Commands.Duplicate();
        if (InputManager.IsPressed(KeybindID.Reorder_Up))
            View.Commands.Move(-1);
        if (InputManager.IsPressed(KeybindID.Reorder_Down))
            View.Commands.Move(1);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            var dirty = Bank?.GetDirtyFiles().Count ?? 0;
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}", false, dirty > 0))
                Save();
            GUI.Tooltip(LOC.Get("ESD_Menubar_Save_TT", dirty));

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)}", false, View.ActionManager.CanUndo()))
                View.ActionManager.UndoAction();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)}", false, View.ActionManager.CanRedo()))
                View.ActionManager.RedoAction();

            ImGui.Separator();

            // The selected block's own menu (the same as right-clicking it)
            var row = View.Commands.SelectedRow;
            if (row != null)
                View.Commands.ContextMenu(row);
            else
                ImGui.TextDisabled(LOC.Get("ESD_Edit_Select_Block"));

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            // Code view
            if (ImGui.MenuItem($"{LOC.Get("ESD_Code_Toggle")}##esdCodeViewToggle"))
                CFG.Current.EsdEditor_Code_View = !CFG.Current.EsdEditor_Code_View;
            GUI.Tooltip(LOC.Get("ESD_Code_Toggle_TT"));
            GUI.ShowActiveStatus(CFG.Current.EsdEditor_Code_View);

            // Tools window
            if (ImGui.MenuItem($"{LOC.Get("ESD_View_Toggle_Tools")}##esdToolWindowToggle"))
                CFG.Current.Interface_EsdEditor_ToolWindow = !CFG.Current.Interface_EsdEditor_ToolWindow;
            GUI.ShowActiveStatus(CFG.Current.Interface_EsdEditor_ToolWindow);

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

    public void SaveAll()
    {
        Save();
    }

    /// <summary>Saves every ESD file with unsaved changes into the project</summary>
    public void Save()
    {
        var bank = Bank;
        if (bank == null)
            return;

        if (bank.GetDirtyFiles().Count == 0)
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", View.Selection.SelectedContainer?.Name ?? LOC.Get("ESD_Editor_Stub")));
        else
            bank.SaveAllDirty();

        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
