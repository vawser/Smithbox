using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public TextViewHandler ViewHandler;

    public TextCommandQueue CommandQueue;
    public TextShortcuts Shortcuts;

    public TextToolView ToolView;

    public TextEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new TextViewHandler(this, project);

        Shortcuts = new TextShortcuts(this, Project);
        CommandQueue = new TextCommandQueue(this, Project);

        ToolView = new TextToolView(this, Project);
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

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TextEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        ViewHandler.HandleViews();

        if (ViewHandler.ActiveView != null)
        {
            ToolView.Display();
        }
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            if (ImGui.MenuItem($"Save All"))
            {
                SaveAll();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"FMG"))
                {
                    CFG.Current.TextEditor_ManualSave_IncludeFMG = !CFG.Current.TextEditor_ManualSave_IncludeFMG;
                }
                UIHelper.Tooltip("If enabled, the text files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.TextEditor_ManualSave_IncludeFMG);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"FMG"))
                {
                    CFG.Current.TextEditor_AutomaticSave_IncludeFMG = !CFG.Current.TextEditor_AutomaticSave_IncludeFMG;
                }
                UIHelper.Tooltip("If enabled, the text files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.TextEditor_AutomaticSave_IncludeFMG);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the automatic saving process.");

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Edit"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanRedo())
                    {
                        activeView.ActionManager.RedoAction();
                    }
                }
                ImGui.Separator();

                // Create
                if (ImGui.MenuItem("Create", InputManager.GetHint(KeybindID.TextEditor_Create_New_Entry)))
                {
                    activeView.NewEntryModal.ShowModal = true;
                }
                UIHelper.Tooltip($"Create new text entries.");

                // Duplicate
                if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    activeView.ActionHandler.DuplicateEntries();
                }
                UIHelper.Tooltip($"Duplicate the currently selected text entries.");

                // Delete
                if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
                {
                    activeView.ActionHandler.DeleteEntries();
                }
                UIHelper.Tooltip($"Delete the currently selected text entries.");
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_TextEditor_ToolWindow = !CFG.Current.Interface_TextEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void ToolMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            if (ImGui.BeginMenu("Data"))
            {
                activeView.FmgImporter.MenubarOptions();

                ImGui.Separator();

                activeView.FmgExporter.MenubarOptions();

                ImGui.Separator();

                activeView.FmgDumper.MenubarOptions();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Language Sync"))
            {
                activeView.LanguageSync.DisplayMenubarOptions();

                ImGui.EndMenu();
            }

            activeView.LanguageSync.OnGui();
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

                    TaskLogs.AddLog($"[Text Editor] Saved {fileEntry.Path}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"[Text Editor] Failed to save {fileEntry.Path}", Microsoft.Extensions.Logging.LogLevel.Warning, LogPriority.High, ex);
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

                TaskLogs.AddLog($"[Text Editor] Saved all modified text files.");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"[Text Editor] Failed to save all modified text files", Microsoft.Extensions.Logging.LogLevel.Warning, LogPriority.High, ex);
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
