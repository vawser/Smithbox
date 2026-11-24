namespace StudioCore.Editors.Common;

/// <summary>
/// How an item (a file, an entry, an object, a field) compares with the game's own copy. Shared by every editor's
/// vanilla comparison and drawn by VanillaMarkers. Plan: Claude/Documentation/UI.md (Vanilla state).
/// </summary>
public enum VanillaStatus
{
    /// <summary>Not compared: not applicable, not worked out yet, or still being compared</summary>
    None,

    /// <summary>The container has no vanilla copy (the mod added it), or it couldn't be read</summary>
    NoVanilla,

    /// <summary>Identical to the game's copy</summary>
    Unchanged,

    /// <summary>The game has it, and the project's copy differs</summary>
    Modified,

    /// <summary>The game has no counterpart</summary>
    Added
}
