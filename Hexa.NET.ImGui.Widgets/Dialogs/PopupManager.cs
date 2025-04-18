namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using System.Collections.Generic;

    public static class PopupManager
    {
        private static readonly List<IPopup> popups = new();
        private static readonly object _lock = new();

        public static object SyncLock => _lock;

        public static IReadOnlyList<IPopup> Popups => popups;

        public static void Add(IPopup popup, bool show)
        {
            lock (_lock)
            {
                if (!popups.Contains(popup))
                {
                    popups.Add(popup);
                }

                if (show)
                {
                    popup.Show(true);
                }
            }
        }

        public static void Remove(IPopup popup, bool close)
        {
            lock (_lock)
            {
                int idx = popups.IndexOf(popup);
                if (idx != -1)
                {
                    return;
                }

                if (close)
                {
                    popup.Close(true);
                }
                popups.RemoveAt(idx);
            }
        }

        public static void Draw()
        {
            lock (_lock)
            {
                if (popups.Count == 0)
                {
                    return;
                }

                var popup = popups[^1];
                popup.Draw();
                if (!popup.Shown)
                {
                    popups.RemoveAt(popups.Count - 1);
                    if (popups.Count == 0)
                    {
                        return;
                    }

                    popups[^1].Show();
                }
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                for (int i = 0; i < popups.Count; i++)
                {
                    popups[i].Close();
                }
                popups.Clear();
            }
        }

        public static void Dispose()
        {
            Clear();
        }
    }
}