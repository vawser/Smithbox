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

    public TextToolView(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        TextSearch = new(view, project);
        TextReplace = new(view, project);
        TextMerge = new(view, project);

        LanguageSyncTool = new(view, project);
        DataTransferTool = new(view, project);
    }

    public void Display()
    {
        LanguageSyncTool.OnGui();

        if (!CFG.Current.Interface_TextEditor_ToolWindow)
            return;

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        // Entry Creator
        if (CFG.Current.Interface_TextEditor_Tool_EntryCreator)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Tools_Header_Text_Entry_Creator")}##textEntryCreatorSection"))
            {
                View.TextEntryCreator.DisplayTool();
            }
        }

        // Data Transfer
        if (CFG.Current.Interface_TextEditor_Tool_DataTransfer)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Tools_Header_Data_Transfer")}##dataTransferSection"))
            {
                DataTransferTool.Display();
            }
        }

        // Language Sync
        if (CFG.Current.Interface_TextEditor_Tool_LanguageSync)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Tools_Language_Sync")}##languageSyncSection"))
            {
                LanguageSyncTool.Display();
            }
        }

        // Global Text Search
        if (CFG.Current.Interface_TextEditor_Tool_TextSearch)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Tools_Text_Search")}##globalTextSearchSection"))
            {
                TextSearch.Display();
            }
        }

        // Global Text Replacement
        if (CFG.Current.Interface_TextEditor_Tool_TextReplacement)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Tools_Text_Replacement")}##globalTextReplacementSection"))
            {
                TextReplace.Display();
            }
        }

        // Text Merge
        if (CFG.Current.Interface_TextEditor_Tool_TextMerge)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Tools_Text_Merge")}##textMergeSection"))
            {
                TextMerge.Display();
            }
        }
    }
    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Tools_Menu_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXT_Tools_View_Text_Entry_Creator")}##textEntryCreatorViewToggle"))
            {
                CFG.Current.Interface_TextEditor_Tool_EntryCreator = !CFG.Current.Interface_TextEditor_Tool_EntryCreator;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_EntryCreator);

            if (ImGui.MenuItem($"{LOC.Get("TEXT_Tools_View_Data_Transfer")}##dataTransferViewToggle"))
            {
                CFG.Current.Interface_TextEditor_Tool_DataTransfer = !CFG.Current.Interface_TextEditor_Tool_DataTransfer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_DataTransfer);

            if (ImGui.MenuItem($"{LOC.Get("TEXT_Tools_View_Language_Sync")}##languageSyncViewToggle"))
            {
                CFG.Current.Interface_TextEditor_Tool_LanguageSync = !CFG.Current.Interface_TextEditor_Tool_LanguageSync;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_LanguageSync);

            if (ImGui.MenuItem($"{LOC.Get("TEXT_Tools_View_Text_Search")}##textSearchViewToggle"))
            {
                CFG.Current.Interface_TextEditor_Tool_TextSearch = !CFG.Current.Interface_TextEditor_Tool_TextSearch;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_TextSearch);

            if (ImGui.MenuItem($"{LOC.Get("TEXT_Tools_View_Text_Replacement")}##textReplacementViewToggle"))
            {
                CFG.Current.Interface_TextEditor_Tool_TextReplacement = !CFG.Current.Interface_TextEditor_Tool_TextReplacement;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_TextReplacement);

            if (ImGui.MenuItem($"{LOC.Get("TEXT_Tools_View_Text_Merge")}##textMergeViewToggle"))
            {
                CFG.Current.Interface_TextEditor_Tool_TextMerge = !CFG.Current.Interface_TextEditor_Tool_TextMerge;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_TextMerge);

            ImGui.EndMenu();
        }
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