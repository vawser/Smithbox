using SoulsFormats;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

public enum SoundContainerKind
{
    /// <summary>A loose FMOD .fsb</summary>
    Fsb,
    /// <summary>A DS2 /sound/*.bnd holding .fev + .fsb</summary>
    FsbBinder,
    /// <summary>A Wwise .bnk</summary>
    WwiseBank,
}

public enum SoundLoadState
{
    NotLoaded,
    Loading,
    Loaded,
    Failed,
}

/// <summary>
/// One sound bank as listed in the Sound Editor. Listed from the file dictionary without reading anything; the file
/// is read and parsed the first time the container is selected (<see cref="BeginLoad"/>), on a worker.
/// </summary>
public class SoundContainer
{
    public SoundData Data;

    /// <summary>VFS path, e.g. /sound/fdp_c1070.fsb or /cs_c2010.bnk</summary>
    public string Path;

    /// <summary>File stem: fdp_c1070, cs_c2010</summary>
    public string Name;

    /// <summary>Category for the list: cs, aeg, s, vc (Wwise); c, m, rm, p (FMOD)</summary>
    public string Group;

    /// <summary>Wwise language folder (enus, ja) or empty for SFX</summary>
    public string Language = "";

    public SoundContainerKind Kind;

    public SoundLoadState State { get; private set; }
    public string Error;

    public List<SoundEntry> Entries = new();

    /// <summary>Wwise: the bank's events (empty for FMOD until the FEV parser, S4b).</summary>
    public List<SoundEvent> Events = new();

    private string _fileAlias;
    private bool _fileAliasDone;

    /// <summary>The character / asset / map the bank belongs to, from the project's aliases (worked out once).</summary>
    public string FileAlias
    {
        get
        {
            if (!_fileAliasDone)
            {
                _fileAlias = Data?.Names?.FileAlias(this);
                _fileAliasDone = Data?.Project.Handler.ProjectData?.Aliases != null;
            }
            return _fileAlias;
        }
    }

    // shown in Properties once loaded
    public string FormatInfo = "";
    public int EventCount;
    public int GeneratedCount;

    // Kept while loaded: FSB samples and in-bank wems are handed to vgmstream from these
    private readonly Dictionary<string, byte[]> _bytes = new();

    private static readonly Regex GroupPattern = new(@"^([a-z]+)", RegexOptions.IgnoreCase);
    private static readonly string[] FmodPrefixes = { "fdp_", "frpg2_", "frpg_", "sprj_", "fdlc_" };

    public static SoundContainer Create(SoundData data, string path, SoundContainerKind kind)
    {
        var name = System.IO.Path.GetFileName(path);
        int dot = name.IndexOf('.');
        if (dot > 0) name = name[..dot];

        var c = new SoundContainer { Data = data, Path = path, Name = name, Kind = kind };

        if (kind == SoundContainerKind.WwiseBank)
        {
            var folder = path[..path.LastIndexOf('/')].Trim('/');
            var first = folder.Split('/')[0];
            if (SoundData.LanguageFolders.Contains(first)) c.Language = first;
            else if (folder.Contains("/eus", StringComparison.OrdinalIgnoreCase)) c.Language = "eus";
            c.Group = GroupPattern.Match(name).Groups[1].Value.ToLowerInvariant();
        }
        else
        {
            var stem = name;
            foreach (var p in FmodPrefixes)
                if (stem.StartsWith(p, StringComparison.OrdinalIgnoreCase)) { stem = stem[p.Length..]; break; }
            c.Group = GroupPattern.Match(stem).Groups[1].Value.ToLowerInvariant();
        }

        if (string.IsNullOrEmpty(c.Group)) c.Group = "other";
        return c;
    }

    /// <summary>Starts reading the file, if not already loaded or loading.</summary>
    public void BeginLoad()
    {
        if (State is SoundLoadState.Loading or SoundLoadState.Loaded)
            return;

        State = SoundLoadState.Loading;
        Error = null;

        Task.Run(() =>
        {
            try
            {
                _loadedEvents = new();
                var entries = Load();
                Entries = entries;
                Events = _loadedEvents;
                State = SoundLoadState.Loaded;
            }
            catch (Exception e)
            {
                Error = e.Message;
                State = SoundLoadState.Failed;
                Smithbox.LogError(this, LOC.Get("SOUND_Data_Log_Failed_to_Read", Path, e.Message));
            }
        });
    }

    /// <summary>Drops the parsed data and bytes (reloaded on next selection).</summary>
    public void Unload()
    {
        if (State == SoundLoadState.Loading) return;
        Entries = new();
        Events = new();
        _bytes.Clear();
        State = SoundLoadState.NotLoaded;
        VgmstreamCli.Forget(CacheKey(null));
    }

    private string CacheKey(string inner) => inner == null ? $"{Data.Project.Descriptor.ProjectPath}|{Path}" : $"{Data.Project.Descriptor.ProjectPath}|{Path}|{inner}";

    private List<SoundEntry> Load()
    {
        var raw = Data.ReadFile(Path) ?? throw new FileNotFoundException(LOC.Get("SOUND_Data_Log_Not_Found", Path));

        switch (Kind)
        {
            case SoundContainerKind.Fsb:
                {
                    var entries = LoadFsb(raw, null);
                    // the FMOD Designer project of the same name holds the events, plus projects shared by several banks
                    _loadedEvents = LoadFevEvents(Data.ReadFile(Path[..^4] + ".fev"), entries);
                    return entries;
                }
            case SoundContainerKind.FsbBinder:
                return LoadBinder(raw);
            default:
                return LoadBank(raw);
        }
    }

    /// <summary>FMOD: what the .fev project says about this bank (shown in Properties).</summary>
    public string FevInfo;

    /// <summary>
    /// The bank's events: every event of its own project (<paramref name="ownFev"/>, may be null), then the events of
    /// projects shared by several banks that play something here (DS1 frpg.fev / fdlc.fev, DS2S frpg2.fevbnd.dcx).
    /// </summary>
    private List<SoundEvent> LoadFevEvents(byte[] ownFev, List<SoundEntry> entries)
    {
        var events = new List<SoundEvent>();
        var info = new List<string>();

        if (ownFev != null)
        {
            var fev = FevFile.Read(ownFev);
            if (fev.SoundDefs.Count == 0 && fev.Events.Count == 0)
                info.Add(fev.Error ?? LOC.Get("SOUND_Fev_Empty"));
            else
            {
                info.Add(LOC.Get(fev.Legacy ? "SOUND_Fev_Info_Legacy" : "SOUND_Fev_Info", fev.Events.Count, fev.SoundDefs.Count));
                events.AddRange(BuildFevEvents(fev, fev.Events, entries, null));
            }
        }

        var shared = new List<string>();
        foreach (var (name, fev) in Data.FevCatalog.ProjectsFor(Name))
        {
            var mine = fev.EventsForBank(Name);
            if (mine.Count == 0)
                continue;
            events.AddRange(BuildFevEvents(fev, mine, entries, Name));
            shared.Add($"{name}.fev ({mine.Count})");
        }
        if (shared.Count > 0)
            info.Add(LOC.Get("SOUND_Fev_Info_Shared", string.Join(", ", shared)));

        FevInfo = info.Count > 0 ? string.Join("; ", info) : null;

        events = events.OrderBy(e => e.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
        for (int i = 0; i < events.Count; i++) events[i].Index = i;
        return events;
    }

    /// <summary>
    /// Events of a project, each linked to the bank's samples its layers' sound definitions play. A waveform is the
    /// sample at its index if the name matches, else the sample of that name, else it is in another bank (SDT keeps
    /// shared waveforms in other .fsb files). For a shared project (<paramref name="bank"/> set), only waveforms whose
    /// bank is this one can match: the same index in another bank is another sample.
    /// </summary>
    private List<SoundEvent> BuildFevEvents(FevFile fev, List<FevFile.Event> fevEvents, List<SoundEntry> entries, string bank)
    {
        var byIndex = new Dictionary<int, SoundEntry>();
        var byName = new Dictionary<string, SoundEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (e.SampleIndex >= 0) byIndex.TryAdd(e.SampleIndex, e);
            byName.TryAdd(System.IO.Path.GetFileNameWithoutExtension(e.Name ?? ""), e);
        }

        var events = new List<SoundEvent>();
        foreach (var fe in fevEvents)
        {
            var ev = new SoundEvent
            {
                Container = this, Id = (uint)fe.Offset, FromFev = true, Name = fe.Name, NameSource = SoundEventNameSource.Fev,
                PlayActions = 1, LayerCount = fe.LayerCount, LayersUnread = fe.FirstDefIndex < 0,
            };
            foreach (var di in fe.DefIndices)
            {
                var def = fev.SoundDefs[di];
                ev.SoundDefs.Add(def.Name);
                foreach (var w in def.Waveforms)
                {
                    SoundEntry entry = null;
                    if (bank != null && !string.Equals(w.BankName, bank, StringComparison.OrdinalIgnoreCase))
                        entry = null;
                    else if (byIndex.TryGetValue(w.Index, out var atIndex) &&
                        System.IO.Path.GetFileNameWithoutExtension(atIndex.Name ?? "").Equals(w.FileStem, StringComparison.OrdinalIgnoreCase))
                        entry = atIndex;
                    else
                        byName.TryGetValue(w.FileStem, out entry);

                    if (entry == null) { ev.ExternalTargets++; continue; }
                    if (!ev.Entries.Contains(entry))
                    {
                        ev.Entries.Add(entry);
                        entry.PlayedBy.Add(ev);
                    }
                }
            }
            var sid = SoundNameRules.ParseSoundId(ev.Name);
            ev.SoundType = sid?.Type;
            ev.SoundId = sid?.Id;
            events.Add(ev);
        }
        return events;
    }

    private List<SoundEntry> LoadBinder(byte[] raw)
    {
        Memory<byte> data = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
        IBinder bnd = data.Span[3] == '4' ? BND4.Read(data) : BND3.Read(data);

        var entries = new List<SoundEntry>();
        var formats = new List<string>();
        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".fsb", StringComparison.OrdinalIgnoreCase)))
        {
            var inner = System.IO.Path.GetFileName(f.Name.Replace('\\', '/'));
            var list = LoadFsb(f.Bytes.ToArray(), inner);
            formats.Add(FormatInfo);
            foreach (var e in list) { e.Index = entries.Count; entries.Add(e); }
        }
        FormatInfo = string.Join(", ", formats.Distinct());

        // DS2: the binder's .fev (RIFF or FEV1); DS2S binders have none, their events are in frpg2.fevbnd.dcx
        var fev = bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".fev", StringComparison.OrdinalIgnoreCase));
        _loadedEvents = LoadFevEvents(fev?.Bytes.ToArray(), entries);
        return entries;
    }

    private List<SoundEntry> LoadFsb(byte[] raw, string inner)
    {
        var fsb = FsbFile.Read(raw);
        var key = inner ?? "";
        _bytes[key] = fsb.Bytes;
        FormatInfo = fsb.WasEncrypted ? $"{fsb.Version} {fsb.Codec} ({LOC.Get("SOUND_Label_Encrypted")})" : $"{fsb.Version} {fsb.Codec}";

        var cacheKey = CacheKey(inner);
        var entries = new List<SoundEntry>();
        foreach (var s in fsb.Samples)
        {
            var e = new SoundEntry
            {
                Container = this,
                Index = entries.Count,
                SampleIndex = s.Index,
                Name = s.Name ?? $"#{s.Index}",
                InnerFile = inner,
                Codec = s.Codec,
                SampleRate = s.SampleRate,
                Channels = s.Channels,
                Seconds = s.Seconds,
                LoopStart = s.LoopStart,
                LoopEnd = s.LoopEnd,
                IsEmpty = !s.HasData,
            };
            int subsong = s.Index + 1;
            e.Source = new SoundSource
            {
                Name = $"{Path}{(inner != null ? "|" + inner : "")} #{subsong}",
                Extension = "fsb",
                Subsong = subsong,
                CacheKey = cacheKey,
                ReadBytes = () => _bytes.TryGetValue(key, out var b) ? b : null,
            };
            e.ParseSoundId();
            entries.Add(e);
        }
        return entries;
    }

    private List<SoundEntry> LoadBank(byte[] raw)
    {
        var bank = WwiseBank.Read(raw);
        _bytes[""] = raw;
        FormatInfo = $"Wwise v{bank.Version}";
        EventCount = bank.EventIds.Count;
        GeneratedCount = bank.Sounds.Count(s => s.IsGenerated);

        var soundsByMedia = bank.Sounds.Where(s => !s.IsGenerated).GroupBy(s => s.SourceId).ToDictionary(g => g.Key, g => g.ToList());
        var inBank = bank.InBankMedia.ToDictionary(m => m.Id, m => m);

        // every media the bank's Sound objects play, then any in-bank media no Sound object names
        var ids = new List<uint>();
        var seen = new HashSet<uint>();
        foreach (var s in bank.Sounds)
            if (!s.IsGenerated && seen.Add(s.SourceId)) ids.Add(s.SourceId);
        foreach (var m in bank.InBankMedia)
            if (seen.Add(m.Id)) ids.Add(m.Id);

        var entries = new List<SoundEntry>();
        foreach (var id in ids)
        {
            soundsByMedia.TryGetValue(id, out var sounds);
            bool streamedByObject = sounds?.Any(s => s.IsStreamed) == true;
            var streamPath = streamedByObject || !inBank.ContainsKey(id) ? Data.FindStreamedWem(id, Language) : null;

            var e = new SoundEntry
            {
                Container = this,
                Index = entries.Count,
                MediaId = id,
                SoundObjectCount = sounds?.Count ?? 0,
                Codec = sounds != null ? WwiseBank.PluginName(sounds[0].PluginId) : "",
                Name = Data.Names?.MediaName(id) ?? "",
                SourceFile = Data.Names?.MediaSourcePath(id),
            };
            e.ParseSoundId();

            // the in-bank header (also present for prefetched sounds) gives format and the full length
            WemHeader header = null;
            if (inBank.TryGetValue(id, out var im) && im.Size > 0)
            {
                header = WemHeader.Read(raw.AsSpan(im.Offset, Math.Min(im.Size, 512)));
                if (header != null)
                {
                    e.Codec = header.Codec;
                    e.SampleRate = header.SampleRate;
                    e.Channels = header.Channels;
                    e.Seconds = header.Seconds;
                    e.SecondsApprox = !header.SecondsExact;
                }
            }

            // prefetched sounds have the start in DATA and the whole sound streamed: play the streamed file
            if (streamPath != null)
            {
                e.IsStreamed = true;
                e.StreamPath = streamPath;
                e.Source = new SoundSource
                {
                    Name = streamPath,
                    Extension = "wem",
                    CacheKey = $"{Data.Project.Descriptor.ProjectPath}|{streamPath}",
                    ReadBytes = () => Data.ReadFile(streamPath),
                };
            }
            else if (inBank.TryGetValue(id, out var m) && header == null)
            {
                // Wwise plug-in media (effect / IR data: AC6 "00 04 02 00…", DB_NT "PLUG"), not a wem
                e.IsNotAudio = true;
                e.Codec = LOC.Get("SOUND_Codec_Plugin_Data");
            }
            else if (inBank.TryGetValue(id, out m))
            {
                e.IsEmpty = m.Size == 0;
                var offset = m.Offset;
                var size = m.Size;
                e.Source = new SoundSource
                {
                    Name = $"{Path}#{id}",
                    Extension = "wem",
                    CacheKey = $"{Data.Project.Descriptor.ProjectPath}|{Path}#{id}",
                    ReadBytes = () => _bytes.TryGetValue("", out var b) ? b.AsSpan(offset, size).ToArray() : null,
                };
            }
            else
            {
                // streamed, but no .wem with this ID in the dictionary
                e.IsStreamed = true;
                e.IsEmpty = true;
            }

            entries.Add(e);
        }

        if (bank.HircBody >= 0)
            _loadedEvents = BuildEvents(raw, bank, entries);

        return entries;
    }

    // built on the worker, published with Entries
    private List<SoundEvent> _loadedEvents = new();

    /// <summary>The bank's events, the sounds each can play (HIRC Event → Action → node → descendants), and their names.</summary>
    private List<SoundEvent> BuildEvents(byte[] raw, WwiseBank bank, List<SoundEntry> entries)
    {
        var hirc = WwiseHirc.Read(raw, bank.HircBody, bank.Version);
        var byMedia = new Dictionary<uint, SoundEntry>();
        foreach (var e in entries) byMedia.TryAdd(e.MediaId, e);

        var events = new List<SoundEvent>();
        foreach (var he in hirc.Events)
        {
            var ev = new SoundEvent { Container = this, Index = events.Count, Id = he.Id };
            foreach (var aid in he.ActionIds)
            {
                if (!hirc.Actions.TryGetValue(aid, out var a)) { ev.OtherActions++; continue; }
                if (a.IsPlay)
                {
                    ev.PlayActions++;
                    if (!hirc.Nodes.ContainsKey(a.TargetId)) ev.ExternalTargets++;
                }
                else if (a.IsStop) ev.StopActions++;
                else ev.OtherActions++;
            }
            foreach (var mid in hirc.EventMedia(he))
                if (byMedia.TryGetValue(mid, out var entry))
                {
                    ev.Entries.Add(entry);
                    entry.PlayedBy.Add(ev);
                }

            var tableName = Data.Names?.EventName(he.Id);
            if (tableName != null) { ev.Name = tableName; ev.NameSource = SoundEventNameSource.Table; }
            events.Add(ev);
        }

        // unnamed events: the Play_/Stop_ + type + 9-digit rule, from the bank's own media sound IDs, then its ID range
        var unnamed = new HashSet<uint>(events.Where(e => e.NameSource == SoundEventNameSource.None).Select(e => e.Id));
        if (unnamed.Count > 0)
        {
            var soundIds = entries.Where(e => e.SoundId != null).Select(e => (e.SoundType, e.SoundId)).Distinct().ToList();
            var found = SoundNameRules.MatchEventNames(unnamed, soundIds, Name, out var guessed);
            foreach (var ev in events)
                if (ev.NameSource == SoundEventNameSource.None && found.TryGetValue(ev.Id, out var n))
                {
                    ev.Name = n;
                    ev.NameSource = guessed.Contains(ev.Id) ? SoundEventNameSource.Guess : SoundEventNameSource.Rule;
                }
        }

        foreach (var ev in events)
        {
            var sid = SoundNameRules.ParseSoundId(ev.Name);
            ev.SoundType = sid?.Type;
            ev.SoundId = sid?.Id;
        }

        // Play events first, then by name (Stop_ events follow their Play_)
        events = events.OrderBy(e => e.DisplayName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                       .ThenBy(e => e.NameSource == SoundEventNameSource.None ? 1 : 0)
                       .ThenBy(e => e.DisplayName, StringComparer.OrdinalIgnoreCase)
                       .ToList();
        for (int i = 0; i < events.Count; i++) events[i].Index = i;
        return events;
    }
}
