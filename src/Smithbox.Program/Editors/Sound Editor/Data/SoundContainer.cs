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

        return Kind switch
        {
            SoundContainerKind.Fsb => LoadFsb(raw, null),
            SoundContainerKind.FsbBinder => LoadBinder(raw),
            _ => LoadBank(raw),
        };
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
