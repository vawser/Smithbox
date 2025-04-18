namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public delegate void DialogCallback(object? sender, DialogResult result);

    [Flags]
    public enum DialogFlags
    {
        None = 1 << 0,
        CenterOnParent = 1 << 1,
        AlwaysCenter = 1 << 2
    }

    public abstract class Dialog : IDialog
    {
        private bool windowEnded;
        private bool shown;
        protected DialogCallback? callback;
        private Vector2 position;
        private Vector2 size;
        private uint viewportId;
        private IUIElement? parent;
        private DialogFlags flags;
        private bool firstFrame = true;

        public Vector2 InitialSize { get; set; } = new(1000, 600);

        public DialogResult Result { get; protected set; }

        public abstract string Name { get; }

        protected abstract ImGuiWindowFlags Flags { get; }

        public Vector2 Position => position;

        public Vector2 Size => size;

        public uint ViewportId => viewportId;

        public event SizeChangedEventHandler? SizeChanged;

        public event PositionChangedEventHandler? PositionChanged;

        public event ViewportChangedEventHandler? ViewportChanged;

        public bool Shown => shown;

        public object? Userdata { get; set; }

        public unsafe void Draw(ImGuiWindowFlags overwriteFlags)
        {
            if (!shown) return;

            var windowFlags = Flags | overwriteFlags;

            var alwaysCenter = (flags & DialogFlags.AlwaysCenter) != 0;
            var viewportsEnable = (ImGui.GetIO().ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0;
            if ((firstFrame || alwaysCenter) && (flags & DialogFlags.CenterOnParent) != 0)
            {
                Vector2 center = ImGui.GetIO().DisplaySize * 0.5f;
                if (parent != null)
                {
                    center = parent.Position + parent.Size * 0.5f;
                    if (viewportsEnable)
                    {
                        ImGui.SetNextWindowViewport(parent.ViewportId);
                    }
                }

                ImGui.SetNextWindowPos(center, alwaysCenter ? ImGuiCond.Always : ImGuiCond.Appearing, new(0.5f));
            }

            bool wasOpen = shown;
            if (!ImGui.Begin(Name, ref shown, windowFlags))
            {
                if (wasOpen && wasOpen != shown)
                {
                    Close();
                }
                ImGui.End();
                return;
            }

            if (wasOpen && wasOpen != shown)
            {
                Close();
                ImGui.End();
                return;
            }

            if (viewportsEnable)
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

            DrawContent();

            windowEnded = false;

            if (firstFrame)
            {
                ImGui.SetWindowSize(InitialSize);
                firstFrame = false;
            }

            if (!windowEnded)
            {
                ImGui.End();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EndDraw()
        {
            ImGui.End();
            windowEnded = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DrawContent();

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

        public virtual void Close()
        {
            shown = false;
            DialogManager.CloseDialog(this);
            callback?.Invoke(this, Result);
            callback = null; // clear callback afterwards to prevent memory leaks
        }

        protected virtual void Close(DialogResult result)
        {
            Result = result;
            Close();
        }

        public virtual void Reset()
        {
            Result = DialogResult.None;
        }

        public virtual void Show()
        {
            DialogManager.ShowDialog(this);
            shown = true;
        }

        public virtual void Show(DialogCallback callback)
        {
            this.callback = callback;
            Show();
        }

        public virtual void Show(DialogCallback callback, IUIElement parent, DialogFlags flags)
        {
            this.flags = flags;
            this.parent = parent;
            this.callback = callback;
            Show();
        }
    }
}