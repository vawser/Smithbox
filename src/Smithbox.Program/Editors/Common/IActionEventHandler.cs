namespace StudioCore.Editors.Common;

/// <summary>
///     Interface for objects that may react to events caused by actions that
///     happen. Useful for invalidating caches that various editors may have.
/// </summary>
public interface IActionEventHandler
{
    public void OnActionEvent(ActionEvent evt);
}
