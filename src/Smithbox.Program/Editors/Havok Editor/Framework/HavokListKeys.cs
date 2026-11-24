using Hexa.NET.ImGui;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Arrow key stepping for the Behavior Contents lists (and the Outline)
/// </summary>
public static class HavokListKeys
{
    /// <summary>
    /// Whether the window being drawn (or a child of it) has focus and nothing is being edited, so the arrow keys
    /// belong to its list. Call it while that window is current, before its list's child window.
    /// </summary>
    public static bool HasFocus()
    {
        return ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) &&
            !ImGui.IsAnyItemActive() &&
            !ImGui.GetIO().WantTextInput;
    }

    /// <summary>
    /// +1 for Down, -1 for Up, else 0 (and 0 when the list hasn't focus: see <see cref="HasFocus"/>)
    /// </summary>
    public static int GetStep()
    {
        return HasFocus() ? GUI.ListStepKey() : 0;
    }

    /// <summary>
    /// The row a step moves to, from <paramref name="current"/> (-1 for none: Down goes to the first row, Up to the
    /// last), or -1 for no move
    /// </summary>
    public static int StepIndex(int current, int count, int step)
    {
        if (step == 0 || count == 0)
            return -1;

        if (current < 0 || current >= count)
            return step > 0 ? 0 : count - 1;

        return Math.Clamp(current + step, 0, count - 1);
    }

    /// <summary>
    /// Call right after drawing the selected row of a list that isn't clipped: scrolls it into view if a step asked
    /// for it and it isn't wholly visible, and clears the request
    /// </summary>
    public static void ScrollIfRequested(ref bool request)
    {
        if (!request)
            return;

        if (!GUI.IsLastItemFullyInView())
            ImGui.SetScrollHereY();

        request = false;
    }
}
