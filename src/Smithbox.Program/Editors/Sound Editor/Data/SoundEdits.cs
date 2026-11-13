namespace StudioCore.Editors.SoundEditor;

/// <summary>A replaced sound, not saved yet.</summary>
public sealed class SoundMediaEdit
{
    /// <summary>The whole new .wem.</summary>
    public byte[] Wem;

    /// <summary>HIRC plug-in ID of its codec (PCM for WAV imports).</summary>
    public uint PluginId;

    /// <summary>What it came from, for lists and logs ("hit.wav", "Vanilla").</summary>
    public string Label;

    /// <summary>The streamed .wem it replaces (VFS path), or null for in-bank only media.</summary>
    public string StreamPath;

    /// <summary>A revert to the game's own audio.</summary>
    public bool IsVanilla;
}

/// <summary>
/// The Sound Editor's unsaved replacements (write plan W2), keyed by media ID and language. Saving writes, for each
/// replacement: every bank of that language that holds or references the media (<see cref="SoundMediaMap"/>), through
/// <see cref="WwiseBankWriter"/>, and the new streamed .wem, all to the project (sd files under /sd/, see
/// <see cref="SoundData.ProjectPath"/>). Banks are rewritten from their current bytes (the project's copy if there is
/// one), so earlier saves are kept.
/// </summary>
public class SoundEdits
{
    private readonly SoundData Data;
    private readonly Dictionary<SoundMediaKey, SoundMediaEdit> _edits = new();

    /// <summary>Bumped on every change (lists cache on it).</summary>
    public int Version { get; private set; }

    public bool IsSaving { get; private set; }

    public SoundEdits(SoundData data)
    {
        Data = data;
    }

    public int Count => _edits.Count;

    public IReadOnlyDictionary<SoundMediaKey, SoundMediaEdit> All => _edits;

    public SoundMediaEdit Get(SoundMediaKey key) => _edits.TryGetValue(key, out var e) ? e : null;

    public static SoundMediaKey KeyOf(SoundEntry e) => new(e.MediaId, e.Container?.Language ?? "");

    public SoundMediaEdit Get(SoundEntry e) => e == null ? null : Get(KeyOf(e));

    /// <summary>Sets or clears (null) the replacement of a sound, and refreshes what plays it. Used by the actions.</summary>
    public void Set(SoundMediaKey key, SoundMediaEdit edit)
    {
        if (edit == null) _edits.Remove(key);
        else _edits[key] = edit;
        Version++;
        Refresh(key);

        // the banks to rewrite are known once the map is built
        if (edit != null)
            Data.MediaMap.EnsureBuilt();
    }

    /// <summary>Is this bank touched by an unsaved replacement? (False until the media map is built.)</summary>
    public bool Affects(SoundContainer c)
    {
        if (_edits.Count == 0) return false;
        foreach (var k in _edits.Keys)
            if (Data.MediaMap.BanksFor(k).Contains(c)) return true;
        return false;
    }

    /// <summary>The files a save writes: banks and streamed wems.</summary>
    public List<string> PendingFiles()
    {
        var files = new List<string>();
        foreach (var (k, e) in _edits)
        {
            if (!Data.MediaMap.IsBuilt)
            {
                files.Add(LOC.Get("SOUND_Edits_Pending_Sound", k.ToString(), e.Label));
                continue;
            }
            files.AddRange(Data.MediaMap.BanksFor(k).Select(c => c.Path));
            if (e.StreamPath != null) files.Add(e.StreamPath);
        }
        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// The playback source of an entry, with its replacement if it has one: entries' sources read through this, so an
    /// edit plays before it is saved.
    /// </summary>
    public byte[] ReadFor(SoundMediaKey key, Func<byte[]> original)
        => _edits.TryGetValue(key, out var e) ? e.Wem : original();

    /// <summary>Drops cached decodes and format facts of every loaded entry of this media, so the new audio is used.</summary>
    private void Refresh(SoundMediaKey key)
    {
        foreach (var c in Data.Containers)
        {
            if (c.State != SoundLoadState.Loaded || (c.Language ?? "") != key.Language) continue;
            foreach (var e in c.Entries)
            {
                if (e.MediaId != key.MediaId) continue;
                if (e.Source?.CacheKey != null) VgmstreamCli.Forget(e.Source.CacheKey);
                e.Info = null;
                e.InfoError = null;
                e.InfoRequested = false;

                var wem = _edits.TryGetValue(key, out var ed) ? ed.Wem : null;
                var h = wem != null ? WemHeader.Read(wem) : null;
                if (h != null)
                {
                    e.Codec = h.Codec;
                    e.SampleRate = h.SampleRate;
                    e.Channels = h.Channels;
                    e.Seconds = h.FormatTag == 0xFFFE && h.SampleRate > 0 && h.Channels > 0 ? (double)h.DataSize / (h.SampleRate * h.Channels * 2) : h.Seconds;
                    e.SecondsApprox = false;
                }
            }
        }
    }

    /// <summary>
    /// Saves every replacement (on a worker). Waits for the media map. Returns false if nothing could be saved; the
    /// replacements stay unsaved then, so nothing is lost.
    /// </summary>
    public Task<bool> SaveAll(Action onDone = null)
    {
        if (IsSaving || _edits.Count == 0)
            return Task.FromResult(false);

        IsSaving = true;
        var edits = _edits.ToDictionary(kv => kv.Key, kv => kv.Value);
        return Task.Run(async () =>
        {
            bool ok = false;
            try
            {
                await Data.MediaMap.EnsureBuilt();
                if (!Data.MediaMap.IsBuilt)
                    throw new InvalidOperationException(Data.MediaMap.Error ?? "media map not built");
                ok = Save(edits);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("SOUND_Edits_Save_Failed", e.Message));
            }
            finally
            {
                IsSaving = false;
                onDone?.Invoke();
            }
            return ok;
        });
    }

    private bool Save(Dictionary<SoundMediaKey, SoundMediaEdit> edits)
    {
        // bank -> its replacements (by media ID: a bank has one language)
        var byBank = new Dictionary<SoundContainer, Dictionary<uint, WwiseMediaReplacement>>();
        int wems = 0;
        foreach (var (k, e) in edits)
        {
            int oldStreamed = 0;
            if (e.StreamPath != null)
            {
                oldStreamed = Data.ReadFile(e.StreamPath)?.Length ?? 0;
                Data.WriteProjectFile(e.StreamPath, e.Wem);
                wems++;
            }

            var r = new WwiseMediaReplacement { Wem = e.Wem, PluginId = e.PluginId, OldStreamedSize = oldStreamed };
            foreach (var c in Data.MediaMap.BanksFor(k))
                (byBank.TryGetValue(c, out var d) ? d : byBank[c] = new())[k.MediaId] = r;
        }

        int banks = 0, failed = 0;
        foreach (var (c, repl) in byBank)
        {
            try
            {
                var raw = Data.ReadFile(c.Path) ?? throw new FileNotFoundException(c.Path);
                var result = WwiseBankWriter.Write(raw, repl);
                if (!result.Changed) continue;
                Data.WriteProjectFile(c.Path, result.Bytes);
                banks++;
            }
            catch (Exception ex)
            {
                failed++;
                Smithbox.LogError(this, LOC.Get("SOUND_Edits_Save_Bank_Failed", c.Path, ex.Message));
            }
        }

        if (failed > 0)
            return false;

        // applied on the main thread (the lists read the edits and banks every frame)
        _saved = (edits, byBank.Keys.ToList());
        Smithbox.Log(this, LOC.Get("SOUND_Edits_Saved", edits.Count, banks, wems));
        return true;
    }

    private volatile object _savedBox;
    private (Dictionary<SoundMediaKey, SoundMediaEdit> Edits, List<SoundContainer> Banks) _saved
    {
        set => _savedBox = value;
    }

    /// <summary>
    /// Called each frame: once a save has finished, drops the saved replacements (an edit made during the save stays)
    /// and reloads the rewritten banks. Returns the banks reloaded, or null.
    /// </summary>
    public List<SoundContainer> ApplySaved()
    {
        if (_savedBox is not ValueTuple<Dictionary<SoundMediaKey, SoundMediaEdit>, List<SoundContainer>> done)
            return null;
        _savedBox = null;

        var (edits, banks) = done;
        foreach (var (k, e) in edits)
            if (_edits.TryGetValue(k, out var cur) && ReferenceEquals(cur, e))
                _edits.Remove(k);
        Version++;

        foreach (var k in edits.Keys)
            Refresh(k);
        foreach (var c in banks)
            c.MarkSaved();
        return banks;
    }
}
