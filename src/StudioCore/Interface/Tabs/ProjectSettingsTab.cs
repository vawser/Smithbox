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

        if (ImGui.BeginTabItem("设置 Settings"))
        {
            ImGui.Checkbox("启用自动加载 Enable Automatic Recent Project Loading", ref CFG.Current.Project_LoadRecentProjectImmediately);
            ImguiUtils.ShowHoverTooltip("自动加载最后一次项目 The last loaded project will be automatically loaded when Smithbox starts up if this is enabled.");

            ImGui.Checkbox("启用恢复文件夹 Enable Recovery Folder", ref CFG.Current.System_EnableRecoveryFolder);
            ImguiUtils.ShowHoverTooltip("防止程序闪退 Enable a recovery project to be created upon an unexpected crash.");

            ImGui.Separator();

            ImGui.Checkbox("启用自动保存 Enable Automatic Save", ref CFG.Current.System_EnableAutoSave);
            ImguiUtils.ShowHoverTooltip("所有更改都将按指定的时间间隔保存 All changes will be saved at the interval specificed.");

            ImGui.Text("自动保存间隔 Automatic Save Interval");
            ImguiUtils.ShowHoverTooltip("自动保存的时间间隔 Interval in seconds between each automatic save.");

            if (ImGui.InputInt("##AutomaticSaveInterval", ref CFG.Current.System_AutoSaveIntervalSeconds))
            {
                if (CFG.Current.System_AutoSaveIntervalSeconds < 10)
                {
                    CFG.Current.System_AutoSaveIntervalSeconds = 10;
                }

                Smithbox.ProjectHandler.UpdateTimer();
            }

            ImGui.Text("自动保存 Automatically Save:");
            ImguiUtils.ShowHoverTooltip("确定如果启用了自动保存，Smithbox的哪些元素将被自动保存\nDetermines which elements of Smithbox will be automatically saved, if automatic save is enabled.");

            ImGui.Indent(5.0f);

            ImGui.Checkbox("项目 Project", ref CFG.Current.System_EnableAutoSave_Project);
            ImguiUtils.ShowHoverTooltip("project.json 将自动保存\n The project.json will be automatically saved.");

            ImGui.Checkbox("地图编辑器 Map Editor", ref CFG.Current.System_EnableAutoSave_MapEditor);
            ImguiUtils.ShowHoverTooltip("已加载的地图将自动保存\n All loaded maps will be automatically saved.");

            ImGui.Checkbox("模型编辑器 Model Editor", ref CFG.Current.System_EnableAutoSave_ModelEditor);
            ImguiUtils.ShowHoverTooltip("当前加载的模型将自动保存\n The currently loaded model will be automatically saved.");

            ImGui.Checkbox("参数编辑器 Param Editor", ref CFG.Current.System_EnableAutoSave_ParamEditor);
            ImguiUtils.ShowHoverTooltip("所有参数支持自动保存\n All params will be automatically saved.");

            ImGui.Checkbox("文本编辑器 Text Editor", ref CFG.Current.System_EnableAutoSave_TextEditor);
            ImguiUtils.ShowHoverTooltip("已修改的文本将自动保存\n All modified text entries will be automatically saved.");

            ImGui.Checkbox("G参编辑器 Gparam Editor", ref CFG.Current.System_EnableAutoSave_GparamEditor);
            ImguiUtils.ShowHoverTooltip("已修改的G参数将自动保存\n All modified gparams will be automatically saved.");

            ImGui.Unindent();

            ImGui.EndTabItem();
        }

    }
}
