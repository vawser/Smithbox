using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            OrderID = 0,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "Check for new versions of Smithbox during startup",
            Description = "When enabled Smithbox will automatically check for new versions upon program start.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_Check_Program_Update);
            }
        };
    }

    public static PreferenceItem System_Ignore_Read_Asserts()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "Ignore Read Asserts",
            Description = "If enabled, when attempting to read files, asserts will be ignored.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_Ignore_Read_Asserts);
            }
        };
    }

    public static PreferenceItem System_Apply_DCX_Heuristic()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "Use DCX Heuristic on Read",
            Description = "If enabled, if a DCX fails to read its compression type, use a heuristic to guess which it should be instead.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_Apply_DCX_Heuristic);
            }
        };
    }

    public static PreferenceItem System_RenderingBackend()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "Rendering Backend",
            Description = "Determines which rendering backend to use. Restart is required for changes to take affect.",

            Draw = () => {
                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", CFG.Current.System_RenderingBackend.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(RenderingBackend)))
                    {
                        var type = (RenderingBackend)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.System_RenderingBackend = (RenderingBackend)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }

    #endregion

    #region Loggers
    public static PreferenceItem System_ShowActionLogger()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.Loggers,

            Title = "Enable Action Log",
            Description = "If enabled, the action logger will be visible in the menu bar.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Logger_Enable_Action_Log);
            }
        };
    }

    public static PreferenceItem Logger_Action_Fade_Time()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Loggers,

            Title = "Action Visibility Duration",
            Description = "The number of frames for which the action logger message stays visible in the menu bar.\n-1 means the message never disappears.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputInt("##inputValue", ref CFG.Current.Logger_Action_Fade_Time);
            }
        };
    }

    public static PreferenceItem Logger_Enable_Warning_Log()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.Loggers,

            Title = "Enable Warning Log",
            Description = "If enabled, the warning logger will be visible in the menu bar.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Logger_Enable_Warning_Log);
            }
        };
    }

    public static PreferenceItem System_WarningLogger_FadeTime()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Loggers,

            Title = "Warning Visibility Duration",
            Description = "The number of frames for which the warning logger message stays visible in the menu bar.\n-1 means the message never disappears.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputInt("##inputValue", ref CFG.Current.Logger_Warning_Fade_Time);
            }
        };
    }

    #endregion

    #region Developer
    public static PreferenceItem Developer_Enable_Tools()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.Developer,

            Title = "Enable Developer Tools",
            Description = "Enables various tools meant for Smithbox developers only.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Developer_Enable_Tools);
            }
        };
    }

    public static PreferenceItem Developer_Smithbox_Build_Folder()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Developer,

            Title = "Smithbox Build Folder",
            Description = "Select the build directory for Smithbox (where the Smithbox.sln is placed).",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Developer_Smithbox_Build_Folder, 255);

                ImGui.SameLine();

                if (ImGui.Button("Select##smithboxBuildDirSelect", DPI.SelectorButtonSize))
                {
                    var smithboxBuildDir = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Build directory", out smithboxBuildDir);

                    if (result)
                    {
                        CFG.Current.Developer_Smithbox_Build_Folder = smithboxBuildDir;
                    }
                }
            }
        };
    }

    #endregion
}
