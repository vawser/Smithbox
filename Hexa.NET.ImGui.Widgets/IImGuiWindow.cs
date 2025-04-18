namespace Hexa.NET.ImGui.Widgets
{
    public interface IImGuiWindow : IUIElement
    {
        string Name { get; }

        bool IsEmbedded { get; internal set; }

        void Close();

        void Dispose();

        void DrawContent();

        void DrawWindow(ImGuiWindowFlags overwriteFlags);

        void Init();

        void Show();
    }
}