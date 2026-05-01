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

    public TextFilters Filters;
    public TextActionHandler ActionHandler;
    public TextContextMenu ContextMenu;

    public TextViewSelection Selection;
    public TextEntryGroupManager EntryGroupManager;
    public TextDifferenceManager DifferenceManager;
    public TextNamingTemplateManager NamingTemplateManager;

    public TextContainerList ContainerList;
    public TextFileList FileList;
    public TextEntryList TextEntryList;
    public TextContents TextContents;

    public TextNewEntryModal NewEntryModal;
    public TextExporterModal TextExportModal;
    public TextDuplicatePopup TextDuplicatePopup;

    public FmgExporter FmgExporter;
    public FmgImporter FmgImporter;
    public LanguageSync LanguageSync;
    public FmgDumper FmgDumper;

    public TextEditorView(TextEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new TextViewSelection(this, Project);
        ContextMenu = new TextContextMenu(this, Project);
        ActionHandler = new TextActionHandler(this, Project);
        Filters = new TextFilters(this, Project);
        EntryGroupManager = new TextEntryGroupManager(this, Project);
        DifferenceManager = new TextDifferenceManager(this, Project);
        NamingTemplateManager = new TextNamingTemplateManager(this, Project);

        ContainerList = new TextContainerList(this, Project);
        FileList = new TextFileList(this, Project);
        TextEntryList = new TextEntryList(this, Project);
        TextContents = new TextContents(this, Project);

        NewEntryModal = new TextNewEntryModal(this, Project);
        TextExportModal = new TextExporterModal(this, Project);
        TextDuplicatePopup = new TextDuplicatePopup(this, Project);

        FmgExporter = new FmgExporter(this, Project);
        FmgImporter = new FmgImporter(this, Project);
        LanguageSync = new LanguageSync(this, Project);

        FmgDumper = new FmgDumper(this, Project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        DisplayMenubar();

        var columnCount = 3;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("textTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##FileList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##EntryList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##EntryContents", ImGuiTableColumnFlags.WidthStretch, 0.5f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

            ImGui.BeginChild("##FileListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height * CFG.Current.TextEditor_Display_ContainerList_Percentage);
            FileList.Display(width, height * CFG.Current.TextEditor_Display_FileList_Percentage);

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##EntryListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_EntryList);
                Editor.ViewHandler.ActiveView = this;
            }

            TextEntryList.Display();

            ImGui.EndChild();

            // --- Column 3 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##EntryContentsArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextEditor_EntryContents);
                Editor.ViewHandler.ActiveView = this;
            }

            TextContents.Display();

            ImGui.EndChild();

            ImGui.EndTable();
        }

        NewEntryModal.Display();
        TextDuplicatePopup.Display();
        FmgExporter.OnGui();
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.BeginMenu("Display"))
                {
                    ImGui.SliderFloat("Containers##containerListDisplayPercentage", ref CFG.Current.TextEditor_Display_ContainerList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        // Auto-adjust the other var so the ratio remains 100%
                        CFG.Current.TextEditor_Display_FileList_Percentage = 1 - CFG.Current.TextEditor_Display_ContainerList_Percentage;
                    }
                    UIHelper.Tooltip("The percentage of the window the Containers section occupies.");

                    ImGui.SliderFloat("Files##fileListDisplayPercentage", ref CFG.Current.TextEditor_Display_FileList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        // Auto-adjust the other var so the ratio remains 100%
                        CFG.Current.TextEditor_Display_ContainerList_Percentage = 1 - CFG.Current.TextEditor_Display_FileList_Percentage;
                    }
                    UIHelper.Tooltip("The percentage of the window the Files section occupies.");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }
}
