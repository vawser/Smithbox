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

    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (activeView.Selection.SelectedBinderEntry == null)
            return;

        if (activeView.Selection.SelectedFileKey == "")
            return;

        if(activeView.Selection.SourceType is MaterialSourceType.MTD)
        {
            if (activeView.Selection.MTDWrapper == null)
                return;

            if (activeView.Selection.SelectedMTD == null)
                return;
        }

        if (activeView.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            if (activeView.Selection.MATBINWrapper == null)
                return;

            if (activeView.Selection.SelectedMATBIN == null)
                return;
        }

        // Only save if the selected file has been modified
        if (activeView.Selection.SelectedFileWrapper == null)
            return;

        if (autoSave && CFG.Current.MaterialEditor_Allow_Automatic_Save || !autoSave)
        {
            Task<bool> saveTask = Project.Handler.MaterialData.PrimaryBank.Save(activeView);
            bool saveTaskResult = await saveTask;

            var displayName = Path.GetFileName(activeView.Selection.SelectedFileKey);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
