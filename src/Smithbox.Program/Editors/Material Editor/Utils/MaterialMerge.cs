using SoulsFormats;
using System.Collections;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// A material in a bank, by binder path and file key (the file's name in its binder)
/// </summary>
public record MaterialRecord(string BinderPath, string Key, object Material)
{
    public MaterialSourceType Kind => Material is MTD ? MaterialSourceType.MTD : MaterialSourceType.MATBIN;
    public string ShortName => MaterialMerge.ShortNameOf(Key);
}

/// <summary>
/// The materials of one side of a merge (mine or vanilla), found by binder and key or by short name. Filled in
/// load order, so the short-name lookup gives the material FLVERs reach (later binders win, R2).
/// </summary>
public class MaterialMergeIndex
{
    private readonly Dictionary<(MaterialSourceType, string, string), MaterialRecord> ByKey = new();
    private readonly Dictionary<(MaterialSourceType, string), List<MaterialRecord>> ByName = new();
    private readonly Dictionary<(MaterialSourceType, string), List<MaterialRecord>> Binders = new();

    public int Count => ByKey.Count;

    public void Add(string binderPath, string key, object material)
    {
        if (material is not (MTD or MATBIN))
            return;

        var record = new MaterialRecord(binderPath, key, material);

        ByKey[(record.Kind, Norm(binderPath), Norm(key))] = record;

        if (!Binders.TryGetValue((record.Kind, Norm(binderPath)), out var inBinder))
            Binders[(record.Kind, Norm(binderPath))] = inBinder = new();

        inBinder.Add(record);

        var nameKey = (record.Kind, Norm(record.ShortName));
        if (!ByName.TryGetValue(nameKey, out var list))
            ByName[nameKey] = list = new();

        list.Add(record);
    }

    public MaterialRecord Find(MaterialSourceType kind, string binderPath, string key) =>
        ByKey.TryGetValue((kind, Norm(binderPath), Norm(key)), out var record) ? record : null;

    /// <summary>
    /// The material a short name resolves to: the last one added
    /// </summary>
    public MaterialRecord FindByShortName(MaterialSourceType kind, string shortName) =>
        ByName.TryGetValue((kind, Norm(shortName)), out var list) ? list[^1] : null;

    public bool HasBinder(MaterialSourceType kind, string binderPath) =>
        binderPath != null && Binders.ContainsKey((kind, Norm(binderPath)));

    /// <summary>
    /// The materials of one binder, in the order added
    /// </summary>
    public IReadOnlyList<MaterialRecord> InBinder(MaterialSourceType kind, string binderPath) =>
        binderPath != null && Binders.TryGetValue((kind, Norm(binderPath)), out var list) ? list : [];

    /// <summary>
    /// The path of the binder with this file name (folder ignored), or null
    /// </summary>
    public string FindBinderByFileName(MaterialSourceType kind, string fileName)
    {
        var wanted = Norm(fileName).Split('\\')[^1];

        foreach (var ((k, path), records) in Binders)
        {
            if (k == kind && records.Count > 0 && path.Split('\\')[^1] == wanted)
                return records[0].BinderPath;
        }

        return null;
    }

    internal static string NormKey(string s) => Norm(s);

    private static string Norm(string s) => (s ?? "").Replace('/', '\\').ToLowerInvariant();
}

/// <summary>
/// One material the source offers. <see cref="BinderPath"/> and <see cref="Key"/> are null when the source doesn't
/// say where it lives (a loose file or a JSON export); it's then matched by short name.
/// </summary>
public record MaterialMergeCandidate(string BinderPath, string Key, string ShortName, object Material)
{
    public MaterialSourceType Kind => Material is MTD ? MaterialSourceType.MTD : MaterialSourceType.MATBIN;

    /// <summary>
    /// For a file of a loose binder: the binder's file name (allmaterial_dlc01.matbinbnd.dcx), resolved to the
    /// project's binder of that name before planning. <see cref="Key"/> is then its name in that binder.
    /// </summary>
    public string BinderFileName { get; init; }
}

public enum MaterialMergeKind
{
    /// <summary>Only they have it</summary>
    Add,

    /// <summary>Only they changed it</summary>
    Change,

    /// <summary>Both changed it, in different entries: merges without a choice</summary>
    Combine,

    /// <summary>Both changed the same entry, or something only a whole-material choice can settle</summary>
    Conflict,

    /// <summary>
    /// They deleted a vanilla material I still have (only from a source that holds whole binders). Merge deletes it.
    /// </summary>
    Removed
}

public enum MaterialMergeChoice
{
    /// <summary>Their changes, and the entry choices for conflicts</summary>
    Merge,

    /// <summary>Their material, whole</summary>
    Theirs,

    /// <summary>Their material as a new file under a numbered name (their name is held by another file)</summary>
    AddNumbered,

    Skip
}

public enum MaterialMergeConflictMode
{
    KeepMine,
    TakeTheirs,
    Skip
}

public enum MaterialConflictReason
{
    None,

    /// <summary>Both changed the same entry differently</summary>
    BothChanged,

    /// <summary>Both added a material by this name, differently (no vanilla to compare with)</summary>
    BothAdded,

    /// <summary>Both changed the shader: the params belong to the shader, so entries can't be mixed</summary>
    ShaderChanged,

    /// <summary>A list repeats an entry name, so entries can't be matched</summary>
    RepeatedNames,

    /// <summary>I deleted a vanilla material they changed</summary>
    DeletedByMe,

    /// <summary>They deleted a vanilla material I changed</summary>
    ChangedByMe
}

public enum MaterialEntryChange
{
    Add,
    Change,
    Remove,
    Conflict
}

public enum MaterialEntrySide
{
    Mine,
    Theirs
}

/// <summary>
/// One header field or list entry that differs. Texts are null where that side doesn't have it.
/// </summary>
public class MaterialMergeEntry
{
    public const string HeaderList = "Header";

    /// <summary><see cref="HeaderList"/>, or the list: Params, Samplers, Textures</summary>
    public string List { get; init; }

    public string Name { get; init; }
    public MaterialEntryChange Change { get; init; }

    public string VanillaText { get; init; }
    public string MineText { get; init; }
    public string TheirsText { get; init; }

    public MaterialEntrySide Choice { get; set; }
}

/// <summary>
/// What merging one source material does. <see cref="Result"/> is the material to apply, rebuilt by
/// <see cref="Rebuild"/> from the choices; it shares nothing with either side, so it can go straight into the
/// project.
/// </summary>
public class MaterialMergeItem
{
    public MaterialMergeKind Kind { get; init; }
    public MaterialSourceType MaterialKind { get; init; }
    public string ShortName { get; init; }

    /// <summary>Where the source has it (null for a loose file or JSON)</summary>
    public string TheirBinderPath { get; init; }
    public string TheirKey { get; init; }

    /// <summary>The material it merges into, or null when it would be added</summary>
    public MaterialRecord Mine { get; init; }

    /// <summary>Mine as analysed: an item whose material changed since is skipped at apply</summary>
    public string MineSignature { get; init; }

    /// <summary>
    /// For an add: the binder it goes in (theirs, or vanilla's when restoring a deleted one), or null when the
    /// tool's target binder decides
    /// </summary>
    public string AddBinderPath { get; init; }

    /// <summary>Their name is held by another file of mine (<see cref="Mine"/>), in another binder or folder</summary>
    public bool NameTaken { get; init; }

    public MaterialConflictReason Reason { get; init; }

    /// <summary>Only Theirs / Skip (and AddNumbered): entries can't be merged one by one</summary>
    public bool WholeOnly { get; init; }

    public List<MaterialMergeEntry> Entries { get; } = new();

    public IReadOnlyList<MaterialMergeChoice> Choices { get; init; }
    public MaterialMergeChoice Choice { get; set; }

    public object Result { get; private set; }
    public List<MaterialFinding> Findings { get; private set; } = new();
    public bool HasErrors => Findings.Any(f => f.Severity is MaterialCheckSeverity.Error);

    /// <summary>Whether applying adds a file rather than changing one</summary>
    public bool Adds => Kind is not MaterialMergeKind.Removed && (Mine == null || Choice is MaterialMergeChoice.AddNumbered);

    /// <summary>Whether applying deletes <see cref="Mine"/></summary>
    public bool Removes => Kind is MaterialMergeKind.Removed && Choice is MaterialMergeChoice.Merge;

    internal object MineCopy;
    internal object TheirsCopy;

    /// <summary>
    /// Works out <see cref="Result"/> and its findings from the item's and entries' choices
    /// </summary>
    public void Rebuild()
    {
        Result = Kind is MaterialMergeKind.Removed ? null : Choice switch
        {
            MaterialMergeChoice.Skip => null,
            MaterialMergeChoice.Theirs or MaterialMergeChoice.AddNumbered => MaterialEntries.Copy(TheirsCopy),
            _ => Kind is MaterialMergeKind.Add or MaterialMergeKind.Change ? MaterialEntries.Copy(TheirsCopy)
                : WholeOnly ? null
                : MaterialMerge.BuildMerged(MineCopy, TheirsCopy, Entries)
        };

        Findings = Result == null ? new() : MaterialChecks.Check(Result);
    }
}

public class MaterialMergeOptions
{
    public MaterialMergeConflictMode ConflictMode { get; init; } = MaterialMergeConflictMode.KeepMine;
    public bool IncludeAdditions { get; init; } = true;

    /// <summary>Whether materials they deleted (and I didn't change) are deleted by default</summary>
    public bool IncludeRemovals { get; init; }
}

public class MaterialMergePlan
{
    public string SourceName { get; init; }
    public List<MaterialMergeItem> Items { get; } = new();

    /// <summary>Source materials that merge to nothing (the same as mine, or as vanilla)</summary>
    public int Unchanged { get; set; }

    /// <summary>What the source had that couldn't be merged, and why</summary>
    public List<string> Skipped { get; } = new();

    public int Count(MaterialMergeKind kind) => Items.Count(i => i.Kind == kind);
}

/// <summary>
/// Three-way material merge (Claude/Documentation/Materials and Textures.md §2, F11): V vanilla, M mine, O the source.
/// <list type="bullet">
/// <item>O equals M, or O equals V: nothing to do.</item>
/// <item>Only they changed it (M equals V): Change, take theirs. Only they have it: Add.</item>
/// <item>Both changed it: merged entry by entry (params by name, samplers and textures by type, header fields),
/// a Conflict only where both changed one entry differently. Both changing the shader, or a repeated entry name,
/// leaves a whole-material choice.</item>
/// </list>
/// Read-only and without editor dependencies: the editor and the survey tool build the indexes.
/// </summary>
public static class MaterialMerge
{
    /// <summary>
    /// A file's name without folder, .dcx or extension, as MaterialBank.ShortNameOf
    /// </summary>
    public static string ShortNameOf(string key)
    {
        var name = (key ?? "").Replace('\\', '/').Split('/')[^1];

        if (name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        var dot = name.LastIndexOf('.');
        return dot > 0 ? name[..dot] : name;
    }

    public static MaterialMergePlan BuildPlan(MaterialMergeSource source, MaterialMergeIndex mine,
        MaterialMergeIndex vanilla, MaterialMergeOptions options)
    {
        var plan = new MaterialMergePlan { SourceName = source.Name };
        plan.Skipped.AddRange(source.Skipped);

        // A loose binder's files go in my binder of the same file name (or vanilla's, if I deleted every file of
        // it); with neither, they're matched by short name
        var resolved = source.Candidates.Select(c =>
        {
            if (c.BinderFileName == null || c.BinderPath != null)
                return c;

            var path = mine.FindBinderByFileName(c.Kind, c.BinderFileName) ?? vanilla.FindBinderByFileName(c.Kind, c.BinderFileName);
            return path == null ? c with { Key = null } : c with { BinderPath = path };
        });

        // A material the source holds twice: the later one, as in a binder
        var candidates = resolved
            .GroupBy(c => (c.Kind, c.BinderPath?.ToLowerInvariant(), (c.Key ?? c.ShortName).ToLowerInvariant()))
            .Select(g =>
            {
                if (g.Count() > 1)
                    plan.Skipped.Add($"{g.Last().ShortName}: repeated in the source, the last one is used");
                return g.Last();
            });

        foreach (var candidate in candidates)
        {
            var item = Plan(candidate, mine, vanilla, options);

            if (item == null)
            {
                plan.Unchanged++;
                continue;
            }

            item.Rebuild();

            // Both changed it, in ways the entries don't show (order, a key): merging gives mine back
            if (item.Kind is MaterialMergeKind.Combine && item.Result != null &&
                MaterialEntries.Signature(item.Result) == item.MineSignature)
            {
                plan.Unchanged++;
                continue;
            }

            plan.Items.Add(item);
        }

        PlanRemovals(plan, source, mine, vanilla, options);

        return plan;
    }

    /// <summary>
    /// In each binder the source holds whole: the vanilla materials I have that it doesn't, so they deleted them.
    /// My own additions (not in vanilla) aren't theirs to delete.
    /// </summary>
    private static void PlanRemovals(MaterialMergePlan plan, MaterialMergeSource source, MaterialMergeIndex mine,
        MaterialMergeIndex vanilla, MaterialMergeOptions options)
    {
        if (source.CompleteBinders.Count == 0)
            return;

        var theirs = source.Candidates
            .Where(c => c.BinderPath != null)
            .Select(c => (c.Kind, MaterialMergeIndex.NormKey(c.BinderPath), MaterialMergeIndex.NormKey(c.Key)))
            .ToHashSet();

        foreach (var binder in source.CompleteBinders)
        {
            foreach (var kind in new[] { MaterialSourceType.MTD, MaterialSourceType.MATBIN })
            {
                foreach (var m in mine.InBinder(kind, binder))
                {
                    if (theirs.Contains((kind, MaterialMergeIndex.NormKey(m.BinderPath), MaterialMergeIndex.NormKey(m.Key))) ||
                        source.IsUnreadable(m.BinderPath, m.Key))
                        continue;

                    var v = vanilla.Find(kind, m.BinderPath, m.Key);
                    if (v == null)
                        continue;

                    var sM = MaterialEntries.Signature(m.Material);
                    var changedByMe = sM != MaterialEntries.Signature(v.Material);

                    var item = new MaterialMergeItem
                    {
                        Kind = MaterialMergeKind.Removed,
                        MaterialKind = kind,
                        ShortName = m.ShortName,
                        TheirBinderPath = m.BinderPath,
                        TheirKey = m.Key,
                        Mine = m,
                        MineSignature = sM,
                        Reason = changedByMe ? MaterialConflictReason.ChangedByMe : MaterialConflictReason.None,
                        Choices = [MaterialMergeChoice.Merge, MaterialMergeChoice.Skip],
                        Choice = options.IncludeRemovals && !changedByMe ? MaterialMergeChoice.Merge : MaterialMergeChoice.Skip
                    };

                    item.Rebuild();
                    plan.Items.Add(item);
                }
            }
        }
    }

    private static MaterialMergeItem Plan(MaterialMergeCandidate o, MaterialMergeIndex mine, MaterialMergeIndex vanilla,
        MaterialMergeOptions options)
    {
        var kind = o.Kind;

        // Where it goes: the same binder and key, else (or for a source that doesn't say) by short name
        var m = o.BinderPath != null ? mine.Find(kind, o.BinderPath, o.Key) : null;
        var nameTaken = false;

        if (m == null)
        {
            m = mine.FindByShortName(kind, o.ShortName);
            nameTaken = m != null && o.BinderPath != null;
        }

        var v = m != null ? vanilla.Find(kind, m.BinderPath, m.Key)
            : o.BinderPath != null ? vanilla.Find(kind, o.BinderPath, o.Key) ?? vanilla.FindByShortName(kind, o.ShortName)
            : vanilla.FindByShortName(kind, o.ShortName);

        var sO = MaterialEntries.Signature(o.Material);
        var sV = v == null ? null : MaterialEntries.Signature(v.Material);

        if (m == null)
        {
            // Only they have it
            if (v == null)
            {
                return new MaterialMergeItem
                {
                    Kind = MaterialMergeKind.Add,
                    MaterialKind = kind,
                    ShortName = o.ShortName,
                    TheirBinderPath = o.BinderPath,
                    TheirKey = o.Key,
                    AddBinderPath = mine.HasBinder(kind, o.BinderPath) ? o.BinderPath : null,
                    Choices = [MaterialMergeChoice.Merge, MaterialMergeChoice.Skip],
                    Choice = options.IncludeAdditions ? MaterialMergeChoice.Merge : MaterialMergeChoice.Skip,
                    TheirsCopy = MaterialEntries.Copy(o.Material)
                };
            }

            // I deleted it: nothing to do unless they changed it
            if (sO == sV)
                return null;

            return new MaterialMergeItem
            {
                Kind = MaterialMergeKind.Conflict,
                MaterialKind = kind,
                ShortName = o.ShortName,
                TheirBinderPath = o.BinderPath,
                TheirKey = o.Key,
                AddBinderPath = mine.HasBinder(kind, o.BinderPath) ? o.BinderPath : v.BinderPath,
                Reason = MaterialConflictReason.DeletedByMe,
                WholeOnly = true,
                Choices = [MaterialMergeChoice.Theirs, MaterialMergeChoice.Skip],
                Choice = options.ConflictMode is MaterialMergeConflictMode.TakeTheirs ? MaterialMergeChoice.Theirs : MaterialMergeChoice.Skip,
                TheirsCopy = MaterialEntries.Copy(o.Material)
            };
        }

        var sM = MaterialEntries.Signature(m.Material);

        if (sO == sM || (sV != null && sO == sV))
            return null;

        var entries = new List<MaterialMergeEntry>();
        var reason = MaterialConflictReason.None;
        var wholeOnly = false;

        if (sV != null && sM == sV)
        {
            // Only they changed it: listed entry by entry, taken whole
            PlanEntries(entries, v.Material, m.Material, o.Material, options.ConflictMode);
        }
        else
        {
            var conflicts = PlanEntries(entries, v?.Material, m.Material, o.Material, options.ConflictMode);

            if (HasRepeatedNames(m.Material) || HasRepeatedNames(o.Material) || (v != null && HasRepeatedNames(v.Material)))
            {
                wholeOnly = true;
                reason = MaterialConflictReason.RepeatedNames;
            }
            else if (entries.Any(e => e.List == MaterialMergeEntry.HeaderList && e.Name == "ShaderPath" &&
                                      e.Change is MaterialEntryChange.Conflict))
            {
                wholeOnly = true;
                reason = MaterialConflictReason.ShaderChanged;
            }
            else if (conflicts > 0)
            {
                reason = v == null ? MaterialConflictReason.BothAdded : MaterialConflictReason.BothChanged;
            }
        }

        var itemKind = sV != null && sM == sV ? MaterialMergeKind.Change
            : wholeOnly || reason != MaterialConflictReason.None ? MaterialMergeKind.Conflict
            : MaterialMergeKind.Combine;

        var choices = new List<MaterialMergeChoice>();
        if (nameTaken) choices.Add(MaterialMergeChoice.AddNumbered);
        if (!wholeOnly) choices.Add(MaterialMergeChoice.Merge);
        if (itemKind is MaterialMergeKind.Combine or MaterialMergeKind.Conflict) choices.Add(MaterialMergeChoice.Theirs);
        choices.Add(MaterialMergeChoice.Skip);

        var choice = nameTaken ? MaterialMergeChoice.Skip
            : itemKind is MaterialMergeKind.Change or MaterialMergeKind.Combine ? MaterialMergeChoice.Merge
            : options.ConflictMode is MaterialMergeConflictMode.Skip ? MaterialMergeChoice.Skip
            : !wholeOnly ? MaterialMergeChoice.Merge
            : options.ConflictMode is MaterialMergeConflictMode.TakeTheirs ? MaterialMergeChoice.Theirs
            : MaterialMergeChoice.Skip;

        var item = new MaterialMergeItem
        {
            Kind = itemKind,
            MaterialKind = kind,
            ShortName = o.ShortName,
            TheirBinderPath = o.BinderPath,
            TheirKey = o.Key,
            Mine = m,
            MineSignature = sM,
            AddBinderPath = mine.HasBinder(kind, o.BinderPath) ? o.BinderPath : m.BinderPath,
            NameTaken = nameTaken,
            Reason = reason,
            WholeOnly = wholeOnly,
            Choices = choices,
            Choice = choice,
            MineCopy = MaterialEntries.Copy(m.Material),
            TheirsCopy = MaterialEntries.Copy(o.Material)
        };

        item.Entries.AddRange(entries);
        return item;
    }

    #region Entries
    /// <summary>
    /// The header fields and list entries where theirs differs from mine and from vanilla. Returns how many are
    /// conflicts. With no vanilla, every difference is one (both added it).
    /// </summary>
    private static int PlanEntries(List<MaterialMergeEntry> entries, object v, object m, object o,
        MaterialMergeConflictMode mode)
    {
        var conflictChoice = mode is MaterialMergeConflictMode.TakeTheirs ? MaterialEntrySide.Theirs : MaterialEntrySide.Mine;
        var conflicts = 0;

        void Add(string list, string name, MaterialEntryChange change, string vt, string mt, string ot)
        {
            if (change is MaterialEntryChange.Conflict)
                conflicts++;

            entries.Add(new MaterialMergeEntry
            {
                List = list,
                Name = name,
                Change = change,
                VanillaText = vt,
                MineText = mt,
                TheirsText = ot,
                Choice = change is MaterialEntryChange.Conflict ? conflictChoice : MaterialEntrySide.Theirs
            });
        }

        // Header fields
        foreach (var field in HeaderFields(m))
        {
            var mt = GetHeader(m, field) ?? "";
            var ot = GetHeader(o, field) ?? "";
            var vt = v == null ? null : GetHeader(v, field) ?? "";

            if (ot == mt || (vt != null && ot == vt))
                continue;

            Add(MaterialEntryHeader, field, vt != null && mt == vt ? MaterialEntryChange.Change : MaterialEntryChange.Conflict, vt, mt, ot);
        }

        // Lists, by entry name
        foreach (var (list, mList) in Lists(m))
        {
            var oList = ListOf(o, list);
            var vList = v == null ? null : ListOf(v, list);

            var mByName = ByName(mList);
            var oByName = ByName(oList);
            var vByName = vList == null ? new() : ByName(vList);

            var names = oByName.Keys.Concat(mByName.Keys).Distinct().ToList();

            foreach (var name in names)
            {
                var hasM = mByName.TryGetValue(name, out var me);
                var hasO = oByName.TryGetValue(name, out var oe);
                var hasV = vByName.TryGetValue(name, out var ve);

                var sm = hasM ? MaterialEntries.EntrySignature(me) : null;
                var so = hasO ? MaterialEntries.EntrySignature(oe) : null;
                var sv = hasV ? MaterialEntries.EntrySignature(ve) : null;

                var vt = hasV ? MaterialEntries.EntryText(ve) : null;
                var mt = hasM ? MaterialEntries.EntryText(me) : null;
                var ot = hasO ? MaterialEntries.EntryText(oe) : null;

                if (hasO && hasM)
                {
                    if (so == sm || (hasV && so == sv))
                        continue;

                    Add(list, name, hasV && sm == sv ? MaterialEntryChange.Change : MaterialEntryChange.Conflict, vt, mt, ot);
                }
                else if (hasO)
                {
                    // I removed a vanilla entry: only a conflict if they changed it
                    if (hasV)
                    {
                        if (so != sv)
                            Add(list, name, MaterialEntryChange.Conflict, vt, mt, ot);
                    }
                    else
                    {
                        Add(list, name, MaterialEntryChange.Add, vt, mt, ot);
                    }
                }
                else if (hasV)
                {
                    // They removed a vanilla entry I have: theirs to remove unless I changed it
                    Add(list, name, sm == sv ? MaterialEntryChange.Remove : MaterialEntryChange.Conflict, vt, mt, ot);
                }

                // Only I have it, and vanilla doesn't: my addition, kept
            }
        }

        return conflicts;
    }

    private const string MaterialEntryHeader = MaterialMergeEntry.HeaderList;

    /// <summary>
    /// Mine with the entries chosen from theirs: header fields set, list entries replaced, removed or added.
    /// Mine's order is kept; an entry only they have goes after the entry before it in their list.
    /// </summary>
    internal static object BuildMerged(object mineCopy, object theirsCopy, List<MaterialMergeEntry> entries)
    {
        var result = MaterialEntries.Copy(mineCopy);
        var taken = entries.Where(e => e.Choice is MaterialEntrySide.Theirs).ToList();

        foreach (var e in taken.Where(e => e.List == MaterialEntryHeader))
        {
            SetHeader(result, e.Name, GetHeader(theirsCopy, e.Name));
        }

        foreach (var (list, items) in Lists(result))
        {
            var theirs = ListOf(theirsCopy, list).Cast<object>().ToList();
            var theirsByName = ByName(theirs);
            var chosen = taken.Where(e => e.List == list).Select(e => e.Name).ToHashSet();

            var merged = new List<object>();

            foreach (var entry in items.Cast<object>())
            {
                var name = MaterialEntries.EntryName(entry) ?? "";

                if (!chosen.Contains(name))
                    merged.Add(entry);
                else if (theirsByName.TryGetValue(name, out var t))
                    merged.Add(Keyed(MaterialEntries.CopyEntry(t)));

                // else: removed by them
            }

            // Their additions, after the entry before them in their list
            string previous = null;
            foreach (var t in theirs)
            {
                var name = MaterialEntries.EntryName(t) ?? "";
                var present = merged.FindIndex(x => (MaterialEntries.EntryName(x) ?? "") == name);

                if (present < 0 && chosen.Contains(name))
                {
                    var at = previous == null ? 0 : merged.FindIndex(x => (MaterialEntries.EntryName(x) ?? "") == previous) + 1;
                    merged.Insert(at, Keyed(MaterialEntries.CopyEntry(t)));
                    previous = name;
                }
                else if (present >= 0)
                {
                    previous = name;
                }
            }

            items.Clear();
            foreach (var entry in merged)
                items.Add(entry);
        }

        // A MATBIN's key follows its source path
        if (result is MATBIN matbin && taken.Any(e => e.List == MaterialEntryHeader && e.Name == nameof(MATBIN.SourcePath)))
            MaterialKeys.Apply(matbin);

        return result;
    }

    /// <summary>
    /// A MATBIN param or sampler taken from theirs gets the key of its text, whatever theirs had
    /// </summary>
    private static object Keyed(object entry)
    {
        MaterialKeys.Apply(entry);
        return entry;
    }

    private static string[] HeaderFields(object material) => material switch
    {
        MTD => [nameof(MTD.ShaderPath), nameof(MTD.Description)],
        MATBIN => [nameof(MATBIN.ShaderPath), nameof(MATBIN.SourcePath)],
        _ => []
    };

    private static string GetHeader(object material, string field) => (material, field) switch
    {
        (MTD m, nameof(MTD.ShaderPath)) => m.ShaderPath,
        (MTD m, nameof(MTD.Description)) => m.Description,
        (MATBIN m, nameof(MATBIN.ShaderPath)) => m.ShaderPath,
        (MATBIN m, nameof(MATBIN.SourcePath)) => m.SourcePath,
        _ => null
    };

    private static void SetHeader(object material, string field, string value)
    {
        switch (material, field)
        {
            case (MTD m, nameof(MTD.ShaderPath)): m.ShaderPath = value; break;
            case (MTD m, nameof(MTD.Description)): m.Description = value; break;
            case (MATBIN m, nameof(MATBIN.ShaderPath)): m.ShaderPath = value; break;
            case (MATBIN m, nameof(MATBIN.SourcePath)): m.SourcePath = value; break;
        }
    }

    private static IEnumerable<(string list, IList items)> Lists(object material) => material switch
    {
        MTD m => [(nameof(MTD.Params), m.Params), (nameof(MTD.Textures), m.Textures)],
        MATBIN m => [(nameof(MATBIN.Params), m.Params), (nameof(MATBIN.Samplers), m.Samplers)],
        _ => []
    };

    private static IList ListOf(object material, string list) =>
        Lists(material).FirstOrDefault(l => l.list == list).items ?? Array.Empty<object>();

    private static Dictionary<string, object> ByName(IEnumerable items)
    {
        var byName = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var item in items)
            byName.TryAdd(MaterialEntries.EntryName(item) ?? "", item);

        return byName;
    }

    private static bool HasRepeatedNames(object material) =>
        Lists(material).Any(l =>
        {
            var names = l.items.Cast<object>().Select(e => MaterialEntries.EntryName(e) ?? "").ToList();
            return names.Distinct(StringComparer.Ordinal).Count() != names.Count;
        });
    #endregion
}
