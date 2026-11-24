using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Bookmarks (Behavior Editor Tools window): the bookmarked states, generators, clips and modifiers of every behavior
/// file, the shown file's first. Clicking one opens its file and shows it. See Tool UI Styling.md.
/// </summary>
public class HavokBehaviorBookmarksTool
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private HavokBehaviorView Behavior => View.PropertyView.BehaviorView;

    public HavokBehaviorBookmarksTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("HAVOK_Bookmarks_Header")}##havokBehaviorBookmarksToolHeader"))
            return;

        ImGui.BeginChild("HavokBehaviorBookmarksToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("HAVOK_Bookmarks_Hint"));

        var bookmarks = Behavior.Bookmarks.Entries;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Bookmarks_List_Header", bookmarks.Count), LOC.Get("HAVOK_Bookmarks_List_Header_TT"));

        if (bookmarks.Count == 0)
        {
            GUI.WrappedText(LOC.Get("HAVOK_Bookmarks_None"));
            return;
        }

        var shownBinder = View.Selection.BinderFileEntry?.Filename;
        var shownFile = View.Selection.FilePath;

        // The shown file's first, then by file
        var groups = bookmarks
            .GroupBy(b => (b.Binder, b.File))
            .OrderBy(g => string.Equals(g.Key.Binder, shownBinder, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(g.Key.File, shownFile, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(g => g.Key.Binder, StringComparer.OrdinalIgnoreCase)
            .ThenBy(g => g.Key.File, StringComparer.OrdinalIgnoreCase)
            .ToList();

        HavokBehaviorBookmark remove = null;

        ImGui.BeginChild("havokBookmarksList");

        foreach (var group in groups)
        {
            ImGui.TextDisabled(group.First().FileLabel);

            foreach (var bookmark in group)
            {
                ImGui.PushID($"{bookmark.Binder}|{bookmark.File}|{bookmark.TypeName}|{bookmark.Name}|{bookmark.Ordinal}");

                if (ImGui.SmallButton("x"))
                    remove = bookmark;

                GUI.Tooltip(LOC.Get("HAVOK_Bookmarks_Remove_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{bookmark.Label}##bookmark"))
                    Jump(bookmark);

                GUI.Tooltip(LOC.Get("HAVOK_Bookmarks_Jump_TT", bookmark.TypeName));

                ImGui.PopID();
            }

            ImGui.Spacing();
        }

        ImGui.EndChild();

        if (remove != null)
            Behavior.Bookmarks.Remove(remove);
    }

    /// <summary>
    /// Opens the bookmark's file (if it isn't shown) and shows the object, as a search result does
    /// </summary>
    private void Jump(HavokBehaviorBookmark bookmark)
    {
        var binder = Project.Locator.HavokBehaviorFiles.Entries
            .FirstOrDefault(e => string.Equals(e.Filename, bookmark.Binder, StringComparison.OrdinalIgnoreCase));

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Select_No_Binder", bookmark.Binder));
            return;
        }

        Behavior.JumpTo(new HavokScanResult(binder, bookmark.File, bookmark.Label, HavokScanJump.Outline, -1, bookmark.Locator, null));
    }
}
