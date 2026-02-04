using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public TextToolView(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (!CFG.Current.Interface_TextEditor_ToolWindow)
            return;

        var activeView = Editor.ViewHandler.ActiveView;

        if (ImGui.Begin("Tools##ToolConfigureWindow_TextEditor", ImGuiWindowFlags.MenuBar))
        {
            FocusManager.SetFocus(EditorFocusContext.TextEditor_Tools);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            // Global Text Search
            if (CFG.Current.Interface_TextEditor_Tool_TextSearch)
            {
                if (ImGui.CollapsingHeader("Text Search"))
                {
                    GlobalTextSearch.Display(activeView);
                }
            }

            // Global Text Replacement
            if (CFG.Current.Interface_TextEditor_Tool_TextReplacement)
            {
                if (ImGui.CollapsingHeader("Text Replacement"))
                {
                    GlobalTextReplacement.Display(activeView);
                }
            }

            // Text Merge
            if (CFG.Current.Interface_TextEditor_Tool_TextMerge)
            {
                if (ImGui.CollapsingHeader("Text Merge"))
                {
                    TextMerge.Display(activeView);
                }
            }
        }

        ImGui.End();
    }
    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
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