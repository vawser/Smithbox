namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;

    public static class DialogManager
    {
        private static readonly List<IDialog> dialogs = [];
        private static readonly Queue<IDialog> closing = [];
        private static readonly object _lock = new();

        public static void ShowDialog(IDialog dialog)
        {
            lock (_lock)
            {
                dialogs.Add(dialog);
            }
        }

        public static void CloseDialog(IDialog dialog)
        {
            lock (_lock)
            {
                closing.Enqueue(dialog);
            }
        }

        public static void Draw()
        {
            lock (_lock)
            {
                for (int i = 0; i < dialogs.Count; i++)
                {
                    bool isNotLast = i != dialogs.Count - 1;

                    ImGui.BeginDisabled(isNotLast);
                    ImGuiWindowFlags overwriteFlags = ImGuiWindowFlags.None;
                    if (isNotLast)
                    {
                        overwriteFlags |= ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBringToFrontOnFocus;
                    }

                    dialogs[i].Draw(overwriteFlags);

                    ImGui.EndDisabled();
                }

#if NETSTANDARD2_0
                
                while (closing.Count > 0)
                {
                    var dialog = closing.Dequeue();
                    dialogs.Remove(dialog);
                }
#else
                while (closing.TryDequeue(out var dialog))
                {
                    dialogs.Remove(dialog);
                }
#endif

                WidgetManager.BlockInput = dialogs.Count > 0;
            }
        }
    }
}