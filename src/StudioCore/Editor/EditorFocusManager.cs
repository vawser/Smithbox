using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editor;

/// <summary>
/// Handles the focusing of ImGui elements within an editor.
/// Overrides the default ImGui behavior of focusing the last element defined.
/// </summary>
public class EditorFocusManager
{
    public EditorScreen Screen;

    public string TargetImGuiElement;

    public bool IsFirstFrame;

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
}
