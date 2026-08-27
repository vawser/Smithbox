using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public GparamViewHandler ViewHandler;

    public GparamShortcuts Shortcuts;
    public GparamCommandQueue CommandQueue;


    public GparamEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new GparamViewHandler(this, project);

        Shortcuts = new GparamShortcuts(this, project);
        CommandQueue = new GparamCommandQueue(this, Project);
    }

    public string EditorName => "";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";
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
                activeView.ToolView.DisplayDropdown();
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_GparamEditor);

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

            // Save All
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##saveAllAction"))
            {
                SaveAll();
            }

            ImGui.Separator();

            // Manual Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Manual_Save_Output")}##manualSaveMenuHeader"))
            {
                // GPARAM
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_GPARAM")}##manualToggle_gparam"))
                {
                    CFG.Current.GparamEditor_ManualSave_IncludeGPARAM = !CFG.Current.GparamEditor_ManualSave_IncludeGPARAM;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_GPARAM_TT"));
                GUI.ShowActiveStatus(CFG.Current.GparamEditor_ManualSave_IncludeGPARAM);


                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Manual_Save_Output_TT"));

            // Automatic Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Auto_Save_Output")}##autoSaveMenuHeader"))
            {
                // GPARAM
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_GPARAM")}##manualToggle_gparam"))
                {
                    CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM = !CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_GPARAM_TT"));
                GUI.ShowActiveStatus(CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM);

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

                ImGui.Separator();

                // Groups
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Groups_Header")}##groupsHeader"))
                {
                    // Add All Missing Groups
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Groups_Add_All_Missing")}##addAllMissingAction", InputManager.GetHint(KeybindID.Add)))
                    {
                        activeView.GroupListView.AddGroupsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Groups_Add_All_Missing_TT"));

                    // Delete Selected Group
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Groups_Delete")}##deleteGroupAction", InputManager.GetHint(KeybindID.Delete)))
                    {
                        activeView.GroupListView.DeleteGroupsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Groups_Delete_TT"));

                    ImGui.EndMenu();
                }

                // Fields
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Fields_Header")}##fieldsHeader"))
                {
                    // Duplicate
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Fields_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
                    {
                        activeView.FieldListView.AddFieldsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Fields_Duplicate_TT"));

                    // Delete
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Fields_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete)))
                    {
                        activeView.FieldListView.DeleteFieldsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Fields_Delete_TT"));

                    ImGui.EndMenu();
                }

                // Values
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Values_Header")}##valuesHeader"))
                {
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Values_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
                    {
                        activeView.FieldValueListView.AddValuesShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Values_Duplicate_TT"));

                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Values_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete)))
                    {
                        activeView.FieldValueListView.DeleteValuesShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Values_Delete_TT"));

                    ImGui.EndMenu();
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
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_View_Toggle_Tools")}##toggleTools"))
            {
                CFG.Current.Interface_GparamEditor_ToolWindow = !CFG.Current.Interface_GparamEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_ToolWindow);

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

        if (!autoSave && CFG.Current.GparamEditor_ManualSave_IncludeGPARAM ||
            autoSave && CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM)
        {
            var targetScript = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == activeView.Selection.SelectedFileEntry.Filename && e.Key.Extension == activeView.Selection.SelectedFileEntry.Extension);

            if (targetScript.Key != null)
            {
                await Project.Handler.GparamData.PrimaryBank.SaveGraphicsParam(targetScript.Key, targetScript.Value);

                Smithbox.Log(this,
                    LOC.Get("GPARAM_Data_Save_File", targetScript.Key.Filename));
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public async void SaveAll(bool autoSave = false)
    {
        if (!autoSave && CFG.Current.GparamEditor_ManualSave_IncludeGPARAM ||
            autoSave && CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM)
        {
            await Project.Handler.GparamData.PrimaryBank.SaveAllGraphicsParams();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
