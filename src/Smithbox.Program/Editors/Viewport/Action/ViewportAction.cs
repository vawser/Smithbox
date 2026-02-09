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
    public abstract string GetEditMessage();
}