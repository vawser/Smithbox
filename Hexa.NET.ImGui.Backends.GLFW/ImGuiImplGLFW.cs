namespace Hexa.NET.ImGui.Backends.GLFW
{
    using HexaGen.Runtime;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ImGuiImplGLFWConfig
    {
        public static bool AotStaticLink;
    }

    public static partial class ImGuiImplGLFW
    {
        static ImGuiImplGLFW()
        {
            if (ImGuiImplGLFWConfig.AotStaticLink)
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
                return "ImGuiImplGLFW";
            }
            return "libImGuiImplGLFW";
        }
    }
}