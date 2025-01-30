namespace StudioCore.Scene.Interfaces;

/// <summary>
///     Simple interface for a modal dialogue
/// </summary>
internal interface IModal
{
    public bool IsClosed { get; }
    public void OpenModal();
    public void OnGui();
}
