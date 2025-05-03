using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editor;

public interface EditorScreen
{
    /// <summary>
    ///     Name of the editor that appears in the UI
    /// </summary>
    public string EditorName { get; }

    /// <summary>
    ///     The endpoint for editor commands that this editor responds to
    /// </summary>
    public string CommandEndpoint { get; }

    /// <summary>
    ///     Description of data that will be saved that will show in the File menu
    /// </summary>
    public string SaveType { get; }

    public bool HasDocked { get; set; }

    public string WindowName { get; }

    /// <summary>
    ///     Save specific data in this editor
    /// </summary>
    public void Save()
    {
        // Default
    }

    /// <summary>
    ///     Save all data in this editor
    /// </summary>
    public void SaveAll()
    {
        // Default
    }

    /// <summary>
    ///     Draw the main GUI using ImGUI
    /// </summary>
    /// <param name="commands">Editor specific commands from other editors</param>
    public void OnGUI(string[] commands);

    /// <summary>
    ///     Returns if the editor is capturing any input such that global input handlers should be ignored
    /// </summary>
    /// <returns>If editor is capturing any input</returns>
    public bool InputCaptured()
    {
        return false;
    }

    /// <summary>
    ///     Update any time-dependent viewport simulations
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame</param>
    public void Update(float deltaTime)
    {
        // Default
    }

    /// <summary>
    ///     Called when the window of the editor has been resized
    /// </summary>
    /// <param name="window">SDL window</param>
    /// <param name="device">Vulkan device</param>
    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        // Default
    }

    /// <summary>
    ///     Draw into any custom editor viewports
    /// </summary>
    /// <param name="device">Vulkan device</param>
    /// <param name="cl">Command list to record into</param>
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        // Default
    }

    public void OnDefocus()
    {
        // Default
    }
}
