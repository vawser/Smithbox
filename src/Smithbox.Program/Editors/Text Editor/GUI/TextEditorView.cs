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

public class TextEditorView
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public int ViewIndex;

    public TextActionHandler ActionHandler;
    public TextContextMenu ContextMenu;

    public TextViewSelection Selection;
    public TextEntryGroupManager EntryGroupManager;
    public TextDifferenceManager DifferenceManager;

    public TextContainerList ContainerList;
    public TextFileList FileList;
    public TextEntryList TextEntryList;
    public TextContents TextContents;

    public TextEntryCreatorTool TextEntryCreator;
    public TextExporterModal TextExportModal;
    public TextDuplicatePopup TextDuplicatePopup;

    public FmgExporter FmgExporter;
    public FmgImporter FmgImporter;
    public FmgDumper FmgDumper;

    public TextEditorView(TextEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new TextViewSelection(this, Project);
        ContextMenu = new TextContextMenu(this, Project);
        ActionHandler = new TextActionHandler(this, Project);
        EntryGroupManager = new TextEntryGroupManager(this, Project);
        DifferenceManager = new TextDifferenceManager(this, Project);

        ContainerList = new TextContainerList(this, Project);
        FileList = new TextFileList(this, Project);
        TextEntryList = new TextEntryList(this, Project);
        TextContents = new TextContents(this, Project);

        TextEntryCreator = new TextEntryCreatorTool(this, Project);
        TextExportModal = new TextExporterModal(this, Project);
        TextDuplicatePopup = new TextDuplicatePopup(this, Project);

        FmgExporter = new FmgExporter(this, Project);
        FmgImporter = new FmgImporter(this, Project);

        FmgDumper = new FmgDumper(this, Project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Container List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Container_List")}###textEditor_ContainerList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height);
        }

        ImGui.End();

        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_File_List")}###textEditor_FileList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display(width, height);
        }

        ImGui.End();

        // Text Entry List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Text_Entries")}###textEditor_TextEntryList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_EntryList);
                Editor.ViewHandler.ActiveView = this;
            }

            TextEntryList.Display();
        }

        ImGui.End();

        // Text Contents
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextEditorView);
        if (ImGui.Begin($@"{LOC.Get("TEXT_Window_Text_Content")}###textEditor_TextContents_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_EntryContents);
                Editor.ViewHandler.ActiveView = this;
            }

            TextContents.Display();
        }

        ImGui.End();

        TextEntryCreator.Display();
        TextDuplicatePopup.Display();
        FmgImporter.OnGui();
        FmgExporter.OnGui();
    }
}
