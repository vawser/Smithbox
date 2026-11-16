using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Text entry groups: FMGs whose entries with the same ID belong together (e.g. a weapon's
/// name, summary and description), edited, duplicated and deleted as one.
/// The groups are defined per game in Assets/FMG/&lt;game&gt;/Text Entry Groups.json, up to
/// TextEntryGroupDefinition.MaxSlots slots each, every slot naming what it means.
/// (They were hard-coded here with four fixed slots: title, summary, description, effect.)
/// </summary>
public class TextEntryGroupManager
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public TextEntryGroupManager(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    private List<EntryGroupAssociation> _groupings;
    private TextEntryGroupDefinitions _groupingsSource;

    /// <summary>
    /// The game's groups, built from the loaded definitions (rebuilt if they are reloaded)
    /// </summary>
    public List<EntryGroupAssociation> GetGroupings()
    {
        var source = Project.Handler.TextData?.EntryGroups;

        if (_groupings == null || source != _groupingsSource)
        {
            _groupings = source?.Groups.Select(g => new EntryGroupAssociation(g)).ToList() ?? new();
            _groupingsSource = source;
        }

        return _groupings;
    }

    /// <summary>
    /// The group the FMG with this binder ID belongs to, or null. The first group wins when an
    /// FMG is listed in more than one.
    /// </summary>
    public EntryGroupAssociation GetAssociation(int fmgId)
    {
        return GetGroupings().FirstOrDefault(g => g.Contains(fmgId));
    }

    /// <summary>
    /// The FMGs of the group the passed FMG belongs to, in slot order, that the selected
    /// container has (a slot whose FMG the container lacks is left out). Empty if the FMG isn't
    /// grouped. The passed FMG itself is included, in its own slot.
    /// </summary>
    public List<(EntryGroupSlot Slot, TextFmgWrapper Wrapper)> GetGroupWrappers(int fmgId)
    {
        var result = new List<(EntryGroupSlot, TextFmgWrapper)>();

        var association = GetAssociation(fmgId);
        var fmgWrappers = Parent.Selection.SelectedContainerWrapper?.FmgWrappers;

        if (association == null || fmgWrappers == null)
            return result;

        foreach (var slot in association.Slots)
        {
            var wrapper = fmgWrappers.FirstOrDefault(w => w.ID == slot.FmgID);

            if (wrapper != null)
                result.Add((slot, wrapper));
        }

        return result;
    }

    /// <summary>
    /// The FMG holding the slot with this meaning in the passed FMG's group, or null
    /// </summary>
    public TextFmgWrapper GetSlotWrapper(int fmgId, string meaning)
    {
        return GetGroupWrappers(fmgId)
            .FirstOrDefault(p => EntryGroupSlot.SameMeaning(p.Slot.Meaning, meaning))
            .Wrapper;
    }

    /// <param name="lookup">
    /// Optional ID index, shared across one bulk operation. Without it each call scans
    /// the associated FMGs linearly, which made Select All + Delete/Duplicate O(n²).
    /// </param>
    public FmgEntryGroup GetEntryGroup(FMG.Entry entry, FmgEntryLookup lookup = null)
    {
        if (entry == null)
            return null;

        if (Parent.Selection.SelectedContainerWrapper?.FmgWrappers == null)
            return null;

        if (Parent.Selection.SelectedFmgWrapper == null)
            return null;

        return new FmgEntryGroup(this,
            Parent.Selection.SelectedContainerWrapper,
            Parent.Selection.SelectedFmgWrapper, entry, lookup);
    }
}

/// <summary>
/// One slot of a group definition: which FMG, and what it holds
/// </summary>
public class EntryGroupSlot
{
    public const string TitleMeaning = "Title";

    public string Meaning { get; }
    public int FmgID { get; }

    public EntryGroupSlot(string meaning, int fmgId)
    {
        Meaning = meaning.Trim();
        FmgID = fmgId;
    }

    /// <summary>
    /// The title slot: the ID field shows its ID, the Entry Creator's incremental template is
    /// added to it, and its box is sized for one line
    /// </summary>
    public bool IsTitle => SameMeaning(Meaning, TitleMeaning);

    /// <summary>
    /// The localized label (TEXT_GroupSlot_&lt;Meaning&gt;) when there is one, otherwise the meaning itself
    /// </summary>
    public string Label
    {
        get
        {
            var key = $"TEXT_GroupSlot_{Meaning.Replace(' ', '_')}";

            return LOC.Has(key) ? LOC.Get(key) : Meaning;
        }
    }

    public static bool SameMeaning(string a, string b) =>
        string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// A group definition: the FMG binder IDs whose same-ID entries belong together, in slot order
/// </summary>
public class EntryGroupAssociation
{
    public string Name { get; }

    public IReadOnlyList<EntryGroupSlot> Slots { get; }

    public EntryGroupAssociation(TextEntryGroupDefinition definition)
    {
        Name = definition.Name ?? "";
        Slots = definition.Slots
            .Select(s => new EntryGroupSlot(s.Meaning, s.FmgID))
            .ToList();
    }

    public bool Contains(int fmgId) => Slots.Any(s => s.FmgID == fmgId);
}

/// <summary>
/// One slot of an entry's group: the slot, the FMG that holds it in the container, and the
/// entry with the group's ID in that FMG (null when the FMG has no such entry yet)
/// </summary>
public class FmgEntryGroupSlot
{
    public EntryGroupSlot Slot { get; init; }
    public TextFmgWrapper Wrapper { get; init; }
    public FMG.Entry Entry { get; init; }

    public string Meaning => Slot.Meaning;
    public string Label => Slot.Label;
    public bool IsTitle => Slot.IsTitle;
}

/// <summary>
/// The group of entries the selected FMG entry belongs to: for each slot of its FMG's group,
/// the entry with the same ID. Slots whose FMG the container doesn't have are left out.
/// </summary>
public class FmgEntryGroup
{
    /// <summary>
    /// The selected FMG is in a group. If false, the Contents panel shows the simple editor.
    /// </summary>
    public bool SupportsGrouping { get; }

    public List<FmgEntryGroupSlot> Slots { get; } = new();

    public FmgEntryGroupSlot TitleSlot => Slots.FirstOrDefault(s => s.IsTitle);

    /// <summary>
    /// The title slot's entry, or null when the group has no title slot or no entry in it
    /// </summary>
    public FMG.Entry Title => TitleSlot?.Entry;

    /// <summary>
    /// The entries the group has, in slot order
    /// </summary>
    public IEnumerable<FMG.Entry> Entries => Slots.Select(s => s.Entry).Where(e => e != null);

    public FmgEntryGroup(TextEntryGroupManager entryManager,
        TextContainerWrapper containerInfo,
        TextFmgWrapper selectedFmgInfo,
        FMG.Entry baseEntry,
        FmgEntryLookup lookup = null)
    {
        var association = entryManager.GetAssociation(selectedFmgInfo.ID);

        if (association == null)
            return;

        SupportsGrouping = true;

        foreach (var slot in association.Slots)
        {
            var wrapper = containerInfo.FmgWrappers.FirstOrDefault(w => w.ID == slot.FmgID);

            if (wrapper?.File == null)
                continue;

            Slots.Add(new FmgEntryGroupSlot
            {
                Slot = slot,
                Wrapper = wrapper,
                Entry = FindEntry(wrapper.File, baseEntry, lookup)
            });
        }
    }

    private static FMG.Entry FindEntry(FMG fmg, FMG.Entry baseEntry, FmgEntryLookup lookup)
    {
        // The slot is the selected entry's own FMG: use the entry itself. Finding it by ID took
        // the first entry with that ID, so with duplicate IDs the Contents panel, Delete and
        // Duplicate acted on a different entry than the one selected.
        if (fmg == baseEntry.Parent)
            return baseEntry;

        if (lookup != null)
            return lookup.Find(fmg, baseEntry.ID);

        for (var i = 0; i < fmg.Entries.Count; i++)
        {
            if (fmg.Entries[i].ID == baseEntry.ID)
                return fmg.Entries[i];
        }

        return null;
    }
}

/// <summary>
/// Lazily built ID → entry index per FMG, for one bulk operation (Delete, Duplicate).
/// Matches the linear scan: the first entry with an ID wins. Only valid while the
/// FMGs aren't modified, so build a new one per operation, before running its actions.
/// </summary>
public class FmgEntryLookup
{
    private readonly Dictionary<FMG, Dictionary<int, FMG.Entry>> Index = new(ReferenceEqualityComparer.Instance);

    public FMG.Entry Find(FMG fmg, int id)
    {
        if (fmg == null)
            return null;

        if (!Index.TryGetValue(fmg, out var map))
        {
            map = new Dictionary<int, FMG.Entry>(fmg.Entries.Count);

            foreach (var entry in fmg.Entries)
            {
                map.TryAdd(entry.ID, entry);
            }

            Index[fmg] = map;
        }

        return map.TryGetValue(id, out var found) ? found : null;
    }
}
