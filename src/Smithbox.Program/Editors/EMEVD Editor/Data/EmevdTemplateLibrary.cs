namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The templates the Templates tool offers: the game's built-ins (Assets/EMEVD/Templates/&lt;GAME&gt;.json, written
/// by EmevdEditorCheck templates from vanilla events) and the project's own, one JSON file per template in
/// &lt;project&gt;/.smithbox/Project/EMEVD/Templates/&lt;GAME&gt;/&lt;name&gt;.json. Removing a project template moves its
/// file into an .archive folder beside it, so nothing is lost.
/// </summary>
public class EmevdTemplateLibrary
{
    private readonly EmevdEditorView View;

    private List<EmevdTemplate> BuiltIns;
    private List<EmevdTemplate> ProjectTemplates;
    private string LoadedGame;

    public EmevdTemplateLibrary(EmevdEditorView view)
    {
        View = view;
    }

    private string Game => View.Project.Handler.EmevdData?.Catalog?.Game;

    public string Folder => Game == null ? null
        : Path.Join(View.Project.Descriptor.ProjectPath, ".smithbox", "Project", "EMEVD", "Templates", Game);

    /// <summary>Project templates first (the user's own), then the built-ins</summary>
    public IReadOnlyList<EmevdTemplate> All
    {
        get
        {
            EnsureLoaded();
            return ProjectTemplates.Concat(BuiltIns).ToList();
        }
    }

    public void Refresh() => ProjectTemplates = null;

    private void EnsureLoaded()
    {
        var game = Game;
        if (game != LoadedGame)
        {
            BuiltIns = null;
            ProjectTemplates = null;
            LoadedGame = game;
        }

        if (BuiltIns == null)
        {
            BuiltIns = new();
            var path = game == null ? null : Path.Join(AppContext.BaseDirectory, "Assets", "EMEVD", "Templates", $"{game}.json");

            if (path != null && File.Exists(path))
            {
                try
                {
                    BuiltIns = EmevdTemplates.LoadSet(path).Templates;
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("EMEVD_Templates_Failed_Load", path), e);
                }
            }
        }

        if (ProjectTemplates == null)
        {
            ProjectTemplates = new();
            var folder = Folder;

            if (folder != null && Directory.Exists(folder))
            {
                foreach (var file in Directory.GetFiles(folder, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (EmevdTemplates.Load(file) is { } t)
                            ProjectTemplates.Add(t);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("EMEVD_Templates_Failed_Load", file), e);
                    }
                }
            }
        }
    }

    /// <summary>Makes a name safe for a file name, or null when nothing is left</summary>
    public static string Clean(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string((name ?? "").Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return clean.Length == 0 ? null : clean;
    }

    public bool Exists(string name) => Folder != null && Clean(name) is { } n && File.Exists(Path.Join(Folder, $"{n}.json"));

    public bool Save(EmevdTemplate template)
    {
        var clean = Clean(template.Name);
        if (clean == null || Folder == null)
            return false;

        try
        {
            Directory.CreateDirectory(Folder);
            var path = Path.Join(Folder, $"{clean}.json");
            File.WriteAllText(path, EmevdTemplates.Write(template));
            Refresh();
            Smithbox.Log(this, LOC.Get("EMEVD_Templates_Saved", template.Name, path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Templates_Failed_Save", template.Name), e);
            return false;
        }
    }

    /// <summary>Moves a project template's file into the .archive folder beside it</summary>
    public bool Archive(EmevdTemplate template)
    {
        if (template.BuiltIn || template.FilePath == null || !File.Exists(template.FilePath))
            return false;

        try
        {
            var archive = Path.Join(Path.GetDirectoryName(template.FilePath), ".archive");
            Directory.CreateDirectory(archive);
            var target = Path.Join(archive, $"{Path.GetFileNameWithoutExtension(template.FilePath)}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.Move(template.FilePath, target);
            Refresh();
            Smithbox.Log(this, LOC.Get("EMEVD_Templates_Archived", template.Name, target));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Templates_Failed_Archive", template.Name), e);
            return false;
        }
    }
}
