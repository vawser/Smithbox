namespace Hexa.NET.ImNodes
{
    using HexaGen.Runtime;
    using System.Diagnostics;

    public static class ImNodesConfig
    {
        public static bool AotStaticLink;
    }

    public static unsafe partial class ImNodes
    {
        static ImNodes()
        {
            if (ImNodesConfig.AotStaticLink)
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
            return "cimnodes";
        }
    }
}