using Hexa.NET.ImGui;
using StudioCore.Application;
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
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "General",

            Title = "Enable Project Auto-Load",
            Description = "If enabled, loading a project will set it to be automatically loaded when Smithbox starts.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Project_Enable_Auto_Load);
            }
        };
    }

    public static PreferenceItem DefaultModDirectory()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = "General",

            Title = "Default Project Directory",
            Description = "The default directory to use during the project directory selection when creating a new project.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.DefaultModDirectory, 255);

                ImGui.SameLine();

                if (ImGui.Button("Select##projectDirSelect", DPI.SelectorButtonSize))
                {
                    var newProjectPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath);

                    if (result)
                    {
                        CFG.Current.DefaultModDirectory = newProjectPath;
                    }
                }
            }
        };
    }

    public static PreferenceItem DefaultDataDirectory()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = "General",

            Title = "Default Data Directory",
            Description = "The default directory to use during the data directory selection when creating a new project.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.DefaultDataDirectory, 255);

                ImGui.SameLine();

                if (ImGui.Button("Select##ProjectDataDirSelect", DPI.SelectorButtonSize))
                {
                    var newDataPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Data Directory", out newDataPath);

                    if (result)
                    {
                        CFG.Current.DefaultDataDirectory = newDataPath;
                    }
                }
            }
        };
    }
    #endregion

    #region Automatic Save
    public static PreferenceItem EnableAutomaticSave()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "Automatic Save",

            Title = "Enable Automatic Save",
            Description = "If enabled, all enabled editors will automatically save.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.EnableAutomaticSave);
            }
        };
    }

    public static PreferenceItem AutomaticSaveIntervalTime()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = "Automatic Save",

            Title = "Automatic Save Interval",
            Description = "The rate at which the automatic save occurs. In seconds.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.SliderFloat("##inputValue", ref CFG.Current.AutomaticSaveIntervalTime, 5f, 3600f);
            }
        };
    }

    public static PreferenceItem AutomaticSave_MapEditor()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "Automatic Save",

            Title = "Include Map Editor",
            Description = "If enabled, the Map Editor is automatically saved.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.AutomaticSave_MapEditor);
            }
        };
    }

    public static PreferenceItem AutomaticSave_ParamEditor()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "Automatic Save",

            Title = "Include Param Editor",
            Description = "If enabled, the Param Editor is automatically saved.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.AutomaticSave_ParamEditor);
            }
        };
    }

    public static PreferenceItem AutomaticSave_TextEditor()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "Automatic Save",

            Title = "Include Text Editor",
            Description = "If enabled, the Text Editor is automatically saved.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.AutomaticSave_TextEditor);
            }
        };
    }

    public static PreferenceItem AutomaticSave_GparamEditor()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "Automatic Save",

            Title = "Include Graphics Param Editor",
            Description = "If enabled, the Graphics Param Editor is automatically saved.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.AutomaticSave_GparamEditor);
            }
        };
    }

    public static PreferenceItem AutomaticSave_MaterialEditor()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,

            Section = "Automatic Save",

            Title = "Include Material Editor",
            Description = "If enabled, the Material Editor is automatically saved.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.AutomaticSave_MaterialEditor);
            }
        };
    }
    #endregion

    #region Mod Engine 3
    public static PreferenceItem ModEngine3ProfileDirectory()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Project,
            Spacer = true,
            InlineName = false,

            Section = "Mod Engine 3",

            Title = "ME3 Profile Directory",
            Description = "Select the directory you want the generated ME3 profiles to be placed in.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.ModEngine3ProfileDirectory, 255);

                ImGui.SameLine();

                if (ImGui.Button("Select##me3ProfileDir", DPI.SelectorButtonSize))
                {
                    var newDataPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Directory", out newDataPath);

                    if (result)
                    {
                        CFG.Current.ModEngine3ProfileDirectory = newDataPath;
                    }
                }

            }
        };
    }
    #endregion
}
