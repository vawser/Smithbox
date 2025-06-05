using HKLib.hk2018.hkAsyncThreadPool;
using HKLib.hk2018.hkHashMapDetail;
using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Core;
using StudioCore.Configuration;

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorScreen Editor;
    public TextSelectionManager Selection;
    private TextActionHandler ActionHandler;

    public TextToolView(TextEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor", ImGuiWindowFlags.MenuBar))
        {
            Selection.SwitchWindowContext(TextEditorContext.ToolWindow);

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