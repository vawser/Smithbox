namespace Hexa.NET.ImGui.Backends.SDL2
{
    using HexaGen.Runtime;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ImGuiImplSDL2Config
    {
        public static bool AotStaticLink;
    }

    public static partial class ImGuiImplSDL2
    {
        static ImGuiImplSDL2()
        {
            if (ImGuiImplSDL2Config.AotStaticLink)
            {
                InitApi(new NativeLibraryContext(Process.GetCurrentProcess().MainModule!.BaseAddress));
            }
            else
            {
                InitApi(new NativeLibraryContext(LibraryLoader.LoadLibrary(GetLibraryName, null)));
            }
        }

        public static string GetLibraryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "ImGuiImplSDL2";
            }
            return "libImGuiImplSDL2";
        }
    }
}