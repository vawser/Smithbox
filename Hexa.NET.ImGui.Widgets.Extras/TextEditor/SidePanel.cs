namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public abstract class SidePanel : IDisposable
    {
        private bool disposedValue;

        public abstract string Icon { get; }

        public abstract string Title { get; }

        public void Draw()
        {
            DrawContent();
        }

        public abstract void DrawContent();

        protected virtual void DisposeCore()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}