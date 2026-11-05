using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

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

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();

                ImGui.Text(LOC.Get("SYS_OpenGL_Viewport_Blocked"));
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
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_Save_TT"));

            // Save All
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##saveAllAction"))
            {
                SaveAll();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            var activeView = ViewHandler.ActiveView;

            if (activeView != null)
            {
                var transfer = activeView.ToolView.DataTransferTool.Commands;

                // Import from File
                transfer.ImportMenuItem();

                // Export GPARAM to File
                if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Export_Action_Gparam")}##exportGparamAction", "", false,
                    transfer.CanCopy(GparamTransferKind.Gparam)))
                {
                    transfer.ExportToFile(GparamTransferKind.Gparam);
                }
                GUI.Tooltip(LOC.Get("GPARAM_Transfer_Export_Action_TT"));

                ImGui.Separator();
            }

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.GparamEditor_Allow_Unmodified_Save = !CFG.Current.GparamEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.GparamEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.GparamEditor_Allow_Automatic_Save = !CFG.Current.GparamEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.GparamEditor_Allow_Automatic_Save);

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

                var transfer = activeView.ToolView.DataTransferTool.Commands;

                // Copy GPARAM
                if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Copy_Action_Gparam")}##copyGparamAction",
                    "", false, transfer.CanCopy(GparamTransferKind.Gparam)))
                {
                    transfer.Copy(GparamTransferKind.Gparam);
                }
                GUI.Tooltip(LOC.Get("GPARAM_Transfer_Copy_Action_TT"));

                // Paste, Paste Special (Import from File is in the File menu)
                transfer.PasteMenuItems(false);

                ImGui.Separator();

                // Groups
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Groups_Header")}##groupsHeader"))
                {
                    // Copy Group
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Copy_Action_Group")}##copyGroupAction",
                        InputManager.GetHint(KeybindID.Copy), false, transfer.CanCopy(GparamTransferKind.Group)))
                    {
                        transfer.Copy(GparamTransferKind.Group);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Transfer_Copy_Action_TT"));

                    // Add All Missing Groups
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Groups_Add_All_Missing")}##addAllMissingAction", "", false, activeView.GroupListView.CanAddGroups()))
                    {
                        activeView.GroupListView.AddGroupsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Groups_Add_All_Missing_TT"));

                    // Delete Selected Group
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Groups_Delete")}##deleteGroupAction", InputManager.GetHint(KeybindID.Delete), false, activeView.GroupListView.CanDeleteGroups()))
                    {
                        activeView.GroupListView.DeleteGroupsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Groups_Delete_TT"));

                    ImGui.EndMenu();
                }

                // Fields
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Fields_Header")}##fieldsHeader"))
                {
                    // Copy Field
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Copy_Action_Field")}##copyFieldAction",
                        InputManager.GetHint(KeybindID.Copy), false, transfer.CanCopy(GparamTransferKind.Field)))
                    {
                        transfer.Copy(GparamTransferKind.Field);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Transfer_Copy_Action_TT"));

                    // Add All Missing Fields
                    // No shortcut hint: the Add key opens the field list's add menu, to pick which to add
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Fields_Add_All_Missing")}##addAllMissingFieldsAction", "", false, activeView.FieldListView.CanAddFields()))
                    {
                        activeView.FieldListView.AddFieldsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Fields_Add_All_Missing_TT"));

                    // Delete
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Fields_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete), false, activeView.FieldListView.CanDeleteFields()))
                    {
                        activeView.FieldListView.DeleteFieldsShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Fields_Delete_TT"));

                    ImGui.EndMenu();
                }

                // Values
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Values_Header")}##valuesHeader"))
                {
                    // Copy Values
                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Copy_Action_Values")}##copyValuesAction",
                        InputManager.GetHint(KeybindID.Copy), false, transfer.CanCopy(GparamTransferKind.Values)))
                    {
                        transfer.Copy(GparamTransferKind.Values);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Transfer_Copy_Action_TT"));

                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Values_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate), false, activeView.FieldValueListView.CanAffectValues()))
                    {
                        activeView.FieldValueListView.AddValuesShortcut();
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_Values_Duplicate_TT"));

                    if (ImGui.MenuItem($"{LOC.Get("GPARAM_Values_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete), false, activeView.FieldValueListView.CanAffectValues()))
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

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        foreach (var view in ViewHandler.TexViews)
        {
            if (view == null || view == ViewHandler.ViewToClose)
                continue;

            if (view.ViewportWindow.Viewport is VulkanViewport vulkanViewport && vulkanViewport.Visible)
            {
                view.ViewportWindow.Draw(device, cl);
            }
        }
    }

    public void Update(float dt)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.ViewportWindow.Update(dt);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        foreach (var view in ViewHandler.TexViews)
        {
            view?.ViewportWindow.EditorResized(window, device);
        }
    }

    public bool InputCaptured()
    {
        var activeView = ViewHandler.ActiveView;

        return activeView != null && activeView.ViewportWindow.InputCaptured();
    }

    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if(autoSave && CFG.Current.GparamEditor_Allow_Automatic_Save || !autoSave)
        {
            var targetScript = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == activeView.Selection.SelectedFileEntry.Filename && e.Key.Extension == activeView.Selection.SelectedFileEntry.Extension);

            if (targetScript.Key != null)
            {
                await Project.Handler.GparamData.PrimaryBank.SaveGraphicsParam(targetScript.Key, targetScript.Value);
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public async void SaveAll(bool autoSave = false)
    {
        if (autoSave && CFG.Current.GparamEditor_Allow_Automatic_Save || !autoSave)
        {
            await Project.Handler.GparamData.PrimaryBank.SaveAllGraphicsParams();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
