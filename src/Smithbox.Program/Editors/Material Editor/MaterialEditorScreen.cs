using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public MaterialViewHandler ViewHandler;

    public MaterialCommandQueue CommandQueue;
    public MaterialShortcuts Shortcuts;

    public MaterialEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new MaterialViewHandler(this, project);

        CommandQueue = new(this, project);
        Shortcuts = new(this, project);
    }

    public string EditorName => "Material Editor##MaterialEditor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            var activeView = ViewHandler.ActiveView;
            if(activeView != null)
            {
                activeView.ToolView.ToolMenu();
            }

            //OptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_MaterialEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_MaterialEditor);

        ViewHandler.HandleViews(dsid);
    }

    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_Save_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.MaterialEditor_Allow_Unmodified_Save = !CFG.Current.MaterialEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.MaterialEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.MaterialEditor_Allow_Automatic_Save = !CFG.Current.MaterialEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.MaterialEditor_Allow_Automatic_Save);


            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanRedo())
                    {
                        activeView.ActionManager.RedoAction();
                    }
                }
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            // Tools
            if (ImGui.MenuItem($"{LOC.Get("MAT_View_Toggle_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_MaterialEditor_ToolWindow = !CFG.Current.Interface_MaterialEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public List<string> GetUnsavedFiles()
    {
        return Project.Handler.MaterialData?.PrimaryBank?.GetDirtyFiles() ?? new();
    }

    public void SaveUnsaved()
    {
        Project.Handler.MaterialData?.PrimaryBank?.SaveAllDirty();

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    /// <summary>
    /// Saves every material with unsaved changes, whichever is selected. With Allow Unmodified Save,
    /// the selected material is written even if it has none.
    /// </summary>
    public void Save(bool autoSave = false)
    {
        var bank = Project.Handler.MaterialData?.PrimaryBank;

        if (bank != null && (!autoSave || CFG.Current.MaterialEditor_Allow_Automatic_Save))
        {
            var selected = ViewHandler.ActiveView?.Selection.SelectedFileWrapper;

            if (!autoSave && selected != null && !selected.IsDirty && CFG.Current.MaterialEditor_Allow_Unmodified_Save)
            {
                selected.IsDirty = true;
            }

            if (bank.GetDirtyFiles().Count == 0)
            {
                if (!autoSave)
                    Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", selected?.Key ?? LOC.Get("MAT_Editor_Stub")));
            }
            else
            {
                bank.SaveAllDirty();
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
