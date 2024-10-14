using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Numerics;
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
    public TextActionMenubar ActionMenubar;

    public TextFileView FileView;
    public TextFmgView FmgView;
    public TextFmgEntryView FmgEntryView;
    public TextFmgEntryPropertyEditor FmgEntryPropertyEditor;
    public TextNewEntryCreationModal EntryCreationModal;

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
        ActionMenubar = new TextActionMenubar(this);

        FileView = new TextFileView(this);
        FmgView = new TextFmgView(this);
        FmgEntryView = new TextFmgEntryView(this);
        FmgEntryPropertyEditor = new TextFmgEntryPropertyEditor(this);

        EntryCreationModal = new TextNewEntryCreationModal(this);
    }

    public string EditorName => "Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}", false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionMenubar.Display();

        ImGui.Separator();

        ToolMenubar.Display();

        if (ImGui.BeginMenu("View"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Files"))
            {
                UI.Current.Interface_EmevdEditor_Files = !UI.Current.Interface_EmevdEditor_Files;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Files);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
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
    }

    /// <summary>
    /// Reset editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            Selection.OnProjectChanged();
            Decorator.OnProjectChanged();

            ActionHandler.OnProjectChanged();
            NamingTemplateManager.OnProjectChanged();
        }

        TextBank.LoadTextFiles();

        ResetActionManager();
    }

    /// <summary>
    /// Save currently selected FMG container
    /// </summary>
    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        TextBank.SaveFmgContainer(Selection.SelectedContainer);
    }

    /// <summary>
    /// Save all modified FMG containers
    /// </summary>
    public void SaveAll()
    {
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
}
