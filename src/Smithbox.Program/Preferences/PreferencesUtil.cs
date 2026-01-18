using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public static class PreferencesUtil
{
    // Some fields used in prefs that need to be filled once at session start.
    public static float TempScale;
    public static string NewThemeName = "";
    public static string CurrentThemeName = "";

    public static void Setup()
    {
        TempScale = CFG.Current.System_UI_Scale;
        CurrentThemeName = CFG.Current.SelectedTheme;
    }
}
