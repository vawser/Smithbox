using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Logger;
using System;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public TextViewHandler ViewHandler;

    public TextCommandQueue CommandQueue;
    public TextShortcuts Shortcuts;

    public TextEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new TextViewHandler(this, project);

        Shortcuts = new TextShortcuts(this, Project);
        CommandQueue = new TextCommandQueue(this, Project);
    }

    public string EditorName => "Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();
        var activeView = ViewHandler.ActiveView;

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            if (activeView != null)
            {
                activeView.ToolView.DisplayDropdown();
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TextEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_TextEditor);

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
                // FMG
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_FMG")}##manualToggle_fmg"))
                {
                    CFG.Current.TextEditor_ManualSave_IncludeFMG = !CFG.Current.TextEditor_ManualSave_IncludeFMG;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_FMG_TT"));
                GUI.ShowActiveStatus(CFG.Current.TextEditor_ManualSave_IncludeFMG);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Manual_Save_Output_TT"));

            // Automatic Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Auto_Save_Output")}##autoSaveMenuHeader"))
            {
                // FMG
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_FMG")}##autoToggle_fmg"))
                {
                    CFG.Current.TextEditor_AutomaticSave_IncludeFMG = !CFG.Current.TextEditor_AutomaticSave_IncludeFMG;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_FMG_TT"));
                GUI.ShowActiveStatus(CFG.Current.TextEditor_AutomaticSave_IncludeFMG);

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

                // Create
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_Action_Create")}##createAction", InputManager.GetHint(KeybindID.TextEditor_Create_New_Entry)))
                {
                    activeView.TextEntryCreator.ShowModal = true;
                }
                GUI.Tooltip(LOC.Get("TEXT_Menubar_Action_Create_TT"));

                // Duplicate
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_Action_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    activeView.ActionHandler.DuplicateEntries();
                }
                GUI.Tooltip(LOC.Get("TEXT_Menubar_Action_Duplicate_TT"));

                // Delete
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_Action_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete)))
                {
                    activeView.ActionHandler.DeleteEntries();
                }
                GUI.Tooltip(LOC.Get("TEXT_Menubar_Action_Delete_TT"));
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
            if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_View_Tool_Window")}##toolWindowToggle"))
            {
                CFG.Current.Interface_TextEditor_ToolWindow = !CFG.Current.Interface_TextEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TextEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Save currently selected FMG container
    /// </summary>
    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            var fileEntry = activeView.Selection.SelectedFileDictionaryEntry;
            var wrapper = activeView.Selection.SelectedContainerWrapper;

            if (fileEntry == null)
            {
                return;
            }

            if (wrapper == null)
            {
                return;
            }

            if (!autoSave && CFG.Current.TextEditor_ManualSave_IncludeFMG ||
                autoSave && CFG.Current.TextEditor_AutomaticSave_IncludeFMG)
            {
                try
                {
                    if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                    {
                        await Project.Handler.TextData.PrimaryBank.SaveLooseFmg(fileEntry, wrapper);
                    }
                    else
                    {
                        await Project.Handler.TextData.PrimaryBank.SaveFmgContainer(fileEntry, wrapper);
                    }

                    Smithbox.Log(this, LOC.Get("TEXT_Save_File_PASS", fileEntry.Path));
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("TEXT_Save_File_FAIL", fileEntry.Path), ex);
                }
            }

            // Save the configuration JSONs
            Smithbox.Instance.SaveConfiguration();
        }
    }

    /// <summary>
    /// Save all modified FMG containers
    /// </summary>
    public async void SaveAll(bool autoSave = false)
    {
        if (!autoSave && CFG.Current.TextEditor_ManualSave_IncludeFMG ||
            autoSave && CFG.Current.TextEditor_AutomaticSave_IncludeFMG)
        {
            try
            {
                await Project.Handler.TextData.PrimaryBank.SaveTextFiles();

                Smithbox.Log(this, LOC.Get("TEXT_Save_All_File_PASS"));
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("TEXT_Save_All_File_FAIL"), ex);
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
