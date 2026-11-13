using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Read-only FMOD Designer event project: RIFF FEV ("FEV FMT " 0x00450000: DS2S, BB, DS3, SDT, half of DS2), whose names
/// come from the STRR string table and whose body is the FEV1 data in the LGCY chunk; and bare FEV1 (0x003D0000: DS1,
/// DS1R, the other half of DS2, two BB files), the same body with inline strings (<c>u32 len + chars + NUL</c>), a
/// 0x84-byte event property block and waveforms that name their bank. Only what the Sound Editor needs is read:
/// <list type="bullet">
/// <item>Sound definitions: <c>u32 count</c>, then per definition <c>u32 name (STRR index), u32 property index,
/// u32 waveform count</c>, and per waveform <c>u32 type, u32 weight</c> and for type 0 (wavetable)
/// <c>u32 strlen, path, u32 bank, u32 sample index, u32 length ms</c>.</item>
/// <item>Events: found by their record head <c>u32 8, u32 name (STRR index), 16-byte GUID</c>; a fixed 0xA8-byte
/// property block, then <c>u32 layer count</c> and the first layer: <c>u16 flags, s16 priority, u16 parameter,
/// u16 sound instance count, u16 envelope count</c>, then the first sound instance's <c>u16 definition index</c>.
/// Later layers / instances aren't walked yet (envelope layout unknown), so an event lists its first definition.</item>
/// </list>
/// Checked by SoundSurvey fevcheck: every waveform's file name must be the FSB sample at its index.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public sealed class FevFile
{
    public sealed class Waveform
    {
        public int Type;
        public int Weight;
        public string Path;
        /// <summary>RIFF: index into <see cref="BankNames"/>; FEV1: -1 (the bank is named inline).</summary>
        public int Bank;

        /// <summary>The FSB stem holding the sample, or null if the index is out of the header's list.</summary>
        public string BankName;
        public int Index;
        public int LengthMs;
        public string FileStem => System.IO.Path.GetFileNameWithoutExtension((Path ?? "").Replace('\\', '/'));
    }

    public sealed class SoundDef
    {
        public int Index;
        public string Name;
        public int PropertyIndex;
        public List<Waveform> Waveforms = new();
    }

    public sealed class Event
    {
        public string Name;
        public int Type;
        public int Offset;
        public int LayerCount;

        /// <summary>The first layer's first sound instance's definition, or -1 if the layer header didn't check out.</summary>
        public int FirstDefIndex = -1;
        public int FirstLayerInstances;

        /// <summary>The definitions the event's layers play (first sound instance of each layer found).</summary>
        public List<int> DefIndices = new();

        /// <summary>Layer headers found; equals <see cref="LayerCount"/> when every layer was located.</summary>
        public int LayersFound;

        /// <summary>A layer has more than one sound instance: only its first is read (instance size unknown).</summary>
        public bool Partial;

        public bool Complete => LayerCount > 0 && LayersFound == LayerCount && !Partial;
    }

    public List<string> Strings = new();
    public List<SoundDef> SoundDefs = new();
    public List<Event> Events = new();
    public string Error;

    /// <summary>Bare FEV1 (DS1/DS1R, half of DS2, two BB files): strings inline instead of in STRR.</summary>
    public bool Legacy;

    /// <summary>
    /// The project's wave banks (FSB stems) from the header: <c>u32 0x200, u32, 8-byte hash, [u32 0 in RIFF], u32 len +
    /// name</c> each. One per bank file, except DS2S's shared projects (frpg2_c1 lists frpg2_c1000, frpg2_c1010…).
    /// </summary>
    public List<string> BankNames = new();

    private static readonly Regex EventNamePattern = new(@"^[a-z]\d{9,10}$", RegexOptions.Compiled);

    /// <summary>FEV1 event property block: from the end of the GUID to <c>u32 layer count</c> (0x90 in RIFF's LGCY).</summary>
    private const int LegacyEventBlock = 0x84;

    public static FevFile Read(byte[] d)
    {
        var f = new FevFile();
        if (d.Length >= 8 && Encoding.ASCII.GetString(d, 0, 4) == "FEV1")
        {
            f.Legacy = true;
            f.ReadBanks(d, 8, d.Length);
            try { f.ReadLegacySoundDefs(d, 8, d.Length); }
            catch (Exception e) { f.Error = $"Sound definitions: {e.Message}"; }
            f.ReadLegacyEvents(d, 8, d.Length);
            return f;
        }

        if (d.Length < 12 || Encoding.ASCII.GetString(d, 0, 4) != "RIFF" || Encoding.ASCII.GetString(d, 8, 4) != "FEV ")
        {
            f.Error = "Not an FMOD event project (RIFF FEV or FEV1)";
            return f;
        }

        int strr = -1, strrSize = 0, lgcy = -1, lgcySize = 0;
        void Walk(int p, int end)
        {
            while (p + 8 <= end)
            {
                var id = Encoding.ASCII.GetString(d, p, 4);
                int s = BitConverter.ToInt32(d, p + 4);
                if (s < 0 || p + 8 + s > end) return;
                if (id == "LIST") Walk(p + 12, p + 8 + s);
                else if (id == "STRR") { strr = p + 8; strrSize = s; }
                else if (id == "LGCY") { lgcy = p + 8; lgcySize = s; }
                p += 8 + s + (s & 1);
            }
        }
        Walk(12, d.Length);

        if (strr < 0 || lgcy < 0)
        {
            f.Error = "No STRR or LGCY chunk";
            return f;
        }

        int count = BitConverter.ToInt32(d, strr);
        for (int i = 0; i < count; i++)
        {
            int start = strr + 4 + count * 4 + BitConverter.ToInt32(d, strr + 4 + i * 4), e = start;
            while (e < strr + strrSize && d[e] != 0) e++;
            f.Strings.Add(Encoding.ASCII.GetString(d, start, e - start));
        }

        f.ReadBanks(d, lgcy, lgcy + lgcySize);

        try { f.ReadSoundDefs(d, lgcy, lgcySize); }
        catch (Exception e) { f.Error = $"Sound definitions: {e.Message}"; }

        // RIFF waveforms give the bank as an index into the header's list
        foreach (var w in f.SoundDefs.SelectMany(s => s.Waveforms))
            w.BankName = w.Bank >= 0 && w.Bank < f.BankNames.Count ? f.BankNames[w.Bank] : null;

        f.ReadEvents(d, lgcy, lgcySize);
        return f;
    }

    /// <summary>Only the header's wave bank list (cheap: no definitions or events), for cataloguing a game's projects.</summary>
    public static List<string> ReadBankNames(byte[] d)
    {
        var f = new FevFile();
        if (d.Length >= 8 && Encoding.ASCII.GetString(d, 0, 4) == "FEV1")
            f.ReadBanks(d, 8, d.Length);
        else if (d.Length >= 12 && Encoding.ASCII.GetString(d, 0, 4) == "RIFF")
        {
            // LGCY sits inside LIST/PROJ near the start; its bank list is in the first few hundred bytes
            int at = d.AsSpan(0, Math.Min(d.Length, 0x4000)).IndexOf("LGCY"u8);
            if (at >= 0 && at + 8 <= d.Length)
                f.ReadBanks(d, at + 8, Math.Min(d.Length, at + 8 + BitConverter.ToInt32(d, at + 4)));
        }
        return f.BankNames;
    }

    /// <summary>The event names alone (for the sound index), without linking anything.</summary>
    public IEnumerable<string> EventNames() => Events.Select(e => e.Name);

    /// <summary>
    /// The events that play something in <paramref name="bank"/> (an FSB stem), for a project shared by several banks.
    /// An event none of whose waveforms could be read is kept out.
    /// </summary>
    public List<Event> EventsForBank(string bank)
        => Events.Where(e => e.DefIndices.Any(i => i >= 0 && i < SoundDefs.Count &&
                SoundDefs[i].Waveforms.Any(w => string.Equals(w.BankName, bank, StringComparison.OrdinalIgnoreCase)))).ToList();

    private void ReadBanks(byte[] d, int start, int end)
    {
        int limit = Math.Min(end, start + 0x400);
        for (int q = start; q + 24 <= limit; q++)
        {
            if (!TryBank(d, q, end, out var name, out var next))
                continue;
            BankNames.Add(name);
            while (TryBank(d, next, end, out name, out next))
                BankNames.Add(name);
            return;
        }
    }

    /// <summary>
    /// One bank record: <c>u32 mode</c> (a single flag bit: 0x80, 0x100, 0x200 seen), <c>u32 small (10)</c>, then
    /// 8-byte hashes, each followed by a u32 in RIFF (BB's language banks repeat <c>hash, u32</c> per language), then
    /// <c>u32 len + name</c>.
    /// </summary>
    private static bool TryBank(byte[] d, int q, int end, out string name, out int next)
    {
        name = null;
        next = q;
        if (q + 24 > end)
            return false;
        uint mode = BitConverter.ToUInt32(d, q);
        if (mode is < 0x10 or > 0x10000 || (mode & (mode - 1)) != 0 || BitConverter.ToUInt32(d, q + 4) is 0 or > 64)
            return false;

        int p = q + 8;
        for (int k = 0; k < 32 && p + 12 <= end; k++)
        {
            p += 8;                                                   // hash
            if (TryInlineString(d, p, end, out name, out next) && IsBankName(name)) return true;
            if (TryInlineString(d, p + 4, end, out name, out next) && IsBankName(name)) return true;
            p += 4;                                                   // RIFF: the word after the hash
        }
        name = null;
        return false;
    }

    private static bool IsBankName(string s)
    {
        foreach (var ch in s)
            if (!(char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-')) return false;
        return s.Length >= 2;
    }

    /// <summary><c>u32 len (with the NUL), chars, NUL</c>, printable.</summary>
    private static bool TryInlineString(byte[] d, int p, int end, out string s, out int next)
    {
        s = null;
        next = p;
        if (p + 4 > end) return false;
        int len = BitConverter.ToInt32(d, p);
        if (len is < 2 or > 1024 || p + 4 + len > end || d[p + 4 + len - 1] != 0 || !IsPath(d, p + 4, len - 1))
            return false;
        s = Encoding.ASCII.GetString(d, p + 4, len - 1);
        next = p + 4 + len;
        return true;
    }

    private static string ReadInlineString(byte[] d, ref int p, int end, string what)
    {
        if (!TryInlineString(d, p, end, out var s, out var next))
            throw new FormatException($"bad {what} string at 0x{p:X}");
        p = next;
        return s;
    }

    /// <summary>
    /// FEV1 sound definitions: <c>u32 count</c>, then per definition <c>u32 len + name, u32 property index, u32 waveform
    /// count</c>, and per waveform <c>u32 type (0), u32 weight, u32 len + path, u32 len + bank name, u32 sample index,
    /// u32 length ms</c>. Found by shape and accepted only if every definition parses.
    /// </summary>
    private void ReadLegacySoundDefs(byte[] d, int start, int end)
    {
        for (int q = start; q + 32 <= end; q++)
        {
            int count = BitConverter.ToInt32(d, q);
            if (count is < 1 or > 200000) continue;
            if (!TryInlineString(d, q + 4, end, out var first, out var p) || first[0] != '/') continue;
            if (p + 12 > end || BitConverter.ToInt32(d, p + 4) is < 0 or > 4096) continue;

            var defs = new List<SoundDef>();
            try
            {
                int r = q + 4;
                for (int k = 0; k < count; k++)
                {
                    var def = new SoundDef { Index = k, Name = ReadInlineString(d, ref r, end, "definition") };
                    def.PropertyIndex = BitConverter.ToInt32(d, r);
                    int n = BitConverter.ToInt32(d, r + 4);
                    r += 8;
                    if (n is < 0 or > 4096) throw new FormatException($"{n} waveforms in {def.Name}");
                    for (int w = 0; w < n; w++)
                    {
                        var wave = new Waveform { Type = BitConverter.ToInt32(d, r), Weight = BitConverter.ToInt32(d, r + 4), Bank = -1 };
                        r += 8;
                        if (wave.Type != 0) throw new FormatException($"waveform type {wave.Type} in {def.Name}");
                        wave.Path = ReadInlineString(d, ref r, end, "waveform path");
                        wave.BankName = ReadInlineString(d, ref r, end, "waveform bank");
                        wave.Index = BitConverter.ToInt32(d, r);
                        wave.LengthMs = BitConverter.ToInt32(d, r + 4);
                        r += 8;
                        def.Waveforms.Add(wave);
                    }
                    if (r > end) throw new FormatException("ran past the end");
                    defs.Add(def);
                }
                SoundDefs = defs;
                return;
            }
            catch (FormatException) { }
            catch (ArgumentOutOfRangeException) { }
        }
        throw new FormatException("table not found");
    }

    /// <summary>
    /// FEV1 events: head <c>u32 8, u32 len + name, 16-byte GUID</c>, a 0x84-byte property block, <c>u32 layer count</c>,
    /// then layers as in RIFF. Heads are accepted when the name is sound-ID shaped, or when the layer header checks out.
    /// </summary>
    private void ReadLegacyEvents(byte[] d, int start, int end)
    {
        var heads = new List<(int Q, string Name, bool Layer)>();
        for (int q = start; q + 8 <= end; q++)
        {
            if (BitConverter.ToInt32(d, q) != 8 || !TryInlineString(d, q + 4, end, out var name, out var guid))
                continue;
            if (name.Contains('/') || guid + 16 + LegacyEventBlock + 24 > end)
                continue;
            int lc = guid + 16 + LegacyEventBlock;
            bool layer = BitConverter.ToInt32(d, lc) is >= 1 and <= 64 && IsLayerHeader(d, lc + 4, end, out _, out _);
            if (!layer && !EventNamePattern.IsMatch(name))
                continue;
            heads.Add((q, name, layer));
            q = lc - 1;
        }

        for (int k = 0; k < heads.Count; k++)
        {
            var (q, name, layer) = heads[k];
            int next = k + 1 < heads.Count ? heads[k + 1].Q : end;
            int lc = q + 4 + 4 + Encoding.ASCII.GetByteCount(name) + 1 + 16 + LegacyEventBlock;
            var ev = new Event { Type = 8, Name = name, Offset = q };
            if (layer)
                ReadLayers(d, ev, BitConverter.ToInt32(d, lc), lc + 4, next);
            Events.Add(ev);
        }
    }

    /// <summary>The first layer at <paramref name="l"/>; later ones by their header signature before <paramref name="next"/>.</summary>
    private void ReadLayers(byte[] d, Event ev, int layers, int l, int next)
    {
        if (layers is < 1 or > 64 || !IsLayerHeader(d, l, next, out var instances, out var def))
            return;

        ev.LayerCount = layers;
        ev.FirstLayerInstances = instances;
        ev.FirstDefIndex = def;
        ev.DefIndices.Add(def);
        ev.LayersFound = 1;
        if (instances > 1) ev.Partial = true;

        // later layers follow variable-size envelopes: find them by their header signature within the event
        for (int x = l + 20; x + 20 <= next && ev.LayersFound < layers; x++)
        {
            if (!IsLayerHeader(d, x, next, out var n2, out var def2, strictVolume: true)) continue;
            ev.LayersFound++;
            if (!ev.DefIndices.Contains(def2)) ev.DefIndices.Add(def2);
            if (n2 > 1) ev.Partial = true;
            x += 19;
        }
    }

    private void ReadSoundDefs(byte[] d, int lgcy, int size)
    {
        // The table: u32 count, then definitions. Found by its shape (count, a definition whose first waveform is a
        // wavetable with a NUL-terminated path), and accepted only if every definition then parses.
        int end = lgcy + size;
        for (int q = lgcy; q + 32 <= end; q++)
        {
            int count = BitConverter.ToInt32(d, q);
            if (count is < 1 or > 200000) continue;
            if (BitConverter.ToInt32(d, q + 4) is var name && (name < 0 || name >= Strings.Count)) continue;
            if (BitConverter.ToInt32(d, q + 12) is < 1 or > 4096) continue;            // waveforms of the first definition
            if (BitConverter.ToInt32(d, q + 16) != 0) continue;                        // wavetable
            if (BitConverter.ToInt32(d, q + 20) is < 0 or > 100000) continue;          // weight (100 usually, 2000 seen in DS3 fdp_c2110)
            int sl = BitConverter.ToInt32(d, q + 24);
            if (sl is < 2 or > 1024 || q + 28 + sl > end || d[q + 28 + sl - 1] != 0) continue;
            if (!IsPath(d, q + 28, sl - 1)) continue;

            var defs = new List<SoundDef>();
            if (TryReadSoundDefs(d, q + 4, end, count, defs))
            {
                SoundDefs = defs;
                return;
            }
        }
        throw new FormatException("table not found");
    }

    private static bool IsPath(byte[] d, int p, int n)
    {
        for (int i = 0; i < n; i++)
            if (d[p + i] < 0x20 || d[p + i] >= 0x7F) return false;
        return true;
    }

    private bool TryReadSoundDefs(byte[] d, int p, int end, int count, List<SoundDef> defs)
    {
        try
        {
            ReadSoundDefsAt(d, p, end, count, defs);
            return true;
        }
        catch
        {
            defs.Clear();
            return false;
        }
    }

    private void ReadSoundDefsAt(byte[] d, int p, int end, int count, List<SoundDef> defs)
    {
        for (int k = 0; k < count; k++)
        {
            var def = new SoundDef { Index = k, PropertyIndex = BitConverter.ToInt32(d, p + 4) };
            int name = BitConverter.ToInt32(d, p), n = BitConverter.ToInt32(d, p + 8);
            if (name < 0 || name >= Strings.Count || n < 0 || n > 4096)
                throw new FormatException($"definition {k}: name {name}, {n} waveforms");
            def.Name = Strings[name];
            p += 12;
            for (int w = 0; w < n; w++)
            {
                var wave = new Waveform { Type = BitConverter.ToInt32(d, p), Weight = BitConverter.ToInt32(d, p + 4) };
                p += 8;
                if (wave.Type != 0)
                    throw new FormatException($"waveform type {wave.Type} in {def.Name}");
                int sl = BitConverter.ToInt32(d, p);
                p += 4;
                if (sl < 1 || sl > 1024 || p + sl > end || d[p + sl - 1] != 0)
                    throw new FormatException($"waveform path length {sl} in {def.Name}");
                wave.Path = Encoding.ASCII.GetString(d, p, Math.Max(0, sl - 1));
                p += sl;
                wave.Bank = BitConverter.ToInt32(d, p);
                wave.Index = BitConverter.ToInt32(d, p + 4);
                wave.LengthMs = BitConverter.ToInt32(d, p + 8);
                p += 12;
                def.Waveforms.Add(wave);
            }
            defs.Add(def);
            if (p > end)
                throw new FormatException("ran past LGCY");
        }
    }

    private void ReadEvents(byte[] d, int lgcy, int size)
    {
        var evNames = new HashSet<int>();
        for (int i = 0; i < Strings.Count; i++)
            if (EventNamePattern.IsMatch(Strings[i])) evNames.Add(i);

        // event record heads: u32 8, u32 name (STRR index), 16-byte GUID
        int end = lgcy + size;
        var starts = new List<int>();
        for (int q = lgcy; q + 0xC0 <= end; q++)
        {
            if (BitConverter.ToInt32(d, q) != 8 || !evNames.Contains(BitConverter.ToInt32(d, q + 4)))
                continue;
            starts.Add(q);
            q += 0x10;
        }

        for (int k = 0; k < starts.Count; k++)
        {
            int q = starts[k], next = k + 1 < starts.Count ? starts[k + 1] : end;
            var ev = new Event { Type = 8, Name = Strings[BitConverter.ToInt32(d, q + 4)], Offset = q - lgcy };
            // the first layer sits right after the 0xA8 property block (head included)
            ReadLayers(d, ev, BitConverter.ToInt32(d, q + 0xA8), q + 0xAC, next);
            Events.Add(ev);
        }
    }

    /// <summary>
    /// A layer header plus the start of its first sound instance: u16 flags (&lt; 0x20), s16 priority (-1..256), u16 parameter,
    /// u16 instance count (1..256), u16 envelope count (&lt;= 64), then the instance: u16 definition index, f32 start 0.0,
    /// f32 volume 0..1 (usually 1.0; 0.99 etc. on about 15% of DS1 events and 2–4% of RIFF ones).
    /// </summary>
    private bool IsLayerHeader(byte[] d, int x, int limit, out int instances, out int def, bool strictVolume = false)
    {
        instances = def = -1;
        if (x + 20 > limit)
            return false;
        ushort flags = BitConverter.ToUInt16(d, x);
        short priority = BitConverter.ToInt16(d, x + 2);
        ushort n = BitConverter.ToUInt16(d, x + 6), env = BitConverter.ToUInt16(d, x + 8), di = BitConverter.ToUInt16(d, x + 10);
        if (flags >= 0x20 || priority < -1 || priority > 256 || n is < 1 or > 256 || env > 64 || di >= SoundDefs.Count)
            return false;
        if (BitConverter.ToUInt32(d, x + 12) != 0)
            return false;
        // a layer at a known place may have any volume; a layer found by scanning must have 1.0, or envelope data passes
        float volume = BitConverter.ToSingle(d, x + 16);
        if (strictVolume ? volume != 1f : !(volume >= 0f && volume <= 1f))
            return false;
        instances = n;
        def = di;
        return true;
    }
}
