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

            ImGui.Separator();

            // Manual Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Manual_Save_Output")}##manualSaveMenuHeader"))
            {
                // MTD
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_MTD")}##manualToggle_mtd"))
                {
                    CFG.Current.MaterialEditor_ManualSave_IncludeMTD = !CFG.Current.MaterialEditor_ManualSave_IncludeMTD;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_MTD_TT"));
                GUI.ShowActiveStatus(CFG.Current.MaterialEditor_ManualSave_IncludeMTD);

                // MATBIN
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_MATBIN")}##manualToggle_matbin"))
                {
                    CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN = !CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_MATBIN_TT"));
                GUI.ShowActiveStatus(CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Manual_Save_Output_TT"));

            // Automatic Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Auto_Save_Output")}##autoSaveMenuHeader"))
            {
                // MTD
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_MTD")}##autoToggle_mtd"))
                {
                    CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD = !CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_MTD_TT"));
                GUI.ShowActiveStatus(CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD);

                // MATBIN
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_MATBIN")}##autoToggle_matbin"))
                {
                    CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN = !CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_MATBIN_TT"));
                GUI.ShowActiveStatus(CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Auto_Save_Output_TT"));


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

            if (!autoSave && !CFG.Current.MaterialEditor_ManualSave_IncludeMTD)
                return;

            if (autoSave && !CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD)
                return;
        }

        if (activeView.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            if (activeView.Selection.MATBINWrapper == null)
                return;

            if (activeView.Selection.SelectedMATBIN == null)
                return;

            if (!autoSave && !CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN)
                return;

            if (autoSave && !CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN)
                return;
        }

        Task<bool> saveTask = Project.Handler.MaterialData.PrimaryBank.Save(activeView);
        bool saveTaskResult = await saveTask;

        var displayName = Path.GetFileName(activeView.Selection.SelectedFileKey);

        if (saveTaskResult)
        {
            Smithbox.Log(this, LOC.Get("MAT_Save_Entry", displayName, activeView.Selection.SelectedBinderEntry.Filename));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAT_Failed_Save_Entry", displayName, activeView.Selection.SelectedBinderEntry.Filename));
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
