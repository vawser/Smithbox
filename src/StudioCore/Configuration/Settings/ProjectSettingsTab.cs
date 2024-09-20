using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class ProjectSettingsTab
{
    public ProjectSettingsTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Enable Automatic Recent Project Loading", ref CFG.Current.Project_LoadRecentProjectImmediately);
            UIHelper.ShowHoverTooltip("The last loaded project will be automatically loaded when Smithbox starts up if this is enabled.");

            ImGui.Checkbox("Enable Recovery Folder", ref CFG.Current.System_EnableRecoveryFolder);
            UIHelper.ShowHoverTooltip("Enable a recovery project to be created upon an unexpected crash.");
        }

        if (ImGui.CollapsingHeader("Automatic Save", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Enable Automatic Save", ref CFG.Current.System_EnableAutoSave);
            UIHelper.ShowHoverTooltip("All changes will be saved at the interval specificed.");

            ImGui.Text("Automatic Save Interval");
            UIHelper.ShowHoverTooltip("Interval in seconds between each automatic save.");

            if (ImGui.InputInt("##AutomaticSaveInterval", ref CFG.Current.System_AutoSaveIntervalSeconds))
            {
                if (CFG.Current.System_AutoSaveIntervalSeconds < 10)
                {
                    CFG.Current.System_AutoSaveIntervalSeconds = 10;
                }

                Smithbox.ProjectHandler.UpdateTimer();
            }

            ImGui.Text("Automatically Save:");
            UIHelper.ShowHoverTooltip("Determines which elements of Smithbox will be automatically saved, if automatic save is enabled.");

            ImGui.Checkbox("Project", ref CFG.Current.System_EnableAutoSave_Project);
            UIHelper.ShowHoverTooltip("The project.json will be automatically saved.");

            ImGui.Checkbox("Map Editor", ref CFG.Current.System_EnableAutoSave_MapEditor);
            UIHelper.ShowHoverTooltip("All loaded maps will be automatically saved.");

            ImGui.Checkbox("Model Editor", ref CFG.Current.System_EnableAutoSave_ModelEditor);
            UIHelper.ShowHoverTooltip("The currently loaded model will be automatically saved.");

            ImGui.Checkbox("Param Editor", ref CFG.Current.System_EnableAutoSave_ParamEditor);
            UIHelper.ShowHoverTooltip("All params will be automatically saved.");

            ImGui.Checkbox("Text Editor", ref CFG.Current.System_EnableAutoSave_TextEditor);
            UIHelper.ShowHoverTooltip("All modified text entries will be automatically saved.");

            ImGui.Checkbox("Gparam Editor", ref CFG.Current.System_EnableAutoSave_GparamEditor);
            UIHelper.ShowHoverTooltip("All modified gparams will be automatically saved.");
        }
    }
}
