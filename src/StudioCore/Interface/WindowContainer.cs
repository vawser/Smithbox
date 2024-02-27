using StudioCore.Interface.Windows;

namespace StudioCore.Interface;
public static class WindowContainer
{
    public static SettingsWindow SettingsWindow { get; set; }
    public static HelpWindow HelpWindow { get; set; }
    public static EventFlagWindow EventFlagWindow { get; set; }
    public static DebugWindow DebugWindow { get; set; }
    public static MapNameWindow MapNameWindow { get; set; }
    public static MapGroupWindow MapGroupWindow { get; set; }
    public static KeybindWindow KeybindWindow { get; set; }
}

