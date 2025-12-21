using Hexa.NET.ImGui;
using StudioCore.Application;
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
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor", ImGuiWindowFlags.MenuBar))
        {
            Editor.Selection.SwitchWindowContext(TextEditorContext.ToolWindow);

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
                    GlobalTextSearch.Display(Editor);
                }
            }

            // Global Text Replacement
            if (CFG.Current.Interface_TextEditor_Tool_TextReplacement)
            {
                if (ImGui.CollapsingHeader("Text Replacement"))
                {
                    GlobalTextReplacement.Display(Editor);
                }
            }

            // Text Merge
            if (CFG.Current.Interface_TextEditor_Tool_TextMerge)
            {
                if (ImGui.CollapsingHeader("Text Merge"))
                {
                    TextMerge.Display(Editor);
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
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