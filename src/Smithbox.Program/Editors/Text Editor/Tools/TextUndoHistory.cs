using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Undo History: the view's undo steps (newest first) and redo steps, each with what it
/// changed. Clicking a step undoes or redoes everything up to and including it.
/// Undo covers every FMG in the view, so this shows what Ctrl+Z will reach next.
/// </summary>
public class TextUndoHistory
{
    private TextEditorView View;
    private ProjectEntry Project;

    public TextUndoHistory(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        // Bordered section, like the other tools
        ImGui.BeginChild("UndoHistorySection", ImGuiChildFlags.Borders);

        DisplayContents();

        ImGui.EndChild();
    }

    private void DisplayContents()
    {
        var manager = View.ActionManager;

        // Stacks enumerate from the top: the undo steps newest first, then the redo steps next-first
        var history = manager.GetHistory().ToList();
        var undoCount = manager.UndoCount;
        var redoCount = history.Count - undoCount;

        if (history.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_History_Empty"));
            return;
        }

        GUI.WrappedText(LOC.Get("TEXT_History_Hint"));

        var height = System.Math.Min(history.Count + 2, 16) * ImGui.GetTextLineHeightWithSpacing() + 8f * DPI.UIScale();

        ImGui.BeginChild("##textUndoHistory", new Vector2(0, height), ImGuiChildFlags.Borders);

        // Redo steps, furthest first, so the list reads oldest at the bottom to newest at the top
        if (redoCount > 0)
        {
            ImGui.TextDisabled(LOC.Get("TEXT_History_Redo_Header", redoCount));

            for (int j = redoCount - 1; j >= 0; j--)
            {
                var action = history[undoCount + j];

                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                var clicked = ImGui.Selectable($"{Describe(action)}##historyRedo{j}");
                ImGui.PopStyleColor();

                GUI.Tooltip(LOC.Get("TEXT_History_Redo_TT", j + 1));

                if (clicked)
                {
                    Step(j + 1, redo: true);
                }
            }

            ImGui.Separator();
        }

        if (undoCount > 0)
        {
            ImGui.TextDisabled(LOC.Get("TEXT_History_Undo_Header", undoCount));

            for (int i = 0; i < undoCount; i++)
            {
                var action = history[i];

                if (ImGui.Selectable($"{Describe(action)}##historyUndo{i}", i == 0))
                {
                    Step(i + 1, redo: false);
                }

                GUI.Tooltip(LOC.Get("TEXT_History_Undo_TT", i + 1));
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Undo or redo this many steps, stopping early if one is refused
    /// </summary>
    private void Step(int count, bool redo)
    {
        for (int n = 0; n < count; n++)
        {
            var before = View.ActionManager.UndoCount;

            if (redo)
                View.RedoAndReveal();
            else
                View.UndoAndReveal();

            if (View.ActionManager.UndoCount == before)
                return;
        }
    }

    /// <summary>
    /// "Edit text — item_dlc02 › WeaponName › 1000000", or for a group of changes
    /// "Delete entry ×12 — ...", with the location of the first change
    /// </summary>
    public static string Describe(EditorAction action)
    {
        var kind = KindOf(action);

        if (action is FmgGroupedAction group)
        {
            var children = group.GetActions().Where(a => a != null).ToList();
            var kinds = children.Select(KindOf).Distinct().ToList();

            kind = kinds.Count == 1
                ? LOC.Get("TEXT_History_Group_Same", kinds[0], children.Count)
                : LOC.Get("TEXT_History_Group_Mixed", children.Count);
        }

        var target = TextActionTargetUtils.GetFirstTarget(action);

        if (target?.TargetContainer == null)
            return kind;

        var location = $"{target.TargetContainer.FileEntry.Filename} › {target.TargetFmg?.Name}";

        if (target.TargetEntry != null)
            location = $"{location} › {target.TargetEntry.ID}";

        return LOC.Get("TEXT_History_Item", kind, location);
    }

    private static string KindOf(EditorAction action) => action switch
    {
        ChangeFmgEntryText => LOC.Get("TEXT_History_Kind_Edit_Text"),
        ChangeFmgEntryID => LOC.Get("TEXT_History_Kind_Change_ID"),
        AddFmgEntry or AddAssociatedEntry => LOC.Get("TEXT_History_Kind_Add"),
        DuplicateFmgEntry => LOC.Get("TEXT_History_Kind_Duplicate"),
        DeleteFmgEntry => LOC.Get("TEXT_History_Kind_Delete"),
        SortFmgList => LOC.Get("TEXT_History_Kind_Sort"),
        FmgGroupedAction => LOC.Get("TEXT_History_Kind_Group"),
        _ => LOC.Get("TEXT_History_Kind_Other")
    };
}
