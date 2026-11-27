using SoulsFormats;
using StudioCore.Application;
using StudioCore.Memory;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The Gparam Reloader for DS2: writes edited values into the live values the renderer uses.
///
/// DS2 converts the fltparam at map load and never reads the file again. Blend objects fill a block of current values,
/// and the game registers each filter field for its debug binding as a MetaInfoInit&lt;T&gt; object
/// (+00 vtable, +08 pointer to the live value, +20 an inline Shift-JIS string). The string names the NEXT registration's
/// value: the group name labels the group's first field, and each field's key labels the field after it. Every group
/// is registered twice (two value sets the game blends between).
///
/// A push writes a changed value where the live value still equals the old value of the same field and ID, so values of
/// IDs that aren't active, and fields the game doesn't register (about half: SSAO, most directional light and shadow
/// fields), are left alone. Values revert when the map reloads (the file on disk is read again).
/// See Claude/Documentation/Gparam.md (Gparam Reloader) §7–8.
/// </summary>
public class GparamLiveReloader
{
    private readonly ProjectEntry Project;

    public GparamLiveReloader(ProjectEntry project)
    {
        Project = project;
    }

    public GameLink Link => Project.Handler.GameLink;

    private GparamReloaderOffsets OffsetsCache;
    private GparamReloaderOffsets Offsets => OffsetsCache ??= GparamReloaderOffsets.Load(Project);

    /// <summary>Whether this game has a game link and a live group</summary>
    public bool IsSupported => Link.IsSupported && Offsets.Groups.Any(g => g.IsLive);

    /// <param name="Registered">fields of the group the game registered</param>
    public record InGameGroup(string Name, int Registered, int Fields);

    public enum ReloadOutcome { Reloaded, NotFound, Failed }

    public record ReloadResult(string Name, ReloadOutcome Outcome, string Message, DateTime At);

    public volatile IReadOnlyList<InGameGroup> Groups = Array.Empty<InGameGroup>();
    public volatile int RegistrationCount;
    public volatile bool IsScanning;
    public volatile bool IsReloading;
    public volatile ReloadResult LastReload;
    public DateTime? LastScan { get; private set; }

    public GameLink.LinkState State => Link.State;
    public string StateMessage => Link.StateMessage;

    /// <summary>One MetaInfoInit: its live value, the size of T, and the string it holds (the next one's label)</summary>
    private record Registration(nint Value, int Size, string Text);

    // The registrations found for one attach of the link, in memory order
    private int ScannedFor = -1;
    private List<Registration> Registrations = new();

    // What this session last pushed of each file: the game holds these values now, not the file on disk
    private readonly Dictionary<string, byte[]> Pushed = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Encoding ShiftJis = CreateShiftJis();

    private static Encoding CreateShiftJis()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(932);
    }

    public static string GameNameOf(string key) => GparamReloader.GameNameOf(key);

    public void Detach()
    {
        Link.Detach();

        lock (Link.Lock)
            Forget();
    }

    private void Forget()
    {
        ScannedFor = -1;
        Registrations = new();
        Groups = Array.Empty<InGameGroup>();
        RegistrationCount = 0;
    }

    #region Scan

    /// <summary>
    /// Finds the registrations on a background task, and how many fields of each group of this GPARAM they cover.
    /// Reads the whole heap; run on request only.
    /// </summary>
    public void RequestScan(byte[] reference)
    {
        if (IsScanning || IsReloading)
            return;

        IsScanning = true;

        Task.Run(() =>
        {
            try
            {
                lock (Link.Lock)
                {
                    if (!Link.EnsureAttached())
                    {
                        Forget();
                        return;
                    }

                    Scan();
                    Summarise(reference);
                }
            }
            catch (Exception e)
            {
                lock (Link.Lock)
                {
                    Link.Fail(LOC.Get("GPARAM_Reloader_State_Read_Failed", e.Message));
                    Forget();
                }
            }
            finally
            {
                LastScan = DateTime.Now;
                IsScanning = false;
            }
        });
    }

    private void Scan()
    {
        var watch = Stopwatch.StartNew();
        var vtables = FindVtables();

        var objects = new List<(nint Obj, int Size)>();
        var scanned = Link.ForEachHeapChunk(0, (start, buffer, read) =>
        {
            var limit = Math.Min(read, GameLink.HeapChunk) & ~7;
            for (var o = 0; o < limit; o += 8)
            {
                if (vtables.TryGetValue(BitConverter.ToInt64(buffer, o), out var size))
                    objects.Add((start + o, size));
            }
        });

        var registrations = new List<Registration>();
        foreach (var (obj, size) in objects.OrderBy(o => o.Obj))
        {
            byte[] body;
            try
            {
                body = Link.ReadBytes(obj, 0x60);
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            var end = Array.IndexOf(body, (byte)0, 0x20);
            if (end <= 0x20)
                continue;

            registrations.Add(new Registration((nint)BitConverter.ToInt64(body, 8), size, ShiftJis.GetString(body, 0x20, end - 0x20)));
        }

        Registrations = registrations;
        RegistrationCount = registrations.Count;
        ScannedFor = Link.AttachId;

        // TEMP diagnostics for the first in-game checks (2026-11-18)
        Smithbox.Log(typeof(GparamLiveReloader),
            $"[Gparam Editor] Live scan: {vtables.Count} MetaInfoInit vtable(s), {registrations.Count} registration(s), " +
            $"{scanned / (1024 * 1024)} MB in {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// The MetaInfoInit&lt;T&gt; vtables and sizeof(T), from the exe's RTTI: a vtable follows a pointer to a
    /// CompleteObjectLocator (signature 1) whose type descriptor is named ".?AV?$MetaInfoInit@...".
    /// </summary>
    private Dictionary<long, int> FindVtables()
    {
        var moduleBase = Link.ModuleBase;
        var image = new byte[Link.ModuleSize];

        for (var o = 0; o < image.Length; o += 0x100000)
        {
            try
            {
                Link.ReadBytes(moduleBase + o, Math.Min(0x100000, image.Length - o)).CopyTo(image, o);
            }
            catch (InvalidOperationException)
            {
                // An unreadable page: left as zeros
            }
        }

        var vtables = new Dictionary<long, int>();

        for (var o = 8; o + 8 <= image.Length; o += 8)
        {
            var col = BitConverter.ToInt64(image, o - 8) - moduleBase;
            if (col <= 0 || col + 0x18 > image.Length || BitConverter.ToInt32(image, (int)col) != 1)
                continue;

            var td = BitConverter.ToInt32(image, (int)col + 0x0C);
            if (td <= 0 || td + 0x50 > image.Length)
                continue;

            var name = Encoding.ASCII.GetString(image, td + 0x10, 0x40);
            if (!name.StartsWith(".?AV?$MetaInfoInit@"))
                continue;

            vtables[(long)moduleBase + o] = SizeOf(name);
        }

        return vtables;
    }

    /// <summary>sizeof(T) from the mangled MetaInfoInit&lt;T&gt; name</summary>
    private static int SizeOf(string mangled)
    {
        var arg = mangled[".?AV?$MetaInfoInit@".Length..];

        if (arg.StartsWith("UDL_VECTOR4")) return 16;
        if (arg.StartsWith("UDL_VECTOR3")) return 12;
        if (arg.StartsWith("UDL_VECTOR2")) return 8;
        if (arg.StartsWith("UDL_COLOR_32")) return 4;
        if (arg.StartsWith("_N")) return 1;             // bool

        return arg[0] switch
        {
            'C' or 'D' or 'E' => 1,                     // char types
            'F' or 'G' => 2,                            // short, unsigned short
            'N' => 8,                                   // double
            _ => 4,                                     // int, unsigned int, float, ...
        };
    }

    /// <summary>
    /// The live values of each field of this GPARAM: for every block the group is registered in, the registration after
    /// the one holding the field's label (the group name for the first field).
    /// </summary>
    private Dictionary<(string Group, string Field), List<Registration>> Pair(GPARAM gparam)
    {
        var map = new Dictionary<(string, string), List<Registration>>();
        var groupKeys = gparam.Params.Select(p => p.Key).ToHashSet();
        var regs = Registrations;

        foreach (var group in gparam.Params)
        {
            if (group.Fields.Count == 0)
                continue;

            var fieldKeys = group.Fields.Select(f => f.Key).ToHashSet();

            for (var h = 0; h < regs.Count; h++)
            {
                if (regs[h].Text != group.Key)
                    continue;

                // The group's registrations run to the next group's header; that header's value is this group's last field
                var end = h + 1;
                while (end < regs.Count && !groupKeys.Contains(regs[end].Text))
                    end++;

                for (var j = h + 1; j <= end && j < regs.Count; j++)
                {
                    var label = j == h + 1 ? group.Fields[0].Key : regs[j - 1].Text;
                    if (!fieldKeys.Contains(label))
                        continue;

                    if (!map.TryGetValue((group.Key, label), out var list))
                        map[(group.Key, label)] = list = new();

                    list.Add(regs[j]);
                }
            }
        }

        return map;
    }

    private void Summarise(byte[] reference)
    {
        if (reference == null)
        {
            Groups = Array.Empty<InGameGroup>();
            return;
        }

        var gparam = GPARAM.Read(reference);
        var map = Pair(gparam);

        Groups = gparam.Params
            .Select(p => new InGameGroup(p.Name ?? p.Key, p.Fields.Count(f => map.ContainsKey((p.Key, f.Key))), p.Fields.Count))
            .ToList();
    }

    #endregion

    #region Reload

    /// <summary>Writes the edited values of a file into the game's live values, on a background task</summary>
    public void RequestReload(string gameName, byte[] bytes)
    {
        if (IsReloading || IsScanning)
        {
            Smithbox.LogError(typeof(GparamLiveReloader), LOC.Get("GPARAM_Reloader_Busy", gameName));
            return;
        }

        IsReloading = true;
        var operation = Link.BeginOperation();

        Task.Run(() =>
        {
            try
            {
                lock (Link.Lock)
                {
                    if (!Link.EnsureAttached())
                    {
                        Forget();
                        LastReload = new(gameName, ReloadOutcome.Failed, StateMessage, DateTime.Now);
                        return;
                    }

                    if (ScannedFor != Link.AttachId || Registrations.Count == 0)
                    {
                        Scan();
                        Summarise(bytes);
                    }

                    LastReload = Reload(gameName, bytes);
                }
            }
            catch (Exception e)
            {
                LastReload = new(gameName, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(GparamLiveReloader), $"[Gparam Editor] Live push of {gameName} failed.", e);
            }
            finally
            {
                operation.Dispose();
                IsReloading = false;
            }
        });
    }

    private ReloadResult Reload(string gameName, byte[] bytes)
    {
        if (Registrations.Count == 0)
            return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Live_No_Registrations"), DateTime.Now);

        var edited = GPARAM.Read(bytes);
        var map = Pair(edited);

        if (map.Count == 0)
            return new(gameName, ReloadOutcome.NotFound, LOC.Get("GPARAM_Reloader_Live_No_Fields", gameName), DateTime.Now);

        // What the game holds now: of what was pushed this session, the project's and vanilla's copy, the one whose
        // values agree with the most live values
        var (previous, source, agree) = Candidates(gameName)
            .Select(c => (Gparam: c.Gparam, c.Source, Agree: Agreement(c.Gparam, map)))
            .OrderByDescending(c => c.Agree)
            .FirstOrDefault();

        if (previous == null || agree == 0)
            return new(gameName, ReloadOutcome.NotFound, LOC.Get("GPARAM_Reloader_Live_Not_Active", gameName), DateTime.Now);

        var changed = 0;
        var changedLive = 0;
        var written = 0;
        var notLive = new HashSet<string>();

        // DS2 repeats group and field keys, and value IDs, within one file: the n-th of each pairs with the n-th
        // (slot keys; Claude/Documentation/Gparam.md §7.1)
        var groupSlots = GparamMerge.SlotKeys(edited.Params, p => p?.Key);

        for (var gi = 0; gi < edited.Params.Count; gi++)
        {
            var group = edited.Params[gi];
            var oldGroup = GparamMerge.FindGroup(previous, groupSlots[gi]);
            if (group == null || oldGroup == null)
                continue;

            var fieldSlots = GparamMerge.SlotKeys(group.Fields, f => f?.Key);

            for (var fi = 0; fi < group.Fields.Count; fi++)
            {
                var field = group.Fields[fi];
                var oldField = GparamMerge.FindField(oldGroup, fieldSlots[fi]);
                if (field == null || oldField == null)
                    continue;

                map.TryGetValue((group.Key, field.Key), out var registrations);

                var occurrence = new Dictionary<int, int>();

                foreach (var value in field.Values)
                {
                    var nth = occurrence.GetValueOrDefault(value.ID);
                    occurrence[value.ID] = nth + 1;

                    var oldValue = GparamMerge.KeyframesOf(oldField, value.ID).ElementAtOrDefault(nth);
                    var newBytes = Encode(value.Value);
                    var oldBytes = oldValue == null ? null : Encode(oldValue.Value);

                    if (newBytes == null || oldBytes == null || newBytes.Length != oldBytes.Length || newBytes.AsSpan().SequenceEqual(oldBytes))
                        continue;

                    changed++;

                    if (registrations == null)
                    {
                        notLive.Add(field.Key);
                        continue;
                    }

                    changedLive++;

                    // Only where the live value is still this ID's old value: the active ID, not mid-blend
                    foreach (var registration in registrations)
                    {
                        var n = Math.Min(registration.Size, oldBytes.Length);
                        var live = Link.ReadBytes(registration.Value, n);

                        if (!live.AsSpan().SequenceEqual(oldBytes.AsSpan(0, n)))
                            continue;

                        Link.WriteBytes(registration.Value, newBytes.AsSpan(0, n).ToArray());
                        written++;
                    }
                }
            }
        }

        Pushed[gameName] = bytes;

        // TEMP diagnostics (2026-11-18)
        Smithbox.Log(typeof(GparamLiveReloader),
            $"[Gparam Editor] Live push {gameName}: previous = {source} ({agree} live value(s) agree), {changed} changed value(s), " +
            $"{changedLive} in registered fields, {written} live value(s) written" +
            (notLive.Count > 0 ? $"; not registered: {string.Join(", ", notLive.Take(10))}{(notLive.Count > 10 ? ", ..." : "")}" : ""));

        if (changed == 0)
            return new(gameName, ReloadOutcome.Reloaded, LOC.Get("GPARAM_Reloader_Patch_Unchanged", gameName), DateTime.Now);

        if (written == 0)
            return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Live_Nothing_Written", gameName, changed, changedLive), DateTime.Now);

        return new(gameName, ReloadOutcome.Reloaded, LOC.Get("GPARAM_Reloader_Live_Done", gameName, written, changed - changedLive), DateTime.Now);
    }

    /// <summary>How many registered fields have a live value equal to one of this GPARAM's values for that field</summary>
    private int Agreement(GPARAM gparam, Dictionary<(string Group, string Field), List<Registration>> map)
    {
        var agree = 0;

        foreach (var ((groupKey, fieldKey), registrations) in map)
        {
            var field = gparam.Params.FirstOrDefault(p => p.Key == groupKey)?.Fields.FirstOrDefault(f => f.Key == fieldKey);
            if (field == null)
                continue;

            var encoded = field.Values.Select(v => Encode(v.Value)).Where(b => b != null).ToList();

            foreach (var registration in registrations)
            {
                byte[] live;
                try
                {
                    live = Link.ReadBytes(registration.Value, registration.Size);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                if (encoded.Any(b => live.AsSpan(0, Math.Min(live.Length, b.Length)).SequenceEqual(b.AsSpan(0, Math.Min(live.Length, b.Length)))))
                {
                    agree++;
                    break;
                }
            }
        }

        return agree;
    }

    private IEnumerable<(GPARAM Gparam, string Source)> Candidates(string gameName)
    {
        if (Pushed.TryGetValue(gameName, out var pushed))
            yield return (GPARAM.Read(pushed), "pushed");

        var data = Project.Handler.GparamData;

        foreach (var (bank, source) in new[] { (data?.PrimaryBank, "project"), (data?.VanillaBank, "vanilla") })
        {
            var entry = bank?.Entries.Keys.FirstOrDefault(k => string.Equals(GameNameOf(k.Filename), gameName, StringComparison.OrdinalIgnoreCase));
            var bytes = entry == null ? null : ReadFile(bank, entry);

            GPARAM gparam = null;
            try
            {
                if (bytes != null)
                    gparam = GPARAM.Read(bytes);
            }
            catch (Exception)
            {
            }

            if (gparam != null)
                yield return (gparam, source);
        }
    }

    private static byte[] ReadFile(GparamBank bank, FileDictionaryEntry entry)
    {
        try
        {
            var data = bank.TargetFS.ReadFile(entry.Path);
            if (data == null)
                return null;

            var bytes = data.Value;
            return (DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes).ToArray();
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>A GPARAM value as the game stores it (colours as RGBA bytes, as in the file)</summary>
    private static byte[] Encode(object value) => value switch
    {
        float f => BitConverter.GetBytes(f),
        int i => BitConverter.GetBytes(i),
        uint u => BitConverter.GetBytes(u),
        short s => BitConverter.GetBytes(s),
        ushort s => BitConverter.GetBytes(s),
        sbyte b => new[] { (byte)b },
        byte b => new[] { b },
        bool b => new[] { (byte)(b ? 1 : 0) },
        long l => BitConverter.GetBytes(l),
        ulong l => BitConverter.GetBytes(l),
        double d => BitConverter.GetBytes(d),
        Vector2 v => new[] { v.X, v.Y }.SelectMany(BitConverter.GetBytes).ToArray(),
        Vector3 v => new[] { v.X, v.Y, v.Z }.SelectMany(BitConverter.GetBytes).ToArray(),
        Vector4 v => new[] { v.X, v.Y, v.Z, v.W }.SelectMany(BitConverter.GetBytes).ToArray(),
        System.Drawing.Color c => new[] { c.R, c.G, c.B, c.A },
        _ => null,
    };

    #endregion
}
