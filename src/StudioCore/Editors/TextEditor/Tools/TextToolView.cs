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

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor"))
        {
            Selection.SwitchWindowContext(TextEditorContext.ToolWindow);

            // Global Text Search
            if (ImGui.CollapsingHeader("Text Search"))
            {
                GlobalTextSearch.Display(Editor);
            }

            // Global Text Replacement
            if (ImGui.CollapsingHeader("Text Replacement"))
            {
                GlobalTextReplacement.Display(Editor);
            }

            // Text Merge
            if (ImGui.CollapsingHeader("Text Merge"))
            {
                TextMerge.Display(Editor);
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}