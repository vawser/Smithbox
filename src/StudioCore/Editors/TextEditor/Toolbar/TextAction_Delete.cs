using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.FFXDLSE;

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
            }
        }

        public static void Act()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.Delete)
            {
                if (ImGui.Button("Apply##action_Selection_Delete", new Vector2(200, 32)))
                {
                    DeleteSelectedEntries();
                }
            }
        }

        public static void DeleteSelectedEntries()
        {
            var entryIds = Smithbox.EditorHandler.TextEditor.SelectionHandler.EntryIds;
            var entries = Smithbox.EditorHandler.TextEditor._EntryLabelCacheFiltered;
            var fmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;

            List<EditorAction> actions = new List<EditorAction>();

            for (var i = 0; i < entries.Count; i++)
            {
                FMG.Entry r = entries[i];
                if (entryIds.Contains(r.ID))
                {
                    var entry = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, fmgInfo);
                    actions.Add(new DeleteFMGEntryAction(entry));
                }
            }

            var compoundAction = new CompoundAction(actions);
            Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(compoundAction);

            Smithbox.EditorHandler.TextEditor._activeEntryGroup = null;
            Smithbox.EditorHandler.TextEditor._activeIDCache = -1;
            Smithbox.EditorHandler.TextEditor._searchFilterCached = "";
        }
    }
}
