using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class SystemPrefs
{
    public static Type GetPrefType()
    {
        return typeof(SystemPrefs);
    }

    #region General
    public static PreferenceItem System_Check_Program_Update()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = "General",

            Title = "Check for new versions of Smithbox during startup",
            Description = "When enabled Smithbox will automatically check for new versions upon program start.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_Check_Program_Update);
            }
        };
    }

    public static PreferenceItem System_IgnoreAsserts()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = "General",

            Title = "Ignore Read Asserts",
            Description = "If enabled, when attempting to read files, asserts will be ignored.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_IgnoreAsserts);
            }
        };
    }

    public static PreferenceItem System_UseDCXHeuristicOnReadFailure()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = "General",

            Title = "Use DCX Heuristic on Read",
            Description = "If enabled, if a DCX fails to read its compression type, use a heuristic to guess which it should be instead.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_UseDCXHeuristicOnReadFailure);
            }
        };
    }

    #endregion

    #region Loggers
    public static PreferenceItem System_ShowActionLogger()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = "Loggers",

            Title = "Show Action Logger",
            Description = "If enabled, the action logger will be visible in the menu bar.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_ShowActionLogger);
            }
        };
    }

    public static PreferenceItem System_ActionLogger_FadeTime()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = "Loggers",

            Title = "Action Log Visibility Duration",
            Description = "The number of frames for which the action logger message stays visible in the menu bar.\n-1 means the message never disappears.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputInt("##inputValue", ref CFG.Current.System_ActionLogger_FadeTime);
            }
        };
    }

    public static PreferenceItem System_ShowWarningLogger()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = "Loggers",

            Title = "Show Warning Logger",
            Description = "If enabled, the warning logger will be visible in the menu bar.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_ShowWarningLogger);
            }
        };
    }

    public static PreferenceItem System_WarningLogger_FadeTime()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = "Loggers",

            Title = "Warning Log Visibility Duration",
            Description = "The number of frames for which the warning logger message stays visible in the menu bar.\n-1 means the message never disappears.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputInt("##inputValue", ref CFG.Current.System_WarningLogger_FadeTime);
            }
        };
    }

    #endregion

    #region Developer
    public static PreferenceItem EnableDeveloperTools()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = "Developer",

            Title = "Enable Developer Tools",
            Description = "Enables various tools meant for Smithbox developers only.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.EnableDeveloperTools);
            }
        };
    }

    public static PreferenceItem SmithboxBuildFolder()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = "Developer",

            Title = "Smithbox Build Folder",
            Description = "Select the build directory for Smithbox (where the Smithbox.sln is placed).",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.SmithboxBuildFolder, 255);

                ImGui.SameLine();

                if (ImGui.Button("Select##smithboxBuildDirSelect", DPI.SelectorButtonSize))
                {
                    var smithboxBuildDir = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Build directory", out smithboxBuildDir);

                    if (result)
                    {
                        CFG.Current.SmithboxBuildFolder = smithboxBuildDir;
                    }
                }
            }
        };
    }

    #endregion
}
