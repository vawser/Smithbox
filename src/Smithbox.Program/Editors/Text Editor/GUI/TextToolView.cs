using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public GlobalTextSearch TextSearch;
    public GlobalTextReplacement TextReplace;
    public TextMerge TextMerge;

    public TextLanguageSyncTool LanguageSyncTool;
    public TextDataTransferTool DataTransferTool;

    public TextToolView(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        TextSearch = new(editor, project);
        TextReplace = new(editor, project);
        TextMerge = new(editor, project);

        LanguageSyncTool = new(editor, project);
        DataTransferTool = new(editor, project);
    }

    public void Display()
    {
        if (!CFG.Current.Interface_TextEditor_ToolWindow)
            return;

        var activeView = Editor.ViewHandler.ActiveView;

        if (ImGui.Begin("Tools##ToolConfigureWindow_TextEditor", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.TextEditor_Tools);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            // Data Transfer
            if (CFG.Current.Interface_TextEditor_Tool_DataTransfer)
            {
                if (ImGui.CollapsingHeader("Data Transfer"))
                {
                    DataTransferTool.Display();
                }
            }

            // Data Transfer
            if (CFG.Current.Interface_TextEditor_Tool_LanguageSync)
            {
                if (ImGui.CollapsingHeader("Language Sync"))
                {
                    LanguageSyncTool.Display();
                }
            }

            // Global Text Search
            if (CFG.Current.Interface_TextEditor_Tool_TextSearch)
            {
                if (ImGui.CollapsingHeader("Text Search"))
                {
                    TextSearch.Display();
                }
            }

            // Global Text Replacement
            if (CFG.Current.Interface_TextEditor_Tool_TextReplacement)
            {
                if (ImGui.CollapsingHeader("Text Replacement"))
                {
                    TextReplace.Display();
                }
            }

            // Text Merge
            if (CFG.Current.Interface_TextEditor_Tool_TextMerge)
            {
                if (ImGui.CollapsingHeader("Text Merge"))
                {
                    TextMerge.Display();
                }
            }
        }

        ImGui.End();
    }
    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Data Transfer"))
            {
                CFG.Current.Interface_TextEditor_Tool_DataTransfer = !CFG.Current.Interface_TextEditor_Tool_DataTransfer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_DataTransfer);

            if (ImGui.MenuItem("Language Sync"))
            {
                CFG.Current.Interface_TextEditor_Tool_LanguageSync = !CFG.Current.Interface_TextEditor_Tool_LanguageSync;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_LanguageSync);

            if (ImGui.MenuItem("Text Search"))
            {
                CFG.Current.Interface_TextEditor_Tool_TextSearch = !CFG.Current.Interface_TextEditor_Tool_TextSearch;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_TextSearch);

            if (ImGui.MenuItem("Text Replacement"))
            {
                CFG.Current.Interface_TextEditor_Tool_TextReplacement = !CFG.Current.Interface_TextEditor_Tool_TextReplacement;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_TextReplacement);

            if (ImGui.MenuItem("Text Merge"))
            {
                CFG.Current.Interface_TextEditor_Tool_TextMerge = !CFG.Current.Interface_TextEditor_Tool_TextMerge;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextEditor_Tool_TextMerge);

            ImGui.EndMenu();
        }
    }
}