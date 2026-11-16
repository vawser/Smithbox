namespace StudioCore.Editors.Common;

/// <summary>
///     An action that can be performed by the user in the editor that represents
///     a single atomic editor action that affects the state of the map. Each action
///     should have enough information to apply the action AND undo the action, as
///     these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class EditorAction
{
    public abstract ActionEvent Execute();
    public abstract ActionEvent Undo();

    /// <summary>
    /// Optional name for undo labels and history lists (e.g. "Mass Edit: selection: weight: * 2"). When null, an
    /// editor names the action from its type.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Optional longer text for a tooltip in a history list (e.g. a mass edit's full script)
    /// </summary>
    public string Details { get; set; }
}
