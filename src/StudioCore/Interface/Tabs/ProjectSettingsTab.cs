using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Tabs;

public class ProjectSettingsTab
{
    public ProjectSettingsTab()
    {

    }

    public void Display()
    {

        if (ImGui.BeginTabItem("Settings"))
        {
            ImGui.Checkbox("Enable Recovery Folder", ref CFG.Current.System_EnableRecoveryFolder);
            ImguiUtils.ShowHoverTooltip("Enable a recovery project to be created upon an unexpected crash.");

            ImGui.Separator();

            ImGui.Checkbox("Enable Automatic Save", ref CFG.Current.System_EnableAutoSave);
            ImguiUtils.ShowHoverTooltip("All changes will be saved at the interval specificed.");

            ImGui.Text("Automatic Save Interval");
            ImguiUtils.ShowHoverTooltip("Interval in seconds between each automatic save.");

            if (ImGui.InputInt("##AutomaticSaveInterval", ref CFG.Current.System_AutoSaveIntervalSeconds))
            {
                if (CFG.Current.System_AutoSaveIntervalSeconds < 10)
                {
                    CFG.Current.System_AutoSaveIntervalSeconds = 10;
                }

                Smithbox.ProjectHandler.UpdateTimer();
            }

            ImGui.Text("Automatically Save:");
            ImguiUtils.ShowHoverTooltip("Determines which elements of Smithbox will be automatically saved, if automatic save is enabled.");

            ImGui.Indent(5.0f);

            ImGui.Checkbox("Project", ref CFG.Current.System_EnableAutoSave_Project);
            ImguiUtils.ShowHoverTooltip("The project.json will be automatically saved.");

            ImGui.Checkbox("Map Editor", ref CFG.Current.System_EnableAutoSave_MapEditor);
            ImguiUtils.ShowHoverTooltip("All loaded maps will be automatically saved.");

            ImGui.Checkbox("Model Editor", ref CFG.Current.System_EnableAutoSave_ModelEditor);
            ImguiUtils.ShowHoverTooltip("The currently loaded model will be automatically saved.");

            ImGui.Checkbox("Param Editor", ref CFG.Current.System_EnableAutoSave_ParamEditor);
            ImguiUtils.ShowHoverTooltip("All params will be automatically saved.");

            ImGui.Checkbox("Text Editor", ref CFG.Current.System_EnableAutoSave_TextEditor);
            ImguiUtils.ShowHoverTooltip("All modified text entries will be automatically saved.");

            ImGui.Checkbox("Gparam Editor", ref CFG.Current.System_EnableAutoSave_GparamEditor);
            ImguiUtils.ShowHoverTooltip("All modified gparams will be automatically saved.");

            ImGui.Unindent();

            ImGui.EndTabItem();
        }

    }
}
