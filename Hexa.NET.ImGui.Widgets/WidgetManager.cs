namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using System.Numerics;

    public static class WidgetManager
    {
        private static bool initialized;
        private static readonly List<IImGuiWindow> widgets = new();

        static WidgetManager()
        {
        }

        public static bool BlockInput { get; set; }

        public static uint DockSpaceId { get; private set; }

        public static WidgetStyle Style { get; set; } = new();

        public static IReadOnlyList<IImGuiWindow> Widgets => widgets;

        public static T? Find<T>(string? id = null) where T : IImGuiWindow
        {
            foreach (var widget in widgets)
            {
                if (id != null && widget.Name != id) continue;
                if (widget is T t)
                {
                    return t;
                }
            }
            return default;
        }

        internal static bool Register<T>(bool mainWindow = false) where T : IImGuiWindow, new()
        {
            return Register(new T(), mainWindow);
        }

        internal static void Unregister<T>() where T : IImGuiWindow, new()
        {
            IImGuiWindow? window = widgets.FirstOrDefault(x => x is T);
            if (window != null)
            {
                Unregister(window);
            }
        }

        internal static void Unregister(IImGuiWindow window)
        {
            if (initialized)
            {
                window.Dispose();
            }

            widgets.Remove(window);
        }

        internal static bool Register(IImGuiWindow widget, bool mainWindow = false)
        {
            if (widgets.Count == 0)
            {
                widget.IsEmbedded = true;
            }

            if (mainWindow)
            {
                widget.IsEmbedded = true;
                for (int i = 0; i < widgets.Count; i++)
                {
                    widgets[i].IsEmbedded = false;
                }
            }

            if (!initialized)
            {
                widgets.Add(widget);
                return false;
            }
            else
            {
                widget.Init();
                widgets.Add(widget);
                return true;
            }
        }

        public static void Init()
        {
            for (int i = 0; i < widgets.Count; i++)
            {
                var widget = widgets[i];
                widget.Init();
            }
            ImGuiGC.Init();
            initialized = true;
        }

        public static void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
            DockSpaceId = ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null); // passing null as first argument will use the main viewport
            ImGui.PopStyleColor(1);

            ImGui.BeginDisabled(BlockInput);

            ImGuiWindowFlags overwriteFlags = ImGuiWindowFlags.None;
            if (BlockInput)
            {
                overwriteFlags |= ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBringToFrontOnFocus;
            }

            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].DrawWindow(overwriteFlags);
            }

            ImGui.EndDisabled();

            DialogManager.Draw();
            MessageBoxes.Draw();
            PopupManager.Draw();
            AnimationManager.Tick();
        }

        public static void Dispose()
        {
            AnimationManager.Release();
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].Dispose();
            }
            widgets.Clear();
            ImGuiGC.Shutdown();
            initialized = false;
        }
    }
}