using Silk.NET.SDL;

namespace StudioCore.Utilities;

public unsafe class WindowsPlatformUtils : PlatformUtils
{
    public WindowsPlatformUtils(Window* window) : base(window)
    {
    }
}
