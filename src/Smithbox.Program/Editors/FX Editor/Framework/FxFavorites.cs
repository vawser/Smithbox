using System.IO;
using System.Text.Json;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Favorite effects, kept with the project in &lt;project&gt;/.smithbox/Project/FX/Favorites.json, and the effects
/// selected recently in this session (FX Effect List Actions Plan, phase C).
/// </summary>
public class FxFavorites
{
    private readonly FxEditorScreen Editor;
    private HashSet<int> Ids;

    /// <summary>Most recent first.</summary>
    public List<int> Recent { get; } = new();
    private const int RecentCount = 20;

    public FxFavorites(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private string FilePath => Path.Join(Editor.Project.Descriptor.ProjectPath, ".smithbox", "Project", "FX", "Favorites.json");

    private HashSet<int> Load()
    {
        if (Ids != null)
            return Ids;
        Ids = new();
        try
        {
            if (File.Exists(FilePath))
                Ids = JsonSerializer.Deserialize<List<int>>(File.ReadAllText(FilePath))?.ToHashSet() ?? new();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Favorites_Failed_Load", FilePath), e);
        }
        return Ids;
    }

    public bool IsFavorite(int id) => Load().Contains(id);

    public int Count => Load().Count;

    public void Set(IEnumerable<int> ids, bool favorite)
    {
        var set = Load();
        foreach (var id in ids)
        {
            if (favorite)
                set.Add(id);
            else
                set.Remove(id);
        }
        Save();
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Ids.OrderBy(i => i).ToList()));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Favorites_Failed_Save", FilePath), e);
        }
    }

    public void Visited(int id)
    {
        Recent.Remove(id);
        Recent.Insert(0, id);
        if (Recent.Count > RecentCount)
            Recent.RemoveAt(Recent.Count - 1);
    }
}
