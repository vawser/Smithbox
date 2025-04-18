namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    public interface IDialog : IUIElement
    {
        bool Shown { get; }

        void Draw(ImGuiWindowFlags overwriteFlags);

        void Close();

        void Reset();

        void Show();
    }
}