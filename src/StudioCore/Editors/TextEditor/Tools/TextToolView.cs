using HKLib.hk2018.hkAsyncThreadPool;
using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using Octokit;
using StudioCore.Core.Project;
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

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorScreen Screen;
    public TextSelectionManager Selection;
    private TextActionHandler ActionHandler;

    public TextToolView(TextEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor"))
        {
            Selection.SwitchWindowContext(TextEditorContext.ToolWindow);

            // Global Text Search
            if (ImGui.CollapsingHeader("Text Search"))
            {
                GlobalTextSearch.Display();
            }

            // Global Text Replacement
            if (ImGui.CollapsingHeader("Text Replacement"))
            {
                GlobalTextReplacement.Display();
            }

            // Text Merge
            if (ImGui.CollapsingHeader("Text Merge"))
            {
                TextMerge.Display();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}