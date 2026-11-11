using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The Behavior Editor: AC6 .bhv state machines (chr behbnds). P2: field and hex editing with undo; see
/// Claude/Documentation/BHV Editor Analysis.md for the phases.
/// </summary>
public class BehaviorEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public BhvEditorView View;

    public BehaviorEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new BhvEditorView(this, project);
    }

    public string EditorName => "Behavior Editor##BehaviorEditor";
    public string CommandEndpoint => "behavior";
    public string SaveType => "Behavior";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    private BhvBank Bank => Project.Handler.BehaviorData?.PrimaryBank;

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

        var dsid = ImGui.GetID("DockSpace_BehaviorEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_BehaviorEditor);

        View.Display(dsid);
    }

    /// <summary>
    /// behavior/select/&lt;behbnd&gt;/&lt;file&gt;[/&lt;state index&gt;], e.g. behavior/select/c0200/w315.bhv/11
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        if (commands == null || commands.Length < 3 || commands[0] != "select" || Bank == null)
            return;

        var binder = Bank.Binders.FirstOrDefault(b => string.Equals(b.Name, commands[1], StringComparison.OrdinalIgnoreCase));
        var file = binder?.Files.FirstOrDefault(f => string.Equals(f.Name, commands[2], StringComparison.OrdinalIgnoreCase));

        if (file == null)
            return;

        View.Selection.SelectFile(file);
        View.FileList.ScrollToSelection = true;

        if (commands.Length > 3 && int.TryParse(commands[3], out var state))
            View.Selection.SelectState(state, true);
    }

    private void MonitorShortcuts()
    {
        if (!FocusManager.IsInBehaviorEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        if (View.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            View.ActionManager.UndoAction();

        if (View.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            View.ActionManager.RedoAction();

        // Not while typing in a box (Delete edits the text), nor from the Tools window
        if (!ImGui.IsAnyItemActive() && FocusManager.Focus != EditorFocusContext.BehaviorEditor_Tools)
        {
            if (InputManager.IsPressed(KeybindID.Duplicate))
                View.Structure.DuplicateSelected();

            if (InputManager.IsPressed(KeybindID.Delete))
                View.Structure.DeleteSelected();
        }
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_Save_TT"));

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.BehaviorEditor_Allow_Unmodified_Save = !CFG.Current.BehaviorEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.BehaviorEditor_Allow_Unmodified_Save);

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.BehaviorEditor_Allow_Automatic_Save = !CFG.Current.BehaviorEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.BehaviorEditor_Allow_Automatic_Save);

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

            if (View.Selection.SelectedFile != null)
            {
                ImGui.Separator();
                View.Structure.SelectionItems();
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("BHV_View_Toggle_Hex")}##hexToggle"))
            {
                CFG.Current.BehaviorEditor_Properties_Show_Hex = !CFG.Current.BehaviorEditor_Properties_Show_Hex;
            }
            GUI.Tooltip(LOC.Get("BHV_View_Toggle_Hex_TT"));
            GUI.ShowActiveStatus(CFG.Current.BehaviorEditor_Properties_Show_Hex);

            if (ImGui.MenuItem($"{LOC.Get("BHV_View_Toggle_Game_Order")}##gameOrderToggle"))
            {
                CFG.Current.BehaviorEditor_Transitions_Game_Order = !CFG.Current.BehaviorEditor_Transitions_Game_Order;
            }
            GUI.Tooltip(LOC.Get("BHV_View_Toggle_Game_Order_TT"));
            GUI.ShowActiveStatus(CFG.Current.BehaviorEditor_Transitions_Game_Order);

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("BHV_View_Toggle_Tools")}##toolWindowToggle"))
            {
                CFG.Current.Interface_BehaviorEditor_ToolWindow = !CFG.Current.Interface_BehaviorEditor_ToolWindow;
            }
            GUI.Tooltip(LOC.Get("BHV_View_Toggle_Tools_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_ToolWindow);

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

    /// <summary>
    /// Saves every behbnd with unsaved .bhv changes. With Allow Unmodified Save, the selected file is written even
    /// if it has none (useful to check a save reproduces the game's file).
    /// </summary>
    public void Save(bool autoSave = false)
    {
        var bank = Bank;

        if (bank != null && (!autoSave || CFG.Current.BehaviorEditor_Allow_Automatic_Save))
        {
            var selected = View.Selection.SelectedFile;

            if (!autoSave && selected != null && !selected.IsDirty && CFG.Current.BehaviorEditor_Allow_Unmodified_Save)
            {
                selected.IsDirty = true;
            }

            if (bank.GetDirtyFiles().Count == 0)
            {
                if (!autoSave)
                    Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", selected?.Key ?? LOC.Get("BHV_Editor_Stub")));
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
