namespace Hexa.NET.ImPlot
{
    using HexaGen.Runtime;
    using System.Diagnostics;

    public static class ImPlotConfig
    {
        public static bool AotStaticLink;
    }

    public static unsafe partial class ImPlot
    {
        static ImPlot()
        {
            if (ImPlotConfig.AotStaticLink)
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
            return "cimplot";
        }
    }
}