namespace Hexa.NET.ImGui.Widgets
{
    /// <summary>
    /// Represents the different types of message boxes.
    /// </summary>
    public enum MessageBoxType
    {
        /// <summary>
        /// An "OK" button only message box.
        /// </summary>
        Ok,

        /// <summary>
        /// A message box with "OK" and "Cancel" buttons.
        /// </summary>
        OkCancel,

        /// <summary>
        /// A message box with "Yes" and "Cancel" buttons.
        /// </summary>
        YesCancel,

        /// <summary>
        /// A message box with "Yes" and "No" buttons.
        /// </summary>
        YesNo,

        /// <summary>
        /// A message box with "Yes," "No," and "Cancel" buttons.
        /// </summary>
        YesNoCancel,
    }
}