namespace Hexa.NET.ImGui.Backends.SDL3
{
    using HexaGen.Runtime;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ImGuiImplSDL3Config
    {
        public static bool AotStaticLink;
    }

    public static partial class ImGuiImplSDL3
    {
        static ImGuiImplSDL3()
        {
            if (ImGuiImplSDL3Config.AotStaticLink)
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
                return "ImGuiImplSDL3";
            }
            return "libImGuiImplSDL3";
        }
    }
}