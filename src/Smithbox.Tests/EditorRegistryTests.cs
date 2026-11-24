using StudioCore.Editors.Common;

namespace SmithboxTests;

/// <summary>
///     The shared editor stub draws every editor's window from its registry entry, so the entries must be complete
///     and their ids distinct.
/// </summary>
public class EditorRegistryTests
{
    public static TheoryData<string> EntryIds()
    {
        var data = new TheoryData<string>();

        foreach (var entry in EditorRegistry.Entries)
            data.Add(entry.Id);

        return data;
    }

    [Theory]
    [MemberData(nameof(EntryIds))]
    public void Entry_IsComplete(string id)
    {
        var entry = EditorRegistry.FindById(id);

        Assert.NotNull(entry);
        Assert.False(string.IsNullOrWhiteSpace(entry.LabelKey));
        Assert.False(string.IsNullOrWhiteSpace(entry.WindowId));
        Assert.NotNull(entry.IsAvailable);
        Assert.NotNull(entry.Screen);
    }

    [Fact]
    public void Ids_AreDistinct()
    {
        var ids = EditorRegistry.Entries.Select(e => e.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    /// <summary>Two editors sharing a window id would share (and fight over) one ImGui window</summary>
    [Fact]
    public void WindowIds_AreDistinct()
    {
        var ids = EditorRegistry.Entries.Select(e => e.WindowId).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Ids_DontClashWithTheProjectOrSettingsEditor()
    {
        Assert.DoesNotContain(EditorRegistry.Entries,
            e => e.Id is EditorRegistry.ProjectEditorId or EditorRegistry.SettingsEditorId);
    }

    [Fact]
    public void OnlyTheMaterialEditor_IsDefocusedWhenHidden()
    {
        // Kept as it was before the per-editor stubs were merged. Other screens implement OnDefocus but were never
        // called: widen this deliberately, not by accident.
        var ids = EditorRegistry.Entries.Where(e => e.DefocusWhenHidden).Select(e => e.Id);

        Assert.Equal(["material"], ids);
    }
}
