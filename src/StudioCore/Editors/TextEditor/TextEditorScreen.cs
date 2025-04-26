using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;

using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Timers;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public ActionManager EditorActionManager = new();

    public TextSelectionManager Selection;
    public TextPropertyDecorator Decorator;
    public TextContextMenu ContextMenu;
    public TextShortcuts EditorShortcuts;
    public TextActionHandler ActionHandler;
    public TextFilters Filters;
    public TextEntryGroupManager EntryGroupManager;
    public TextDifferenceManager DifferenceManager;
    public TextNamingTemplateManager NamingTemplateManager;

    public TextCommandQueue CommandQueue;

    public TextToolView ToolView;
    public TextToolMenubar ToolMenubar;

    public TextFileView FileView;
    public TextFmgView FmgView;
    public TextFmgEntryView FmgEntryView;
    public TextFmgEntryPropertyEditor FmgEntryPropertyEditor;
    public TextNewEntryCreationModal EntryCreationModal;
    public TextExporterModal TextExportModal;

    public TextEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Selection = new TextSelectionManager(this);
        Decorator = new TextPropertyDecorator(this);
        ContextMenu = new TextContextMenu(this);
        ActionHandler = new TextActionHandler(this);
        EditorShortcuts = new TextShortcuts(this);
        CommandQueue = new TextCommandQueue(this);
        Filters = new TextFilters(this);
        EntryGroupManager = new TextEntryGroupManager(this);
        DifferenceManager = new TextDifferenceManager(this);
        NamingTemplateManager = new TextNamingTemplateManager(this);

        ToolView = new TextToolView(this);
        ToolMenubar = new TextToolMenubar(this);

        FileView = new TextFileView(this);
        FmgView = new TextFmgView(this);
        FmgEntryView = new TextFmgEntryView(this);
        FmgEntryPropertyEditor = new TextFmgEntryPropertyEditor(this);

        EntryCreationModal = new TextNewEntryCreationModal(this);
        TextExportModal = new TextExporterModal(this);
    }

    public string EditorName => "Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";

    public void EditDropdown()
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
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
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            // Create
            if (ImGui.MenuItem("Create", KeyBindings.Current.CORE_CreateNewEntry.HintText))
            {
                EntryCreationModal.ShowModal = true;
            }
            UIHelper.Tooltip($"Create new text entries.");

            // Duplicate
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.DuplicateEntries();
            }
            UIHelper.Tooltip($"Duplicate the currently selected text entries.");

            // Delete
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.DeleteEntries();
            }
            UIHelper.Tooltip($"Delete the currently selected text entries.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Files"))
            {
                UI.Current.Interface_TextEditor_FileContainerList = !UI.Current.Interface_TextEditor_FileContainerList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextEditor_FileContainerList);

            if (ImGui.MenuItem("Text Files"))
            {
                UI.Current.Interface_TextEditor_FmgList = !UI.Current.Interface_TextEditor_FmgList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextEditor_FmgList);

            if (ImGui.MenuItem("Text Entries"))
            {
                UI.Current.Interface_TextEditor_FmgEntryList = !UI.Current.Interface_TextEditor_FmgEntryList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextEditor_FmgEntryList);

            if (ImGui.MenuItem("Contents"))
            {
                UI.Current.Interface_TextEditor_FmgEntryProperties = !UI.Current.Interface_TextEditor_FmgEntryProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextEditor_FmgEntryProperties);

            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_TextEditor_ToolConfigurationWindow = !UI.Current.Interface_TextEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextEditor_ToolConfigurationWindow);

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        if (ImGui.BeginMenu("Data"))
        {
            FmgImporter.MenubarOptions();

            ImGui.Separator();

            FmgExporter.MenubarOptions();

            ImGui.EndMenu();
        }

        // ImGui.Separator();

        // Tools
        // ToolMenubar.Display();
    }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        var scale = DPI.GetUIScale();

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

        if (!TextUtils.IsSupportedProjectType() || !TextBank.PrimaryBankLoaded)
        {
            ImGui.Begin("Editor##InvalidTextEditor");

            if (!TextUtils.IsSupportedProjectType())
            {
                ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");
            }
            else
            {
                ImGui.Text($"This editor is still loading...");
            }

            ImGui.End();
        }
        else
        {
            if (TextBank.PrimaryBankLoaded)
            {
                if(!TextBank.VanillaBankLoaded && !TextBank.VanillaBankLoading)
                {
                    TextBank.LoadVanillaTextFiles();
                }

                if (UI.Current.Interface_TextEditor_FileContainerList)
                {
                    FileView.Display();
                }
                if (UI.Current.Interface_TextEditor_FmgList)
                {
                    FmgView.Display();
                }
                if (UI.Current.Interface_TextEditor_FmgEntryList)
                {
                    FmgEntryView.Display();
                }
                if (UI.Current.Interface_TextEditor_FmgEntryProperties)
                {
                    FmgEntryPropertyEditor.Display();
                }
            }

            EditorShortcuts.Monitor();

            if (UI.Current.Interface_TextEditor_ToolConfigurationWindow)
            {
                ToolView.Display();
            }

            CommandQueue.Parse(initcmd);
            EntryCreationModal.Display();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);

        FmgExporter.OnGui();
    }

    /// <summary>
    /// Reset editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        SetupDifferenceTimer();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            Selection.OnProjectChanged();
            Decorator.OnProjectChanged();

            ActionHandler.OnProjectChanged();
            NamingTemplateManager.OnProjectChanged();

            FmgImporter.OnProjectChanged();
        }

        TextBank.LoadTextFiles();

        ResetActionManager();
    }

    /// <summary>
    /// Save currently selected FMG container
    /// </summary>
    public void Save()
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            TextBank.SaveLooseFmgs(Selection.SelectedContainerWrapper);
        }
        else
        {
            if (TextBank.PrimaryBankLoaded)
            {
                TextBank.SaveFmgContainer(Selection.SelectedContainerWrapper);
            }
        }
    }

    /// <summary>
    /// Save all modified FMG containers
    /// </summary>
    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_FMG)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        TextBank.SaveTextFiles();
    }

    /// <summary>
    /// Reset the undo/redo stack
    /// </summary>
    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    public Timer AutomaticDiffTimer;

    private void SetupDifferenceTimer()
    {
        if (AutomaticDiffTimer != null)
        {
            AutomaticDiffTimer.Close();
        }

        var interval = 3000;

        AutomaticDiffTimer = new Timer(interval);
        AutomaticDiffTimer.Elapsed += OnAutomaticDiff;
        AutomaticDiffTimer.AutoReset = true;
        AutomaticDiffTimer.Enabled = true;
    }

    public void OnAutomaticDiff(object source, ElapsedEventArgs e)
    {
        if (CFG.Current.TextEditor_EnableAutomaticDifferenceCheck)
        {
            if (TextBank.VanillaBankLoaded)
            {
                DifferenceManager.TrackFmgDifferences();
            }
        }
    }
}
