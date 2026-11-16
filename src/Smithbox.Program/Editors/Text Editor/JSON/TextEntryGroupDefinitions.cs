using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Assets/FMG/&lt;game&gt;/Text Entry Groups.json: which FMGs form an entry group, and what each slot of
/// the group means. Entries with the same ID across a group's FMGs are edited, duplicated and
/// deleted together (e.g. a weapon's name, summary and description).
/// </summary>
public class TextEntryGroupDefinitions
{
    public List<TextEntryGroupDefinition> Groups { get; set; } = new();
}

public class TextEntryGroupDefinition
{
    /// <summary>
    /// For people reading the file only
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Up to MaxSlots; the order is the order the Contents panel shows them in
    /// </summary>
    public List<TextEntryGroupSlotDefinition> Slots { get; set; } = new();

    public const int MaxSlots = 10;
}

public class TextEntryGroupSlotDefinition
{
    /// <summary>
    /// What the slot holds, e.g. "Title", "Summary", "Lore". Shown through the loc key
    /// TEXT_GroupSlot_&lt;Meaning&gt; when it exists. "Title" has the title behaviour (the ID field
    /// shows its ID, the Entry Creator's incremental template is added to it), and Copy/Paste
    /// matches slots between groups by this name.
    /// </summary>
    public string Meaning { get; set; }

    /// <summary>
    /// The FMG's binder ID in the container
    /// </summary>
    public int FmgID { get; set; }
}
