using Silk.NET.SDL;

namespace StudioCore.Platform;

public unsafe class WindowsPlatformUtils : PlatformUtils
{
    public WindowsPlatformUtils(Window* window) : base(window)
    {
    }
}
