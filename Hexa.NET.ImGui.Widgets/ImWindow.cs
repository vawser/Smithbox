namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    public delegate void SizeChangedEventHandler(object? sender, Vector2 oldSize, Vector2 size);

    public delegate void PositionChangedEventHandler(object? sender, Vector2 oldPosition, Vector2 position);

    public delegate void ViewportChangedEventHandler(object? sender, uint oldViewportId, uint viewportId);

    public abstract class ImWindow : IImGuiWindow
    {
        private bool isEmbedded;
        protected bool IsShown;
        protected bool IsDocked;
        protected bool windowEnded;
        private Vector2 size;
        private Vector2 position;
        private uint viewportId;

        public abstract string Name { get; }

        protected ImGuiWindowFlags Flags;

        public bool IsEmbedded { get => isEmbedded; protected set => isEmbedded = value; }

        bool IImGuiWindow.IsEmbedded { get => isEmbedded; set => isEmbedded = value; }

        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                ImGui.SetWindowSize(Name, size);
            }
        }

        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                ImGui.SetWindowPos(Name, position);
            }
        }

        public uint ViewportId => viewportId;

        public event SizeChangedEventHandler? SizeChanged;

        public event PositionChangedEventHandler? PositionChanged;

        public event ViewportChangedEventHandler? ViewportChanged;

        public virtual void Init()
        {
        }

        public virtual unsafe void DrawWindow(ImGuiWindowFlags overwriteFlags)
        {
            if (!IsShown) return;

            if (isEmbedded)
            {
                ImGuiWindowClass windowClass;
                windowClass.DockNodeFlagsOverrideSet = (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.NoTabBar;
                ImGui.SetNextWindowClass(&windowClass);
                ImGui.SetNextWindowDockID(WidgetManager.DockSpaceId);
            }

            var windowFlags = Flags | overwriteFlags;

            if (!ImGui.Begin(Name, ref IsShown, windowFlags))
            {
                size = Vector2.Zero; // window is closed.
                ImGui.End();
                OnClosedInternal();
                return;
            }

            if (!IsShown)
            {
                OnClosedInternal();
                ImGui.End();
                return;
            }

            if ((ImGui.GetIO().ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                var currentViewport = ImGui.GetWindowViewport().ID;

                if (viewportId != currentViewport)
                {
                    var oldViewportId = viewportId;
                    viewportId = currentViewport;
                    OnViewportChangedInternal(oldViewportId, viewportId);
                }
            }

            var currentSize = ImGui.GetWindowSize();

            if (size != currentSize)
            {
                var oldSize = size;
                size = currentSize;
                OnSizeChangedInternal(oldSize, size);
            }

            var currentPosition = ImGui.GetWindowPos();

            if (position != currentPosition)
            {
                var oldPosition = position;
                position = currentPosition;
                OnPositionChangedInternal(oldPosition, position);
            }

            windowEnded = false;

            DrawContent();

            if (!windowEnded)
                ImGui.End();
        }

        private void OnPositionChangedInternal(Vector2 oldPosition, Vector2 position)
        {
            OnPositionChanged(oldPosition, position);
            PositionChanged?.Invoke(this, oldPosition, position);
        }

        protected virtual void OnPositionChanged(Vector2 oldPosition, Vector2 position)
        {
        }

        private void OnSizeChangedInternal(Vector2 oldSize, Vector2 size)
        {
            OnSizeChanged(oldSize, size);
            SizeChanged?.Invoke(this, oldSize, size);
        }

        protected virtual void OnSizeChanged(Vector2 oldSize, Vector2 size)
        {
        }

        private void OnViewportChangedInternal(uint oldViewportId, uint viewportId)
        {
            OnViewportChanged(oldViewportId, viewportId);
            ViewportChanged?.Invoke(this, oldViewportId, viewportId);
        }

        protected virtual void OnViewportChanged(uint oldViewportId, uint viewportId)
        {
        }

        private void OnClosedInternal()
        {
            bool onClosedHandled = false;
            OnClosed(ref onClosedHandled);
            IsShown = true;
            if (!onClosedHandled)
            {
                Close();
            }
        }

        protected virtual void OnClosed(ref bool handled)
        {
        }

        public abstract void DrawContent();

        protected void EndWindow()
        {
            if (!IsShown) return;
            ImGui.End();
            windowEnded = true;
        }

        public virtual void Show()
        {
            if (IsShown) return;
            IsShown = true;
            WidgetManager.Register(this);
        }

        public virtual void Close()
        {
            if (!IsShown) return;
            WidgetManager.Unregister(this);
            IsShown = false;
        }

        public virtual void Dispose()
        {
        }
    }
}