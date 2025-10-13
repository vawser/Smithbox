using System;
using System.IO;

namespace Common
{
    internal static class FileLocations
    {
#if MACOS
                                      // NSBundle.MainBundle.ResourcePath
        public static readonly string Resources = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../Resources"));
        public static readonly string Assets = Path.Combine(Resources, "Assets");
        public static readonly string Res = Path.Combine(Resources, "Res");
        public static readonly string CurImgui = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smithbox"));
        public static readonly string StoreImgui = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "../Preferences/Smithbox"));
#else
        public static readonly string Resources = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string Assets = Path.Combine(Resources, "Assets");
        public static readonly string Res = Path.Combine(Resources, "Res");
        public static readonly string CurImgui = Resources;
        public static readonly string StoreImgui = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smithbox"));
#endif
    }
}
