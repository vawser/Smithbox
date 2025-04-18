namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;

    /// <summary>
    /// A tooltip helper for ImGui
    /// </summary>
    public static class TooltipHelper
    {
        /// <summary>
        /// Shows a tooltip if the item is hovered with <paramref name="desc"/> as text.
        /// </summary>
        /// <param name="desc">The text of the tooltip.</param>
        public static void Tooltip(string desc)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}