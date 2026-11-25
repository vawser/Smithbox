using SoulsFormats;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// What an import of stored text would do, built without touching the project
/// (FmgImporter.BuildPlan). Shown by the import preview, then rebuilt and applied (FmgImporter.ApplyPlan).
/// </summary>
public class TextImportPlan
{
    public StoredFmgContainer Source;
    public ImportBehavior Behavior;
    public FmgImportType Scope;
    public TextContainerWrapper Target;

    /// <summary>The export names another container than the target (a heads-up, not a refusal).</summary>
    public bool ContainerMismatch;

    /// <summary>Stored FMGs matched to a target FMG in scope, in target order.</summary>
    public List<TextImportFmgPlan> Fmgs = new();

    /// <summary>Stored FMGs with no target FMG in scope (other binder ID, other name, or outside the scope).</summary>
    public List<StoredFmgWrapper> Unmatched = new();

    public int Added => Fmgs.Sum(f => f.Added.Count);
    public int Changed => Fmgs.Sum(f => f.Changed.Count);
    public int Kept => Fmgs.Sum(f => f.Kept.Count);
    public int Identical => Fmgs.Sum(f => f.Identical);

    public bool HasChanges => Added + Changed > 0;
}

public class TextImportFmgPlan
{
    public TextFmgWrapper Target;
    public StoredFmgWrapper Stored;

    /// <summary>Stored entries whose ID the target FMG doesn't have: added.</summary>
    public List<FMG.Entry> Added = new();

    /// <summary>Existing entries whose text the import changes (Replace).</summary>
    public List<(FMG.Entry Entry, string NewText)> Changed = new();

    /// <summary>Existing entries with different stored text that Append leaves alone.</summary>
    public List<(FMG.Entry Entry, string NewText)> Kept = new();

    /// <summary>Existing entries whose text already matches.</summary>
    public int Identical;
}
