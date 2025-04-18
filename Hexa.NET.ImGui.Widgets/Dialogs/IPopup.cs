namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    public interface IPopup
    {
        string Name { get; }

        bool Shown { get; }

        void Draw();

        void Reset();

        internal void Close(bool internalValue);

        void Close();

        internal void Show(bool internalValue);

        void Show();
    }
}