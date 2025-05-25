using StudioCore.Editor;

namespace StudioCore.Scene.Interfaces;

/// <summary>
///     An abstract object held by a render object that can be selected
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Function executed upon selection.
    /// </summary>
    public void OnSelected(EditorScreen editor);

    /// <summary>
    /// Function executed upon deselection.
    /// </summary>
    public void OnDeselected(EditorScreen editor);
}
