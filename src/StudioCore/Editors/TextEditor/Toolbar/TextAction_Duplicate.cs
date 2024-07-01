using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StudioCore.TextEditor.FMGBank;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public class TextAction_Duplicate
    {
        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.RadioButton("Duplicate##tool_Duplicate", TextToolbar.SelectedAction == TextEditorAction.Duplicate))
            {
                TextToolbar.SelectedAction = TextEditorAction.Duplicate;
            }
            ImguiUtils.ShowHoverTooltip("Duplicate selected entry.");
        }

        public static void Configure()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.Duplicate)
            {
                ImguiUtils.WrappedText("Duplicate the current selection.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Amount:");
                ImGui.InputInt("##dupeamount", ref CFG.Current.FMG_DuplicateAmount);
                ImguiUtils.ShowHoverTooltip("The number of times to duplicate this entry.");
                ImguiUtils.WrappedText("");

                if (CFG.Current.FMG_DuplicateAmount < 1)
                    CFG.Current.FMG_DuplicateAmount = 1;

                ImguiUtils.WrappedText("Increment:");
                ImGui.InputInt("##dupeIncrement", ref CFG.Current.FMG_DuplicateIncrement);
                ImguiUtils.ShowHoverTooltip("The increment to apply to the text id when duplicating.");
                ImguiUtils.WrappedText("");

                if (CFG.Current.FMG_DuplicateIncrement < 1)
                    CFG.Current.FMG_DuplicateIncrement = 1;
            }
        }

        public static void Act()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.Duplicate)
            {
                if (ImGui.Button("Apply##action_Selection_Duplicate", new Vector2(200, 32)))
                {
                    DuplicateSelectedEntry();
                }
            }
        }

        public static void DuplicateSelectedEntry()
        {
            var entry = Smithbox.EditorHandler.TextEditor._activeEntryGroup;

            for (int i = 0; i < CFG.Current.FMG_DuplicateAmount; i++)
            {
                TextEditorScreen._activeIDCache = entry.GetNextUnusedID(CFG.Current.FMG_DuplicateIncrement);
                var action = new DuplicateFMGEntryAction(entry);
                TextEditorScreen.EditorActionManager.ExecuteAction(action);
            }

            // Lazy method to refresh search filter
            // TODO: _searchFilterCached should be cleared whenever CacheBank is cleared.
            TextEditorScreen._searchFilterCached = "";
        }
    }
}
