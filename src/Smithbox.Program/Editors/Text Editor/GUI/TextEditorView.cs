using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextEditorView : IEditorView
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public int ViewIndex;

    public TextActionHandler ActionHandler;

    public TextViewSelection Selection;
    public TextEntryGroupManager EntryGroupManager;
    public TextDifferenceManager DifferenceManager;

    public TextContainerList ContainerList;
    public TextFileList FileList;
    public TextEntryList TextEntryList;
    public TextContents TextContents;
    public TextToolView ToolView;

    public TextEntryCreatorTool TextEntryCreator;
    public TextExporterModal TextExportModal;
    public TextDuplicatePopup TextDuplicatePopup;
    public TextDeleteConfirmPopup TextDeleteConfirmPopup;

    public FmgExporter FmgExporter;
    public FmgImporter FmgImporter;
    public FmgDumper FmgDumper;

    public TextFindMenu TextFindMenu;

    public TextEditorView(TextEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new TextViewSelection(this, Project);
        ActionHandler = new TextActionHandler(this, Project);
        EntryGroupManager = new TextEntryGroupManager(this, Project);
        DifferenceManager = new TextDifferenceManager(this, Project);

        ContainerList = new TextContainerList(this, Project);
        FileList = new TextFileList(this, Project);
        TextEntryList = new TextEntryList(this, Project);
        TextContents = new TextContents(this, Project);
        ToolView = new TextToolView(this, Project);

        TextEntryCreator = new TextEntryCreatorTool(this, Project);
        TextExportModal = new TextExporterModal(this, Project);
        TextDuplicatePopup = new TextDuplicatePopup(this, Project);
        TextDeleteConfirmPopup = new TextDeleteConfirmPopup(this, Project);

        FmgExporter = new FmgExporter(this, Project);
        FmgImporter = new FmgImporter(this, Project);

        FmgDumper = new FmgDumper(this, Project);

        TextFindMenu = new TextFindMenu(this, Project);
    }

    /// <summary>
    /// Undo the last action, then select what it changed.
    /// </summary>
    public void UndoAndReveal()
    {
        if (!ActionManager.CanUndo())
            return;

        var target = TextActionTargetUtils.GetFirstTarget(ActionManager.PeekUndoAction());
        var countBefore = ActionManager.UndoCount;

        ActionManager.UndoAction();

        // Only reveal if the undo actually ran (the execution guard can refuse it)
        if (ActionManager.UndoCount < countBefore)
            Selection.RevealTarget(target);
    }

    /// <summary>
    /// Redo the last undone action, then select what it changed.
    /// </summary>
    public void RedoAndReveal()
    {
        if (!ActionManager.CanRedo())
            return;

        var target = TextActionTargetUtils.GetFirstTarget(ActionManager.PeekRedoAction());
        var countBefore = ActionManager.UndoCount;

        ActionManager.RedoAction();

        if (ActionManager.UndoCount > countBefore)
            Selection.RevealTarget(target);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Each panel sets the focus context (which decides what Delete, Ctrl+D, Insert
        // and Alt+D act on) when it is the focused window, i.e. the one last clicked
        // or tabbed into. It used to follow the mouse hover, so shortcuts changed target
        // as the mouse crossed panels.
        // Container List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Container_List")}###textEditor_ContainerList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height);
        }

        ImGui.End();

        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_File_List")}###textEditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display(width, height);
        }

        ImGui.End();

        // Text Entry List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Text_Entries")}###textEditor_TextEntryList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_EntryList);
                Editor.ViewHandler.ActiveView = this;
            }

            TextEntryList.Display();
        }

        ImGui.End();

        // Text Contents
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Text_Content")}###textEditor_TextContents_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_EntryContents);
                Editor.ViewHandler.ActiveView = this;
            }

            TextContents.Display();
        }

        ImGui.End();

        if (CFG.Current.Interface_TextEditor_ToolWindow)
        {
            // Tools
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_TextEditorView);
            if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Tools")}###textEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.TextEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolView.Display();
            }

            ImGui.End();
        }

        TextEntryCreator.Display();
        TextDuplicatePopup.Display();
        TextDeleteConfirmPopup.Display();
        FmgImporter.OnGui();
        FmgExporter.OnGui();

        TextFindMenu.Shortcut();
        TextFindMenu.Display();
    }
}
