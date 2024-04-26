using StudioCore.Interface.Windows;

namespace StudioCore.Interface;
public static class WindowContainer
{
    public static ProjectWindow ProjectWindow { get; set; }
    public static SettingsWindow SettingsWindow { get; set; }
    public static HelpWindow HelpWindow { get; set; }
    public static DebugWindow DebugWindow { get; set; }
    public static KeybindWindow KeybindWindow { get; set; }
    public static MemoryWindow MemoryWindow { get; set; }
    public static AliasWindow AliasWindow { get; set; }
    public static ColorPickerWindow ColorPickerWindow { get; set; }
}

