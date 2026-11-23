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

public static class FxVanillaStateExtensions
{
    /// <summary>
    /// The shared status, for VanillaMarkers. FxVanillaState stays its own enum: its default (0) is Same, which the
    /// bank and the project check rely on, whereas the shared enum's default is None.
    /// </summary>
    public static Common.VanillaStatus ToStatus(this FxVanillaState state) => state switch
    {
        FxVanillaState.Modified => Common.VanillaStatus.Modified,
        FxVanillaState.Added => Common.VanillaStatus.Added,
        _ => Common.VanillaStatus.Unchanged
    };
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

    /// <summary>The loaded effect (FXR3 games), null until selected.</summary>
    public FXR Fxr;

    /// <summary>The loaded effect (DES, DS2, DS2S: DLSE class streams), null until selected.</summary>
    public FFXDLSE Ffx;

    /// <summary>The loaded file object, whichever format it is: <see cref="Fxr"/> or <see cref="Ffx"/>.</summary>
    public object File => (object)Fxr ?? Ffx;

    /// <summary>
    /// The file name without its extension where the game's names don't follow f&lt;id:D9&gt; (DES and DS2 use 7
    /// digits, and DES some 9); null for the FXR3 games.
    /// </summary>
    public string NameStem;

    /// <summary>
    /// How many different contents the binders holding this id have. Vanilla FXR3 copies are identical (1), but DES
    /// keeps different effects with the same id in different binders (FFX Older Games Plan §14.2).
    /// </summary>
    public int CopyVersions = 1;

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

    public bool IsLoaded => File != null;

    /// <summary>The name the game uses for the file, e.g. f000620400 (FXR3) or f0000030 (DES, DS2).</summary>
    public string Stem => NameStem ?? $"f{Id:D9}";

    /// <summary>Whether a binder is in <see cref="Binders"/>.</summary>
    public bool InBinder(string path) => Binders.Any(b => b.Binder.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

    /// <summary>Whether saving would add or remove a file: <see cref="Binders"/> differs from <see cref="OnDisk"/>.</summary>
    public bool MembershipChanged => Binders.Count != OnDisk.Count || Binders.Any(b => !OnDisk.ContainsKey(b.Binder.Path));
}
