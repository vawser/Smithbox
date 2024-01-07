namespace StudioCore.Scene;

/// <summary>
///     An abstract object held by a render object that can be selected
/// </summary>
public interface ISelectable
{
    public void OnSelected();
    public void OnDeselected();
}
