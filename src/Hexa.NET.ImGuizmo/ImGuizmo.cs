namespace Hexa.NET.ImGuizmo
{
    using HexaGen.Runtime;
    using System.Diagnostics;

    public static class ImGuizmoConfig
    {
        public static bool AotStaticLink;
    }

    public static unsafe partial class ImGuizmo
    {
        static ImGuizmo()
        {
            if (ImGuizmoConfig.AotStaticLink)
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
            return "cimguizmo";
        }
    }
}