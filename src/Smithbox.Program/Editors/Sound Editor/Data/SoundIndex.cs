using SoulsFormats;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

/// <summary>A sound event found by the index: which bank holds it, under which name.</summary>
public sealed class SoundMatch
{
    public SoundContainer Container;

    /// <summary>The event name (Play_s000806100 / c107001001), or the hash as text when unknown.</summary>
    public string EventName;

    /// <summary>Wwise: the event ID. FMOD: 0 (events are matched by name).</summary>
    public uint EventId;

    public string SoundType;
    public string SoundId;

    public string Label => string.IsNullOrEmpty(Container.FileAlias) ? $"{EventName}  ({Container.Name})" : $"{EventName}  ({Container.Name}, {Container.FileAlias})";
}

/// <summary>
/// Finds which bank holds a sound without opening every bank (plan §7e):
/// <list type="bullet">
/// <item>Wwise: soundbanksinfo.mobnkinfo (bank → events; all of every game's bank events are in it, under the right
/// bank), and the event name rule Play_ + type + 9-digit ID.</item>
/// <item>FMOD: the event names of each bank's own .fev (RIFF STRR, or FEV1 inline), DS2 binders' inner .fev, and shared
/// projects (DS1 frpg.fev / fdlc.fev, DS2S frpg2.fevbnd.dcx) under the banks their events play in.</item>
/// </list>
/// A bare number (params, TAE, MSB) has no type letter, so it is tried with every letter in use; 90–98% of sound IDs
/// match one letter only.
/// </summary>
public class SoundIndex
{
    private readonly SoundData Data;

    public static readonly string[] Letters = { "a", "b", "c", "d", "e", "f", "g", "i", "m", "o", "p", "s", "v", "w", "x", "y", "z" };

    public bool IsBuilt { get; private set; }
    public bool IsBuilding { get; private set; }
    public string Error { get; private set; }

    // Wwise: event ID -> banks
    private Dictionary<uint, List<SoundContainer>> _wwise = new();
    // FMOD: lowercased event name -> banks
    private Dictionary<string, List<SoundContainer>> _fmod = new(StringComparer.OrdinalIgnoreCase);

    public int EventCount => Data.Middleware == SoundMiddleware.Wwise ? _wwise.Count : _fmod.Count;

    public SoundIndex(SoundData data)
    {
        Data = data;
    }

    /// <summary>Starts building on a worker (once). Lookups return nothing until <see cref="IsBuilt"/>.</summary>
    public void EnsureBuilt()
    {
        if (IsBuilt || IsBuilding)
            return;

        IsBuilding = true;
        Task.Run(() =>
        {
            try
            {
                if (Data.Middleware == SoundMiddleware.Wwise) BuildWwise();
                else BuildFmod();
                IsBuilt = true;
                Smithbox.Log(this, LOC.Get("SOUND_Index_Log_Built", EventCount));
            }
            catch (Exception e)
            {
                Error = e.Message;
                Smithbox.LogError(this, LOC.Get("SOUND_Index_Log_Failed", e.Message));
            }
            finally
            {
                IsBuilding = false;
            }
        });
    }

    private void BuildWwise()
    {
        var path = Data.FindPath("soundbanksinfo.mobnkinfo") ?? throw new FileNotFoundException("soundbanksinfo.mobnkinfo");
        var info = SoundBanksInfo.Read(Data.ReadFile(path));

        // bank ID = FNV-1 of the bank file's name; language folders can hold banks of the same name
        var banks = new Dictionary<uint, List<SoundContainer>>();
        foreach (var c in Data.Containers.Where(c => c.Kind == SoundContainerKind.WwiseBank))
        {
            var id = WwiseBank.Hash(c.Name);
            (banks.TryGetValue(id, out var l) ? l : banks[id] = new()).Add(c);
        }

        var map = new Dictionary<uint, List<SoundContainer>>();
        foreach (var (eventId, bankIds) in info.EventBanks)
        {
            var list = new List<SoundContainer>();
            foreach (var b in bankIds)
                if (banks.TryGetValue(b, out var cs)) list.AddRange(cs);
            if (list.Count > 0) map[eventId] = list;
        }
        _wwise = map;
    }

    private void BuildFmod()
    {
        var map = new Dictionary<string, List<SoundContainer>>(StringComparer.OrdinalIgnoreCase);
        void Add(string name, SoundContainer c)
        {
            if (!Regex.IsMatch(name, @"^[a-z]\d{9,10}$")) return;
            var l = map.TryGetValue(name, out var x) ? x : map[name] = new();
            if (!l.Contains(c)) l.Add(c);
        }

        // own projects: the loose .fev next to each .fsb
        foreach (var c in Data.Containers.Where(c => c.Kind == SoundContainerKind.Fsb))
        {
            var fev = Data.ReadFile(c.Path[..^4] + ".fev");
            if (fev != null)
                foreach (var name in FevEventNames(fev)) Add(name, c);
        }

        // DS2: the .fev inside each binder. DS2S binders hold only the .fsb (its projects are in frpg2.fevbnd.dcx), so
        // they aren't opened.
        bool sharedProjectGame = Data.FmodProjectPaths().Any(p => p.Contains(".fevbnd", StringComparison.OrdinalIgnoreCase));
        if (!sharedProjectGame)
        {
            foreach (var c in Data.Containers.Where(c => c.Kind == SoundContainerKind.FsbBinder))
            {
                var raw = Data.ReadFile(c.Path);
                if (raw == null) continue;
                try
                {
                    Memory<byte> d = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
                    IBinder bnd = d.Span[3] == '4' ? BND4.Read(d) : BND3.Read(d);
                    var fev = bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".fev", StringComparison.OrdinalIgnoreCase));
                    if (fev != null)
                        foreach (var name in FevEventNames(fev.Bytes.ToArray())) Add(name, c);
                }
                catch { }
            }
        }

        // shared projects (DS1 frpg.fev / fdlc.fev, DS2S frpg2.fevbnd.dcx): each event under the banks it plays in
        var byStem = Data.Containers.Where(c => c.Kind is SoundContainerKind.Fsb or SoundContainerKind.FsbBinder)
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        foreach (var (_, fev) in Data.FevCatalog.AllProjects())
            foreach (var bank in fev.BankNames)
                if (byStem.TryGetValue(bank, out var cs))
                    foreach (var ev in fev.EventsForBank(bank))
                        foreach (var c in cs) Add(ev.Name, c);

        _fmod = map;
    }

    /// <summary>RIFF FEV: the STRR strings (cheap); FEV1: the parsed events (names are inline).</summary>
    private static IEnumerable<string> FevEventNames(byte[] fev)
        => fev.Length >= 4 && fev[0] == 'F' && fev[1] == 'E' && fev[2] == 'V' && fev[3] == '1'
            ? FevFile.Read(fev).EventNames()
            : SoundBanksInfo.FevStrings(fev);

    /// <summary>
    /// The Play events of a sound ID. With <paramref name="type"/> null, every type letter is tried (params and TAE give
    /// only the number).
    /// </summary>
    public List<SoundMatch> Find(string soundId, string type = null)
    {
        var result = new List<SoundMatch>();
        if (!IsBuilt || !long.TryParse(soundId, out var n) || n < 0)
            return result;

        foreach (var t in type != null ? new[] { type } : Letters)
        {
            if (Data.Middleware == SoundMiddleware.Wwise)
            {
                var name = SoundNameRules.EventName("Play", t, n.ToString());
                var id = WwiseBank.Hash(name);
                if (_wwise.TryGetValue(id, out var cs))
                    foreach (var c in cs)
                        result.Add(new SoundMatch { Container = c, EventName = name, EventId = id, SoundType = t, SoundId = n.ToString() });
            }
            else
            {
                var name = $"{t}{n:D9}";
                if (_fmod.TryGetValue(name, out var cs))
                    foreach (var c in cs)
                        result.Add(new SoundMatch { Container = c, EventName = name, SoundType = t, SoundId = n.ToString() });
            }
        }
        return result;
    }

    /// <summary>
    /// Search by text: a number is a sound ID; otherwise event names (Wwise: the name table, FMOD: .fev names) and Sounds
    /// alias names containing the text.
    /// </summary>
    public List<SoundMatch> Search(string text, int max = 500)
    {
        var result = new List<SoundMatch>();
        if (!IsBuilt || string.IsNullOrWhiteSpace(text))
            return result;
        text = text.Trim();

        // a sound ID, with or without its type letter (806100, s000806100, Play_s000806100)
        if (long.TryParse(text, out _))
            return Find(text);
        var sid = SoundNameRules.ParseSoundId(text);
        if (sid != null)
            return Find(sid.Value.Id, sid.Value.Type);

        // Sounds aliases by name
        var aliases = Data.Project.Handler.ProjectData?.Aliases;
        if (aliases != null && aliases.TryGetValue(StudioCore.Editors.MetadataEditor.ProjectAliasType.Sounds, out var sounds))
            foreach (var a in sounds.Where(a => a.Name != null && a.Name.Contains(text, StringComparison.OrdinalIgnoreCase)))
            {
                result.AddRange(Find(a.ID));
                if (result.Count >= max) return result;
            }

        // event names
        if (Data.Middleware == SoundMiddleware.Wwise)
        {
            foreach (var (id, cs) in _wwise)
            {
                var name = Data.Names.EventName(id);
                if (name == null || !name.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;
                var s = SoundNameRules.ParseSoundId(name);
                foreach (var c in cs)
                    result.Add(new SoundMatch { Container = c, EventName = name, EventId = id, SoundType = s?.Type, SoundId = s?.Id });
                if (result.Count >= max) break;
            }
        }
        else
        {
            foreach (var (name, cs) in _fmod)
            {
                if (!name.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;
                var s = SoundNameRules.ParseSoundId(name);
                foreach (var c in cs)
                    result.Add(new SoundMatch { Container = c, EventName = name, SoundType = s?.Type, SoundId = s?.Id });
                if (result.Count >= max) break;
            }
        }
        return result;
    }

    /// <summary>The editor command that opens a match: sound/event/&lt;bank path with '|' for '/'&gt;/&lt;event name&gt;[/play]</summary>
    public static string Command(SoundMatch m, bool play)
        => $"sound/event/{m.Container.Path.Replace('/', '|')}/{m.EventName}{(play ? "/play" : "")}";
}
