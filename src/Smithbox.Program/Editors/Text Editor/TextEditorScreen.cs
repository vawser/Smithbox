using Hexa.NET.ImGui;
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

    public ActionManager EditorActionManager = new();

    public TextSelectionManager Selection;
    public TextContextMenu ContextMenu;
    public TextShortcuts EditorShortcuts;
    public TextActionHandler ActionHandler;
    public TextFilters Filters;
    public TextEntryGroupManager EntryGroupManager;
    public TextDifferenceManager DifferenceManager;
    public TextNamingTemplateManager NamingTemplateManager;

    public TextCommandQueue CommandQueue;

    public TextContainerWindow FileView;
    public TextFileWindow FmgView;
    public TextEntryWindow FmgEntryView;
    public TextContentsWindow FmgEntryPropertyEditor;
    public TextNewEntryCreationModal EntryCreationModal;
    public TextExporterModal TextExportModal;
    public TextDuplicatePopup TextDuplicatePopup;
    public TextToolView ToolView;

    public FmgExporter FmgExporter;
    public FmgImporter FmgImporter;
    public LanguageSync LanguageSync;

    public FmgDumper FmgDumper;

    public TextEditorScreen(ProjectEntry project)
    {
        Project = project;

        Selection = new TextSelectionManager(this, Project);
        ContextMenu = new TextContextMenu(this, Project);
        ActionHandler = new TextActionHandler(this, Project);
        EditorShortcuts = new TextShortcuts(this, Project);
        CommandQueue = new TextCommandQueue(this, Project);
        Filters = new TextFilters(this, Project);
        EntryGroupManager = new TextEntryGroupManager(this, Project);
        DifferenceManager = new TextDifferenceManager(this, Project);
        NamingTemplateManager = new TextNamingTemplateManager(this, Project);

        FileView = new TextContainerWindow(this, Project);
        FmgView = new TextFileWindow(this, Project);
        FmgEntryView = new TextEntryWindow(this, Project);
        FmgEntryPropertyEditor = new TextContentsWindow(this, Project);
        ToolView = new TextToolView(this, Project);

        EntryCreationModal = new TextNewEntryCreationModal(this, Project);
        TextExportModal = new TextExporterModal(this, Project);
        TextDuplicatePopup = new TextDuplicatePopup(this, Project);

        FmgExporter = new FmgExporter(this, Project);
        FmgImporter = new FmgImporter(this, Project);
        LanguageSync = new LanguageSync(this, Project);

        FmgDumper = new FmgDumper(this, Project);
    }

    public string EditorName => "Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.UIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_TextEntries");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_TextEditor_FileContainerList)
        {
            FileView.Display();
        }
        if (CFG.Current.Interface_TextEditor_FmgList)
        {
            FmgView.Display();
        }
        if (CFG.Current.Interface_TextEditor_TextEntryList)
        {
            FmgEntryView.Display();
        }
        if (CFG.Current.Interface_TextEditor_TextEntryContents)
        {
            FmgEntryPropertyEditor.Display();
        }
        EditorShortcuts.Monitor();

        if (CFG.Current.Interface_TextEditor_ToolWindow)
        {
            ToolView.Display();
        }

        CommandQueue.Parse(initcmd);
        EntryCreationModal.Display();
        TextDuplicatePopup.Display();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);

        FmgExporter.OnGui();
    }
    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(InputAction.Save)}"))
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
        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(InputAction.Undo)} / {InputManager.GetHint(InputAction.Undo_Repeat)}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(InputAction.Redo)} / {InputManager.GetHint(InputAction.Redo_Repeat)}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            // Create
            if (ImGui.MenuItem("Create", InputManager.GetHint(InputAction.TextEditor_Create_New_Entry)))
            {
                EntryCreationModal.ShowModal = true;
            }
            UIHelper.Tooltip($"Create new text entries.");

            // Duplicate
            if (ImGui.MenuItem("Duplicate", InputManager.GetHint(InputAction.Duplicate)))
            {
                ActionHandler.DuplicateEntries();
            }
            UIHelper.Tooltip($"Duplicate the currently selected text entries.");

            // Delete
            if (ImGui.MenuItem("Delete", InputManager.GetHint(InputAction.Delete)))
            {
                ActionHandler.DeleteEntries();
            }
            UIHelper.Tooltip($"Delete the currently selected text entries.");

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_TextEditor_FileContainerList = !CFG.Current.Interface_TextEditor_FileContainerList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_FileContainerList);

            if (ImGui.MenuItem("Text Files"))
            {
                CFG.Current.Interface_TextEditor_FmgList = !CFG.Current.Interface_TextEditor_FmgList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_FmgList);

            if (ImGui.MenuItem("Text Entries"))
            {
                CFG.Current.Interface_TextEditor_TextEntryList = !CFG.Current.Interface_TextEditor_TextEntryList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_TextEntryList);

            if (ImGui.MenuItem("Contents"))
            {
                CFG.Current.Interface_TextEditor_TextEntryContents = !CFG.Current.Interface_TextEditor_TextEntryContents;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_TextEntryContents);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_TextEditor_ToolWindow = !CFG.Current.Interface_TextEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_ToolWindow);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void ToolMenu()
    {
        if (ImGui.BeginMenu("Data"))
        {
            FmgImporter.MenubarOptions();

            ImGui.Separator();

            FmgExporter.MenubarOptions();

            ImGui.Separator();

            FmgDumper.MenubarOptions();

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Language Sync"))
        {
            LanguageSync.DisplayMenubarOptions();

            ImGui.EndMenu();
        }

        LanguageSync.OnGui();
    }

    /// <summary>
    /// Save currently selected FMG container
    /// </summary>
    public async void Save(bool autoSave = false)
    {
        var fileEntry = Selection.SelectedFileDictionaryEntry;
        var wrapper = Selection.SelectedContainerWrapper;

        if(fileEntry == null)
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
