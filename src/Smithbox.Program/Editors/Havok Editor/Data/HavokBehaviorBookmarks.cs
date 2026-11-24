using StudioCore.Application;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// A bookmarked behavior object: its binder (by name), file (its path in the binder), and a locator that finds it
/// again when the file is read (type, name, place among same-named objects of that type), with the label it had
/// </summary>
public record HavokBehaviorBookmark(string Binder, string File, string TypeName, string Name, int Ordinal, string Label)
{
    public HavokObjectLocator Locator => new(TypeName, Name, Ordinal);

    public string FileLabel => $"{Binder}/{Path.GetFileName(File.Replace('\\', '/'))}";

    public bool Matches(HavokBehaviorBookmark other) =>
        string.Equals(Binder, other.Binder, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(File, other.File, StringComparison.OrdinalIgnoreCase) &&
        TypeName == other.TypeName && Name == other.Name && Ordinal == other.Ordinal;
}

/// <summary>
/// The Behavior Editor's bookmarks, per project: kept one per line (tab-separated) in the project's .smithbox folder,
/// as the File Browser keeps its bookmarks, so they travel with the project. Never stored in the behavior files.
/// </summary>
public class HavokBehaviorBookmarks
{
    private readonly ProjectEntry Project;
    private readonly List<HavokBehaviorBookmark> Items = new();

    public IReadOnlyList<HavokBehaviorBookmark> Entries => Items;

    public HavokBehaviorBookmarks(ProjectEntry project)
    {
        Project = project;
        Load();
    }

    private string FilePath => Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "BehaviorBookmarks.txt");

    /// <summary>
    /// The bookmark an object of the shown file would have, or null if it can't be located
    /// </summary>
    public static HavokBehaviorBookmark For(HavokBehaviorSymbols symbols, FileDictionaryEntry binder, string file, object obj)
    {
        if (symbols == null || binder == null || file == null || obj == null)
            return null;

        var locator = new HavokLocatorIndex(symbols).Get(obj);

        if (locator == null)
            return null;

        return new(binder.Filename, file, locator.TypeName, locator.Name, locator.Ordinal, HavokBehaviorSymbols.GetOutlineLabel(obj));
    }

    public bool Contains(HavokBehaviorBookmark bookmark) => bookmark != null && Items.Any(b => b.Matches(bookmark));

    public void Toggle(HavokBehaviorBookmark bookmark)
    {
        if (bookmark == null)
            return;

        var existing = Items.FindIndex(b => b.Matches(bookmark));

        if (existing >= 0)
            Items.RemoveAt(existing);
        else
            Items.Add(bookmark);

        Save();
    }

    public void Remove(HavokBehaviorBookmark bookmark)
    {
        if (Items.RemoveAll(b => b.Matches(bookmark)) > 0)
            Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return;

            foreach (var line in File.ReadAllLines(FilePath))
            {
                var parts = line.Split('\t');

                if (parts.Length < 6 || !int.TryParse(parts[4], out var ordinal))
                    continue;

                var bookmark = new HavokBehaviorBookmark(parts[0], parts[1], parts[2], parts[3], ordinal, parts[5]);

                if (!Contains(bookmark))
                    Items.Add(bookmark);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Bookmarks_Load_Failed", FilePath), e);
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            // Tabs and line breaks can't appear in a field (names don't have them; labels are cleaned)
            static string Clean(string s) => (s ?? "").Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');

            File.WriteAllLines(FilePath, Items.Select(b =>
                string.Join('\t', Clean(b.Binder), Clean(b.File), Clean(b.TypeName), Clean(b.Name), b.Ordinal.ToString(), Clean(b.Label))));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Bookmarks_Save_Failed", FilePath), e);
        }
    }
}
