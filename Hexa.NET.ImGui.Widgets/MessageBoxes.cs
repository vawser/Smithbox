namespace Hexa.NET.ImGui.Widgets
{
    using System.Collections.Generic;

    /// <summary>
    /// A static class responsible for managing and displaying message boxes. Fully thread-safe.
    /// </summary>
    public static class MessageBoxes
    {
        private static readonly List<MessageBox> messageBoxes = [];

        /// <summary>
        /// Draws and updates all the open message boxes.
        /// </summary>
        public static void Draw()
        {
            lock (messageBoxes)
            {
                for (int i = 0; i < messageBoxes.Count; i++)
                {
                    MessageBox box = messageBoxes[i];
                    if (box.Draw())
                    {
                        messageBoxes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// Shows a message box to be displayed and managed by the MessageBoxes class.
        /// </summary>
        /// <param name="messageBox">The MessageBox instance to be displayed.</param>
        public static void Show(MessageBox messageBox)
        {
            lock (messageBoxes)
            {
                messageBoxes.Add(messageBox);
            }
        }

        /// <summary>
        /// Closes a specific message box by its title.
        /// </summary>
        /// <param name="title">The title of the message box to be closed.</param>
        /// <returns>True if the message box was found and closed; otherwise, false.</returns>
        public static bool Close(string title)
        {
            lock (messageBoxes)
            {
                for (int i = 0; i < messageBoxes.Count; i++)
                {
                    var box = messageBoxes[i];
                    if (box.Title == title)
                    {
                        messageBoxes.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}