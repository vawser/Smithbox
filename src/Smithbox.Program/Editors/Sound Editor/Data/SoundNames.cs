using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The names the Sound Editor shows, from three sources (Claude/Documentation/Sound Editor Plan.md §3):
/// <list type="number">
/// <item>File aliases: a bank's character / asset / map, from the project's Characters, Assets and MapNames aliases.</item>
/// <item>Sound aliases: the project's Sounds aliases, keyed by the sound ID inside a sample or media name
/// (<c>c107001001b</c> → 107001001; <c>Play_s000806100</c> → 806100), the same ID params and TAE events use.</item>
/// <item>Wwise names: media, event and bank IDs are hashes or source IDs; Assets/SOUND/Wwise names those Elden Ring's
/// Wwise reports list (also used in NR / AC6 / DB_NT where IDs match).</item>
/// </list>
/// </summary>
public class SoundNames
{
    private readonly ProjectEntry Project;

    private readonly Dictionary<uint, string> _events = new();
    private readonly Dictionary<uint, string> _media = new();
    private readonly Dictionary<uint, string> _banks = new();

    public int EventCount => _events.Count;
    public int MediaCount => _media.Count;

    public SoundNames(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>Reads the shipped Wwise name table (Wwise games only).</summary>
    public void LoadWwise()
    {
        var dir = Path.Join(AppContext.BaseDirectory, "Assets", "SOUND", "Wwise");
        try
        {
            foreach (var name in ReadLines(Path.Join(dir, "Events.txt")))
                _events.TryAdd(WwiseBank.Hash(name), name);

            foreach (var name in ReadLines(Path.Join(dir, "Banks.txt")))
                _banks.TryAdd(WwiseBank.Hash(name), name);

            foreach (var line in ReadLines(Path.Join(dir, "Media.txt")))
            {
                int tab = line.IndexOf('\t');
                if (tab > 0 && uint.TryParse(line.AsSpan(0, tab), out var id))
                    _media.TryAdd(id, line[(tab + 1)..]);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("SOUND_Names_Log_Load_Failed", dir), e);
        }
    }

    private static IEnumerable<string> ReadLines(string path)
    {
        if (!File.Exists(path))
            yield break;
        foreach (var line in File.ReadLines(path))
            if (line.Length > 0 && line[0] != '#')
                yield return line;
    }

    /// <summary>The source name of a Wwise media ID ("c325006008_01"), or null.</summary>
    public string MediaName(uint id)
    {
        if (!_media.TryGetValue(id, out var shortName))
            return null;
        var file = shortName[(shortName.LastIndexOfAny(new[] { '\\', '/' }) + 1)..];
        return Path.GetFileNameWithoutExtension(file);
    }

    /// <summary>The full source path of a Wwise media ID ("cs_c3250\c325006008_01.wav"), or null.</summary>
    public string MediaSourcePath(uint id) => _media.TryGetValue(id, out var s) ? s : null;

    /// <summary>The name of a Wwise event ID (Play_c800001000), or null.</summary>
    public string EventName(uint id) => _events.TryGetValue(id, out var n) ? n : null;


    // ID -> Sounds alias, rebuilt when the list (or its size) changes: the lists look it up per visible row
    private Dictionary<string, AliasEntry> _soundIndex = new();
    private List<AliasEntry> _soundIndexList;
    private int _soundIndexCount = -1, _soundIndexVersion = -1;

    /// <summary>The project's Sounds alias for a sound ID, or null.</summary>
    public AliasEntry SoundAlias(string id)
    {
        if (id == null || Project.Handler.ProjectData?.Aliases == null)
            return null;
        if (!Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Sounds, out var list))
            return null;

        if (!ReferenceEquals(list, _soundIndexList) || list.Count != _soundIndexCount || _soundIndexVersion != _version)
        {
            _soundIndex = new();
            foreach (var a in list)
                if (a.ID != null) _soundIndex.TryAdd(a.ID, a);
            _soundIndexList = list;
            _soundIndexCount = list.Count;
            _soundIndexVersion = _version;
        }

        return _soundIndex.TryGetValue(id, out var e) ? e : null;
    }

    /// <summary>
    /// Sets (or with an empty name, removes) the Sounds alias of a sound ID and writes the project's alias file,
    /// so the Param and Time Act editors show it too.
    /// </summary>
    public void SetSoundAlias(string id, string name)
    {
        var aliases = Project.Handler.ProjectData?.Aliases;
        if (id == null || aliases == null)
            return;
        if (!aliases.TryGetValue(ProjectAliasType.Sounds, out var list))
            aliases[ProjectAliasType.Sounds] = list = new();

        var existing = list.FirstOrDefault(a => a.ID == id);
        if (string.IsNullOrWhiteSpace(name))
        {
            if (existing != null)
                list.Remove(existing);
        }
        else if (existing != null)
            existing.Name = name.Trim();
        else
            list.Add(new AliasEntry { ID = id, Name = name.Trim(), Tags = new() { "sound" } });

        Smithbox.Orchestrator.ProjectEditor.AliasMenu.SaveIndividualAlias(ProjectAliasType.Sounds);
        _version++;
    }

    private int _version;

    /// <summary>Bumped when a Sounds alias changes, so cached labels are rebuilt.</summary>
    public int Version => _version;

    /// <summary>
    /// What the bank is for, from the project's other aliases: a character (cs_c2010, fdp_c1070), an asset (aeg027_002)
    /// or a map (cs_m10, fdp_m30, s10_00_0030). Null if none matches.
    /// </summary>
    public string FileAlias(SoundContainer c)
    {
        var aliases = Project.Handler.ProjectData?.Aliases;
        if (aliases == null)
            return null;

        switch (SoundNameRules.BankAliasKey(c.Name, out var exactId, out var prefix))
        {
            case BankAliasKind.Character:
                return Lookup(aliases, ProjectAliasType.Characters, exactId);
            case BankAliasKind.Asset:
                return Lookup(aliases, ProjectAliasType.Assets, exactId);
            case BankAliasKind.Map:
                // the most specific map alias that exists
                if (!aliases.TryGetValue(ProjectAliasType.MapNames, out var maps))
                    return null;
                var hit = maps.FirstOrDefault(a => a.ID == exactId)
                          ?? maps.FirstOrDefault(a => a.ID.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                return string.IsNullOrEmpty(hit?.Name) ? null : hit.Name;
            default:
                return null;
        }
    }

    private static string Lookup(AliasStore aliases, ProjectAliasType type, string id)
    {
        if (!aliases.TryGetValue(type, out var list))
            return null;
        var e = list.FirstOrDefault(a => string.Equals(a.ID, id, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrEmpty(e?.Name) ? null : e.Name;
    }
}
