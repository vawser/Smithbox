namespace Hexa.NET.ImGui.Widgets
{
    using System.Numerics;

    public interface IUIElement
    {
        public Vector2 Position { get; }

        public Vector2 Size { get; }

        public uint ViewportId { get; }
    }
}