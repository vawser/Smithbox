namespace Hexa.NET.ImGuiNodeEditor
{
    using HexaGen.Runtime;
    using System.Diagnostics;

    public static class ImGuiNodeEditorConfig
    {
        public static bool AotStaticLink;
    }

    public static partial class ImGuiNodeEditor
    {
        static ImGuiNodeEditor()
        {
            if (ImGuiNodeEditorConfig.AotStaticLink)
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
            return "cimguinodeeditor";
        }
    }
}