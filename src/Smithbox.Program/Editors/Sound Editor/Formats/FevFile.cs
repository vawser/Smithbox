using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Read-only FMOD Designer event project (RIFF FEV, "FEV FMT " 0x00450000: DS2S, BB, DS3, SDT). Names come from the
/// STRR string table; the event body is the legacy FEV1 data in the LGCY chunk. Only what the Sound Editor needs is read:
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
        public int Bank;
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

    private static readonly Regex EventNamePattern = new(@"^[a-z]\d{9,10}$", RegexOptions.Compiled);

    public static FevFile Read(byte[] d)
    {
        var f = new FevFile();
        if (d.Length < 12 || Encoding.ASCII.GetString(d, 0, 4) != "RIFF" || Encoding.ASCII.GetString(d, 8, 4) != "FEV ")
        {
            f.Error = "Not a RIFF FEV (legacy FEV1 isn't read yet)";
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

        try { f.ReadSoundDefs(d, lgcy, lgcySize); }
        catch (Exception e) { f.Error = $"Sound definitions: {e.Message}"; }

        f.ReadEvents(d, lgcy, lgcySize);
        return f;
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
            int layers = BitConverter.ToInt32(d, q + 0xA8);
            int l = q + 0xAC;

            // the first layer sits right after the 0xA8 property block
            if (layers is >= 1 and <= 64 && IsLayerHeader(d, l, next, out var instances, out var def))
            {
                ev.LayerCount = layers;
                ev.FirstLayerInstances = instances;
                ev.FirstDefIndex = def;
                ev.DefIndices.Add(def);
                ev.LayersFound = 1;
                if (instances > 1) ev.Partial = true;

                // later layers follow variable-size envelopes: find them by their header signature within the event
                for (int x = l + 20; x + 20 <= next && ev.LayersFound < layers; x++)
                {
                    if (!IsLayerHeader(d, x, next, out var n2, out var def2)) continue;
                    ev.LayersFound++;
                    if (!ev.DefIndices.Contains(def2)) ev.DefIndices.Add(def2);
                    if (n2 > 1) ev.Partial = true;
                    x += 19;
                }
            }
            Events.Add(ev);
        }
    }

    /// <summary>
    /// A layer header plus the start of its first sound instance: u16 flags (&lt; 0x20), s16 priority (-1..256), u16 parameter,
    /// u16 instance count (1..256), u16 envelope count (&lt;= 64), then the instance: u16 definition index, f32 0.0, f32 1.0.
    /// </summary>
    private bool IsLayerHeader(byte[] d, int x, int limit, out int instances, out int def)
    {
        instances = def = -1;
        if (x + 20 > limit)
            return false;
        ushort flags = BitConverter.ToUInt16(d, x);
        short priority = BitConverter.ToInt16(d, x + 2);
        ushort n = BitConverter.ToUInt16(d, x + 6), env = BitConverter.ToUInt16(d, x + 8), di = BitConverter.ToUInt16(d, x + 10);
        if (flags >= 0x20 || priority < -1 || priority > 256 || n is < 1 or > 256 || env > 64 || di >= SoundDefs.Count)
            return false;
        if (BitConverter.ToUInt32(d, x + 12) != 0 || BitConverter.ToUInt32(d, x + 16) != 0x3F800000)
            return false;
        instances = n;
        def = di;
        return true;
    }
}
