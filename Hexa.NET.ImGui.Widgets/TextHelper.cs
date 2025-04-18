namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;

    /// <summary>
    /// A text helper for ImGui.
    /// </summary>
    public static class TextHelper
    {
        /// <summary>
        /// Centers a text vertically.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void TextCenteredV(string text)
        {
            var windowHeight = ImGui.GetWindowSize().Y;
            var textHeight = ImGui.CalcTextSize(text).Y;

            ImGui.SetCursorPosY((windowHeight - textHeight) * 0.5f);
            ImGui.Text(text);
        }

        /// <summary>
        /// Centers a text horizontally.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void TextCenteredH(string text)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);
        }

        /// <summary>
        /// Centers a text vertically and horizontally.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void TextCenteredVH(string text)
        {
            var windowSize = ImGui.GetWindowSize();
            var textSize = ImGui.CalcTextSize(text);

            ImGui.SetCursorPos((windowSize - textSize) * 0.5f);
            ImGui.Text(text);
        }
    }
}