#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The layout the Animation Editor's tools share (UI.md §2.3): a header per group, a two-column form (labels in a
/// fixed column, inputs filling the rest) and full-width buttons whose labels say what they do.
/// </summary>
public static class AnimationToolLayout
{
    /// <summary>A group: a little space, then its header</summary>
    public static void Group(string title, string tooltip)
    {
        GUI.Spacer();
        GUI.SimpleHeader(title, tooltip);
    }

    /// <summary>A two-column form: labels in a fixed column, inputs filling the rest. End it with <c>ImGui.EndTable</c>.</summary>
    public static bool BeginForm(string id)
    {
        if (!ImGui.BeginTable(id, 2, ImGuiTableFlags.SizingStretchProp))
            return false;

        ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthFixed, 110 * DPI.UIScale());
        ImGui.TableSetupColumn("##input", ImGuiTableColumnFlags.WidthStretch);
        return true;
    }

    /// <summary>A form row: the label, then the next item set to fill its column</summary>
    public static void FormRow(string label, string tooltip)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
        if (tooltip.Length > 0)
            GUI.Tooltip(tooltip);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
    }

    /// <summary>A full-width button, its tooltip shown when disabled too</summary>
    public static bool WideButton(string label, string id, bool enabled, string tooltip)
    {
        ImGui.BeginDisabled(!enabled);
        var pressed = ImGui.Button($"{label}##{id}", new Vector2(-1, 0));
        ImGui.EndDisabled();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && tooltip.Length > 0)
            ImGui.SetTooltip(tooltip);
        return pressed && enabled;
    }

    /// <summary>
    /// Buttons side by side sharing the width evenly (for short, related actions such as Add / Duplicate / Remove).
    /// Returns the index pressed, -1 for none.
    /// </summary>
    public static int ButtonRow(string id, params (string Label, bool Enabled, string Tooltip)[] buttons)
    {
        var pressed = -1;
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var width = (ImGui.GetContentRegionAvail().X - spacing * (buttons.Length - 1)) / buttons.Length;
        for (var i = 0; i < buttons.Length; i++)
        {
            if (i > 0)
                ImGui.SameLine();

            var (label, enabled, tooltip) = buttons[i];
            ImGui.BeginDisabled(!enabled);
            if (ImGui.Button($"{label}##{id}_{i}", new Vector2(width, 0)) && enabled)
                pressed = i;
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && tooltip.Length > 0)
                ImGui.SetTooltip(tooltip);
        }

        return pressed;
    }
}
