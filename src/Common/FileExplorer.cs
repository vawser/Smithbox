using System.Diagnostics;

namespace StudioCore.Common
{
    internal static class FileExplorer
    {
        public static Process Start(string path)
        {
#if WINDOWS
                return Process.Start("explorer.exe", path);
#elif MACOS
                return Process.Start("/usr/bin/open", path);
#elif LINUX
                return Process.Start("/usr/bin/xdg-open", path);
#endif
        }
    }
}