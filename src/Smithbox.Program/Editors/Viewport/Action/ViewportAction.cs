using System.Collections.Generic;

namespace StudioCore.Editors.Common;

/// <summary>
///     An action that can be performed by the user in the editor that represents
///     a single atomic editor action that affects the state of the map. Each action
///     should have enough information to apply the action AND undo the action, as
///     these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class ViewportAction
{
    public abstract ActionEvent Execute(bool isRedo = false);
    public abstract ActionEvent Undo();

    /// <summary>
    /// What this action changed (in the Map Editor, the IDs of the maps), recorded while it ran through
    /// a ViewportActionManager, so the history of one of them can be dropped without the rest.
    /// </summary>
    public HashSet<string> TouchedScopes { get; } = new();
}
