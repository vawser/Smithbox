namespace StudioCore.Scene;

/// <summary>
///     An abstract object held by a render object that can be selected
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Function executed upon selection.
    /// </summary>
    public void OnSelected();

    /// <summary>
    /// Function executed upon deselection.
    /// </summary>
    public void OnDeselected();
}
