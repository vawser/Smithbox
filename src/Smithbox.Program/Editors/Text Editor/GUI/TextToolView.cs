using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorView View;
    private ProjectEntry Project;

    public GlobalTextSearch TextSearch;
    public GlobalTextReplacement TextReplace;
    public TextMerge TextMerge;

    public TextLanguageSyncTool LanguageSyncTool;
    public TextDataTransferTool DataTransferTool;

    public FmgReloaderTool FmgReloader;

    private ToolSections Sections = new();

    public TextToolView(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        TextSearch = new(view, project);
        TextReplace = new(view, project);
        TextMerge = new(view, project);

        LanguageSyncTool = new(view, project);
        DataTransferTool = new(view, project);

        FmgReloader = new(view, project);
    }

    public void Display()
    {
        LanguageSyncTool.OnGui();

        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Global Text Search
        if (ToolSections.Header($"{LOC.Get("TEXT_Tools_Text_Search")}##globalTextSearchSection"))
        {
            TextSearch.Display();
        }

        // Global Text Replacement
        if (ToolSections.Header($"{LOC.Get("TEXT_Tools_Text_Replacement")}##globalTextReplacementSection"))
        {
            TextReplace.Display();
        }

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        // Entry Creator
        if (ToolSections.Header($"{LOC.Get("TEXT_Tools_Header_Text_Entry_Creator")}##textEntryCreatorSection"))
        {
            View.TextEntryCreator.DisplayTool();
        }

        // Language Sync
        if (ToolSections.Header($"{LOC.Get("TEXT_Tools_Language_Sync")}##languageSyncSection"))
        {
            LanguageSyncTool.Display();
        }

        // Data
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        // Data Transfer
        if (ToolSections.Header($"{LOC.Get("TEXT_Tools_Header_Data_Transfer")}##dataTransferSection"))
        {
            DataTransferTool.Display();
        }

        // Text Merge
        if (ToolSections.Header($"{LOC.Get("TEXT_Tools_Text_Merge")}##textMergeSection"))
        {
            TextMerge.Display();
        }

        // Game Memory
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        // FMG Reloader
        FmgReloader.OnToolWindow();

        Sections.End();
    }

    public void DisplayDropdown()
    {
        // Tools
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Tools")}##toolsMenuHeader"))
        {
            DataTransferTool.DisplayDropdown();
            View.FmgDumper.DumperDropdownOptions();

            ImGui.EndMenu();
        }
    }
}
