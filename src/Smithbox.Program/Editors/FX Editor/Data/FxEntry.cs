using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.FxEditor;

/// <summary>Where an effect is stored: a binder and the FXR's name inside it.</summary>
public record FxBinderRef(FileDictionaryEntry Binder, string InternalName);

/// <summary>
/// One effect id. Vanilla stores identical copies of an effect in every binder that needs it, so the editor
/// edits the effect once and saves it to all of them (FX Editor Plan, decision 1).
/// </summary>
public class FxEntry
{
    public int Id;

    /// <summary>Every binder holding this effect, in binder path order. The first is read from.</summary>
    public List<FxBinderRef> Binders = new();

    /// <summary>The loaded effect, null until selected.</summary>
    public FXR Fxr;

    /// <summary>
    /// The effect's file bytes as last read or saved, deflated. Loading and reloading unpack this, so no binder
    /// has to be decompressed when an effect is selected.
    /// </summary>
    public byte[] Packed;
    public int PackedLength;

    public bool IsDirty;

    /// <summary>The Particles alias name, or empty.</summary>
    public string Alias = "";

    public bool IsLoaded => Fxr != null;

    /// <summary>The name the game uses for the file, e.g. f000620400.</summary>
    public string Stem => $"f{Id:D9}";
}
