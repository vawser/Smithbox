using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.TextEditor.TextEditorScreen;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public enum TextEditorAction
    {
        None,
        SearchAndReplace,
        SyncEntries
    }

    public class TextEditorToolbar
    {
        public static ActionManager EditorActionManager;

        public static TextEditorAction SelectedAction;

        public static List<string> TargetTypes = new List<string>
        {
            "Selected Category",
            "Selected Entry"
        };

        public static List<string> TextCategories = new List<string>
        {
            "Title",
            "Description",
            "Summary",
            "Text Body",
            "Extra Text",
            "All"
        };

        public TextEditorToolbar(ActionManager actionManager)
        {
            EditorActionManager = actionManager;

            TextAction_SearchAndReplace.Setup();
            TextAction_SyncEntries.Setup();
        }

        public void OnGui()
        {
            if (Project.Type == ProjectType.Undefined)
                return;

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Toolbar##TextEditorToolbar"))
            {
                if (CFG.Current.Interface_TextEditor_Toolbar_HorizontalOrientation)
                {
                    ImGui.Columns(2);

                    ImGui.BeginChild("##TextEditorToolbar_Selection");

                    ShowActionList();

                    ImGui.EndChild();

                    ImGui.NextColumn();

                    ImGui.BeginChild("##TextEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
                else
                {
                    ShowActionList();

                    ImGui.BeginChild("##TextEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        public void ShowActionList()
        {
            ImGui.Separator();
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Actions");
            ImguiUtils.ShowHoverTooltip("Click to select a toolbar action.");
            ImGui.SameLine();

            if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
            {
                CFG.Current.Interface_TextEditor_Toolbar_HorizontalOrientation = !CFG.Current.Interface_TextEditor_Toolbar_HorizontalOrientation;
            }
            ImguiUtils.ShowHoverTooltip("Toggle the orientation of the toolbar.");
            ImGui.SameLine();

            if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
            {
                if (CFG.Current.Interface_TextEditor_PromptUser)
                {
                    CFG.Current.Interface_TextEditor_PromptUser = false;
                    PlatformUtils.Instance.MessageBox("Text Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
                }
                else
                {
                    CFG.Current.Interface_TextEditor_PromptUser = true;
                    PlatformUtils.Instance.MessageBox("Text Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
                }
            }
            ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
            ImGui.Separator();

            TextAction_SearchAndReplace.Select();
            TextAction_SyncEntries.Select();
        }

        public void ShowSelectedConfiguration()
        {
            ImGui.Indent(10.0f);
            ImGui.Separator();
            ImGui.Text("Configuration");
            ImGui.Separator();

            TextAction_SearchAndReplace.Configure();
            TextAction_SyncEntries.Configure();

            TextAction_SearchAndReplace.Act();
            TextAction_SyncEntries.Act();
        }
    }
}
