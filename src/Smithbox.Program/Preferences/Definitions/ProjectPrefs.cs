using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class ProjectPrefs
{
    public static Type GetPrefType()
    {
        return typeof(ProjectPrefs);
    }

    #region General

    public static PreferenceItem Project_Enable_Auto_Load()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Project_Enable_Auto_Load",
            Description = "PREF_Project_Enable_Auto_Load_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Enable_Auto_Load);
            }
        };
    }
    public static PreferenceItem Project_Enable_Automatic_Auto_Load_Assignment()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Project_Enable_Automatic_Auto_Load_Assignment",
            Description = "PREF_Project_Enable_Automatic_Auto_Load_Assignment_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment);
            }
        };
    }

    public static PreferenceItem Project_Default_Mod_Directory()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_Project_Default_Mod_Directory",
            Description = "PREF_Project_Default_Mod_Directory_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Project_Default_Mod_Directory, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("DIALOG_Select")}##projectDirSelect", DPI.SelectorButtonSize))
                {
                    var newProjectPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("DIALOG_Select_Project_Directory"), out newProjectPath);

                    if (result)
                    {
                        CFG.Current.Project_Default_Mod_Directory = newProjectPath;
                    }
                }
            }
        };
    }

    public static PreferenceItem Project_Default_Data_Directory()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_Project_Default_Data_Directory",
            Description = "PREF_Project_Default_Data_Directory_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Project_Default_Data_Directory, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("DIALOG_Select")}##ProjectDataDirSelect", DPI.SelectorButtonSize))
                {
                    var newDataPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("DIALOG_Select_Data_Directory"), out newDataPath);

                    if (result)
                    {
                        CFG.Current.Project_Default_Data_Directory = newDataPath;
                    }
                }
            }
        };
    }
    public static PreferenceItem Project_Enable_Backup_Saves()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Project_Enable_Backup_Saves",
            Description = "PREF_Project_Enable_Backup_Saves_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Enable_Backup_Saves);
            }
        };
    }
    public static PreferenceItem Project_Scan_Directory_For_Additions()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Project_Scan_Directory_For_Additions",
            Description = "PREF_Project_Scan_Directory_For_Additions_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Scan_Directory_For_Additions);
            }
        };
    }
    public static PreferenceItem Project_Scan_Directory_Loose_Mode()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Project_Scan_Directory_Strict_Mode",
            Description = "PREF_Project_Scan_Directory_Strict_Mode_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Scan_Directory_Strict_Mode);
            }
        };
    }

    // TODO: enable once the Complete type has been implemented.
    //public static PreferenceItem Project_Backup_Type()
    //{
    //    return new PreferenceItem
    //    {
    //        Category = PreferenceCategory.Project,
    //        Spacer = true,

    // Section = SectionCategory.General,

    //        Title = "Backup Save Type",
    //        Description = "Determines the type of backup saving that occurs.\nSimple: backup files are placed along side source file.\nComplete: backup files are stored in a time-dated folder.",

    //        Draw = () => {
    //            var curProject = Smithbox.Orchestrator.SelectedProject;

    //            if (ImGui.BeginCombo("##inputValue", CFG.Current.Project_Backup_Type.GetDisplayName()))
    //            {
    //                foreach (var entry in Enum.GetValues(typeof(ProjectBackupBehaviorType)))
    //                {
    //                    var type = (ProjectBackupBehaviorType)entry;

    //                    if (ImGui.Selectable(type.GetDisplayName()))
    //                    {
    //                        CFG.Current.Project_Backup_Type = (ProjectBackupBehaviorType)entry;
    //                    }
    //                }
    //                ImGui.EndCombo();
    //            }
    //        }
    //    };
    //}

    #endregion

    #region Automatic Save
    public static PreferenceItem Project_Enable_Automatic_Save()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Enable_Automatic_Save",
            Description = "PREF_Project_Enable_Automatic_Save_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Enable_Automatic_Save);
            }
        };
    }

    public static PreferenceItem Project_Automatic_Save_Interval()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Automatic_Save_Interval",
            Description = "PREF_Project_Automatic_Save_Interval_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.SliderFloat("##inputValue", ref CFG.Current.Project_Automatic_Save_Interval, 5f, 3600f);
            }
        };
    }

    public static PreferenceItem ProjecT_Automatic_Save_Include_Map_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Automatic_Save_Include_Map_Editor",
            Description = "PREF_Project_Automatic_Save_Include_Map_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Automatic_Save_Include_Map_Editor);
            }
        };
    }

    public static PreferenceItem ProjecT_Automatic_Save_Include_Param_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Automatic_Save_Include_Param_Editor",
            Description = "PREF_Project_Automatic_Save_Include_Param_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Automatic_Save_Include_Param_Editor);
            }
        };
    }

    public static PreferenceItem Project_Automatic_Save_Include_Text_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Automatic_Save_Include_Text_Editor",
            Description = "PREF_Project_Automatic_Save_Include_Text_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Automatic_Save_Include_Text_Editor);
            }
        };
    }

    public static PreferenceItem Project_Automatic_Save_Include_Gparam_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Automatic_Save_Include_Gparam_Editor",
            Description = "PREF_Project_Automatic_Save_Include_Gparam_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Automatic_Save_Include_Gparam_Editor);
            }
        };
    }

    public static PreferenceItem Project_Automatic_Save_Include_Material_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = SectionCategory.AutomaticSave,

            Title = "PREF_Project_Automatic_Save_Include_Material_Editor",
            Description = "PREF_Project_Automatic_Save_Include_Material_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Automatic_Save_Include_Material_Editor);
            }
        };
    }
    #endregion

    #region Mod Engine 3
    public static PreferenceItem Project_ME3_Profile_Directory()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ModEngine3,

            Title = "PREF_Project_ME3_Profile_Directory",
            Description = "PREF_Project_ME3_Profile_Directory_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Project_ME3_Profile_Directory, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("DIALOG_Select")}##me3ProfileDir", DPI.SelectorButtonSize))
                {
                    var newDataPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("DIALOG_Select_Directory"), out newDataPath);

                    if (result)
                    {
                        CFG.Current.Project_ME3_Profile_Directory = newDataPath;
                    }
                }

            }
        };
    }
    #endregion

    #region Data Overrides
    public static PreferenceItem MapEditor_Use_PTDE_Collisions_In_DS1R_Projects()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Project,
            Spacer = true,
            DisplayRestrictions = new List<ProjectType>() { ProjectType.DS1R },

            Section = SectionCategory.DataOverride,

            Title = "PREF_MapEditor_Use_PTDE_Collisions_In_DS1R_Projects",
            Description = "PREF_MapEditor_Use_PTDE_Collisions_In_DS1R_Projects_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Use_PTDE_Collisions_In_DS1R_Projects);
            }
        };
    }
    public static PreferenceItem PTDE_Data_Path()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>() { ProjectType.DS1R },

            Section = SectionCategory.DataOverride,

            Title = "PREF_PTDE_Data_Path",
            Description = "PREF_PTDE_Data_Path_TT",

            Draw = () => {
                ImGui.InputText("##inputValue", ref CFG.Current.PTDE_Data_Path, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("DIALOG_Select")}##ptdeGameDirectorySelect", DPI.SelectorButtonSize))
                {
                    var ptdeDir = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("DIALOG_Select_Directory"), out ptdeDir);

                    if (result)
                    {
                        CFG.Current.PTDE_Data_Path = ptdeDir;
                    }
                }
            }
        };
    }

    #endregion

}
