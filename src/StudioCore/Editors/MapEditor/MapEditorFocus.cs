using Hexa.NET.ImGui;

namespace StudioCore.Editors.MapEditorNS;

/// <summary>
/// Handles the focusing of ImGui elements within an editor.
/// Overrides the default ImGui behavior of focusing the last element defined.
/// </summary>
public class MapEditorFocus
{
    public MapEditor Editor;

    public string FocusedWindowName;

    public bool ApplyFocus;

    public MapEditorContext FocusContext = MapEditorContext.None;

    public MapEditorFocus(MapEditor editor)
    {
        Editor = editor;
    }

    /// <summary>
    /// Set the focus element string to the desired ImGui ID
    /// </summary>
    public void SetFocus(string text)
    {
        ApplyFocus = true;
        FocusedWindowName = text;
    }

    /// <summary>
    /// Occurs on first frame of focused editor
    /// </summary>
    public void Update()
    {
        if (ApplyFocus)
        {
            ApplyFocus = false;
            ImGui.SetWindowFocus(FocusedWindowName);
        }
    }

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SetFocusContext(MapEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            FocusContext = newContext;
        }
    }
}

