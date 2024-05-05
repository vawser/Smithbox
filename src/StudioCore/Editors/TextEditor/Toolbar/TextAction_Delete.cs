using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public class TextAction_Delete
    {
        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.RadioButton("Delete##tool_Delete", TextToolbar.SelectedAction == TextEditorAction.Delete))
            {
                TextToolbar.SelectedAction = TextEditorAction.Delete;
            }
            ImguiUtils.ShowHoverTooltip("Delete selected entry.");
        }

        public static void Configure()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.Delete)
            {
                ImguiUtils.WrappedText("Delete the current selection.");
                ImguiUtils.WrappedText("");

                /*
                if(ImGui.Checkbox("Standard", ref CFG.Current.FMG_StandardDelete))
                {
                    if (CFG.Current.FMG_BlockDelete)
                    {
                        CFG.Current.FMG_BlockDelete = false;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Delete the selected entry.");

                if (ImGui.Checkbox("Block", ref CFG.Current.FMG_BlockDelete))
                {
                    if (CFG.Current.FMG_StandardDelete)
                    {
                        CFG.Current.FMG_StandardDelete = false;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Delete the entries within the specified start and end text ID.");
                ImGui.Text("");
                */

                if (CFG.Current.FMG_BlockDelete)
                {
                    ImguiUtils.WrappedText("Start ID:");
                    ImGui.InputInt("##startId", ref CFG.Current.FMG_BlockDelete_StartID);
                    ImguiUtils.ShowHoverTooltip("Text ID at which to start deletion.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("End ID:");
                    ImGui.InputInt("##endId", ref CFG.Current.FMG_BlockDelete_EndID);
                    ImguiUtils.ShowHoverTooltip("Text ID at which to end deletion.");
                }
            }
        }

        public static void Act()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.Delete)
            {
                if (ImGui.Button("Apply##action_Selection_Delete", new Vector2(200, 32)))
                {
                    if (CFG.Current.FMG_StandardDelete)
                    {
                        DeleteSelectedEntry();
                    }

                    if(CFG.Current.FMG_BlockDelete)
                    {
                        DeleteEntryBlock();
                    }
                }
            }
        }

        public static void DeleteSelectedEntry()
        {
            var entry = TextEditorScreen._activeEntryGroup;

            var action = new DeleteFMGEntryAction(entry);
            TextEditorScreen.EditorActionManager.ExecuteAction(action);
            TextEditorScreen._activeEntryGroup = null;
            TextEditorScreen._activeIDCache = -1;

            // Lazy method to refresh search filter
            TextEditorScreen._searchFilterCached = "";
        }

        private static void DeleteEntryBlock()
        {

        }
    }
}
