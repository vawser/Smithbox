using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;

namespace StudioCore.Editors.Common;

/// <summary>
/// Handles the focusing of ImGui elements within an editor.
/// Overrides the default ImGui behavior of focusing the last element defined.
/// </summary>
public class EditorFocusManager
{
    public EditorScreen Screen;

    public string TargetImGuiElement;

    public bool IsFirstFrame;

    public MapEditorContext MapEditorContext = MapEditorContext.None;

    public ModelEditorContext ModelEditorContext = ModelEditorContext.None;

    public EditorFocusManager(EditorScreen screen)
    {
        Screen = screen;
    }

    /// <summary>
    /// Reset the first frame check
    /// </summary>
    public void ResetFocus()
    {
        IsFirstFrame = true;
    }

    /// <summary>
    /// Set the focus element string to the desired ImGui ID
    /// </summary>
    public void SetDefaultFocusElement(string text)
    {
        TargetImGuiElement = text;
    }

    /// <summary>
    /// Occurs on first frame of focused editor
    /// </summary>
    public void OnFocus()
    {
        if (IsFirstFrame)
        {
            ImGui.SetWindowFocus(TargetImGuiElement);

            IsFirstFrame = false;
        }
    }

    public void SwitchMapEditorContext(MapEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            MapEditorContext = newContext;
        }
    }

    public void SwitchModelEditorContext(ModelEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            ModelEditorContext = newContext;
        }
    }
}
