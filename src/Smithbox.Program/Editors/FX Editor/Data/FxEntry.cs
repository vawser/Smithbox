using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.FxEditor;

/// <summary>Where an effect is stored: a binder and the FXR's name inside it.</summary>
public record FxBinderRef(FileDictionaryEntry Binder, string InternalName);

/// <summary>How an effect compares with the game's own copy.</summary>
public enum FxVanillaState
{
    /// <summary>Read from the game, or identical to it.</summary>
    Same,
    /// <summary>The project's copy differs from the game's.</summary>
    Modified,
    /// <summary>The game has no such effect (made in the editor or added by the project).</summary>
    Added,
}

/// <summary>
/// One effect id. Vanilla stores identical copies of an effect in every binder that needs it, so the editor
/// edits the effect once and saves it to all of them (FX Editor Plan, decision 1).
/// </summary>
public class FxEntry
{
    public int Id;

    /// <summary>
    /// Every binder that should hold this effect, in binder path order. The first is read from. Saving makes the
    /// binders match: see <see cref="OnDisk"/> (FX Effect List Actions Plan §2).
    /// </summary>
    public List<FxBinderRef> Binders = new();

    /// <summary>The binders whose files hold this effect now, by binder path: set by indexing and by each save.</summary>
    public Dictionary<string, FxBinderRef> OnDisk = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The loaded effect, null until selected.</summary>
    public FXR Fxr;

    /// <summary>
    /// The model view of the loaded effect (<see cref="FxData.Effect"/>), kept while the same file object is loaded,
    /// so its adapters keep their identity. Format-neutral code reads the effect through it.
    /// </summary>
    internal Model.IFxEffect ModelCache;

    /// <summary>
    /// The effect's file bytes as last read or saved, deflated. Loading and reloading unpack this, so no binder
    /// has to be decompressed when an effect is selected.
    /// </summary>
    public byte[] Packed;
    public int PackedLength;

    /// <summary>The effect's ffxreslist (ER, AC6, NR) from the first binder that has one; copied into binders it's added to.</summary>
    public byte[] ResourceList;

    public FxVanillaState Vanilla = FxVanillaState.Same;

    /// <summary>The game's copy of a <see cref="FxVanillaState.Modified"/> effect, deflated like <see cref="Packed"/>.</summary>
    public byte[] VanillaPacked;
    public int VanillaLength;

    public bool IsDirty;

    /// <summary>
    /// Made in the editor and not saved yet: no binder holds it, so saving adds the file (and an ffxreslist, copied
    /// from <see cref="ResourceList"/>, or <see cref="ResourceListFrom"/>'s in the same binder, where the binder keeps them).
    /// </summary>
    public bool IsNew;

    /// <summary>For a new effect: the effect whose ffxreslist the new one starts from, when it has no <see cref="ResourceList"/>.</summary>
    public int ResourceListFrom = -1;

    /// <summary>The Particles alias name, or empty.</summary>
    public string Alias = "";

    public bool IsLoaded => Fxr != null;

    /// <summary>The name the game uses for the file, e.g. f000620400.</summary>
    public string Stem => $"f{Id:D9}";

    /// <summary>Whether a binder is in <see cref="Binders"/>.</summary>
    public bool InBinder(string path) => Binders.Any(b => b.Binder.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

    /// <summary>Whether saving would add or remove a file: <see cref="Binders"/> differs from <see cref="OnDisk"/>.</summary>
    public bool MembershipChanged => Binders.Count != OnDisk.Count || Binders.Any(b => !OnDisk.ContainsKey(b.Binder.Path));
}
