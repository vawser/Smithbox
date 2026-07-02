using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
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
    public static PreferenceItem System_Program_Language()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_Program_Language",
            Description = "PREF_Program_Language_TT",

            Draw = () => {
                var curLanguage = LOC.CurrentLanguage();

                if (curLanguage != null)
                {
                    var previewName = LOC.Get(curLanguage.Key);

                    DPI.ApplyInputWidth();
                    if (ImGui.BeginCombo("##inputValue", previewName))
                    {
                        foreach (var entry in LOC.LanguageList)
                        {
                            var displayName = LOC.Get(entry.Key);

                            if (ImGui.Selectable(displayName))
                            {
                                Startup.Current.Program_Language = entry.Name;
                                LOC.Load(); // Refresh the localization
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
            }
        };
    }

    public static PreferenceItem System_Check_Program_Update()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_System_Check_Program_Update",
            Description = "PREF_System_Check_Program_Update_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref Startup.Current.System_Check_Program_Update);
            }
        };
    }

    public static PreferenceItem System_Ignore_Read_Asserts()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_System_Ignore_Read_Asserts",
            Description = "PREF_System_Ignore_Read_Asserts_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_Ignore_Read_Asserts);
            }
        };
    }

    public static PreferenceItem System_Apply_DCX_Heuristic()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_System_Apply_DCX_Heuristic",
            Description = "PREF_System_Apply_DCX_Heuristic_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.System_Apply_DCX_Heuristic);
            }
        };
    }

    public static PreferenceItem System_RenderingBackend()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_System_RenderingBackend",
            Description = "PREF_System_RenderingBackend_TT",

            Draw = () => {
                var previewName = LOC.Get(Startup.Current.System_RenderingBackend.GetDisplayName());

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(RenderingBackend)))
                    {
                        var type = (RenderingBackend)entry;

                        var displayName = LOC.Get(type.GetDisplayName());

                        if (ImGui.Selectable(displayName))
                        {
                            Startup.Current.System_RenderingBackend = (RenderingBackend)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }

    #endregion

    #region Loggers
    public static PreferenceItem Logger_Enable_Action_Log()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.Loggers,

            Title = "PREF_Logger_Enable_Action_Log",
            Description = "PREF_Logger_Enable_Action_Log_TT",

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

            Title = "PREF_Logger_Action_Fade_Time",
            Description = "PREF_Logger_Action_Fade_Time_TT",

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

            Title = "PREF_Logger_Enable_Warning_Log",
            Description = "PREF_Logger_Enable_Warning_Log_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Logger_Enable_Warning_Log);
            }
        };
    }

    public static PreferenceItem Logger_Warning_Fade_Time()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.System,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Loggers,

            Title = "PREF_Logger_Warning_Fade_Time",
            Description = "PREF_Logger_Warning_Fade_Time_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputInt("##inputValue", ref CFG.Current.Logger_Warning_Fade_Time);
            }
        };
    }

    public static PreferenceItem Logger_Enable_Color_Fade()
    {
        return new PreferenceItem()
        {
            OrderID = 4,
            Category = PreferenceCategory.System,
            Spacer = true,
            Section = SectionCategory.Loggers,

            Title = "PREF_Logger_Enable_Color_Fade",
            Description = "PREF_Logger_Enable_Color_Fade_TT",
            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Logger_Enable_Color_Fade);
            }
        };
    }

    public static PreferenceItem Logger_Enable_Log_Popups()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.System,
            Spacer = true,
            
            Section = SectionCategory.Loggers,
            
            Title = "PREF_Logger_Enable_Log_Popups",
            Description = "PREF_Logger_Enable_Log_Popups_TT",
            
            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Logger_Enable_Log_Popups);
            }
        };
    }
    public static PreferenceItem Logger_Enable_Scope_Logging()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.System,
            Spacer = true,

            Section = SectionCategory.Loggers,

            Title = "PREF_Logger_Enable_Scope_Logging",
            Description = "PREF_Logger_Enable_Scope_Logging_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Logger_Enable_Scope_Logging);
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

            Title = "PREF_Developer_Enable_Tools",
            Description = "PREF_Developer_Enable_Tools_TT",

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

            Title = "PREF_Developer_Smithbox_Build_Folder",
            Description = "PREF_Developer_Smithbox_Build_Folder_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Developer_Smithbox_Build_Folder, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("DIALOG_Select")}##smithboxBuildDirSelect", DPI.SelectorButtonSize))
                {
                    var smithboxBuildDir = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("DIALOG_Select_Directory"), out smithboxBuildDir);

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
