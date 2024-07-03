using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Tabs;

public class ProjectStatusTab
{
    public ProjectStatusTab()
    {

    }

    public void Display()
    {
        var widthUnit = ImGui.GetWindowWidth() / 100;

        if (ImGui.BeginTabItem("Status"))
        {
            if (Smithbox.ProjectHandler.CurrentProject == null)
            {
                ImGui.Text("No project loaded");
                ImguiUtils.ShowHoverTooltip("No project has been loaded yet.");
            }
            else if (TaskManager.AnyActiveTasks())
            {
                ImGui.Text("Waiting for program tasks to finish...");
                ImguiUtils.ShowHoverTooltip("Smithbox must finished all program tasks before it can load a project.");
            }
            else
            {
                ImGui.Text($"Project Name: {Smithbox.ProjectHandler.CurrentProject.Config.ProjectName}");
                ImGui.Text($"Project Type: {Smithbox.ProjectType}");
                ImGui.Text($"Project Root Directory: {Smithbox.GameRoot}");
                ImGui.Text($"Project Mod Directory: {Smithbox.ProjectRoot}");

                ImGui.Separator();

                if (ImGui.Button("Open Project.JSON", new Vector2(widthUnit * 48, 32)))
                {
                    var projectPath = CFG.Current.LastProjectFile;
                    Process.Start("explorer.exe", projectPath);
                }
                ImGui.SameLine();
                if (ImGui.Button("Clear Recent Project List", new Vector2(widthUnit * 48, 32)))
                {
                    CFG.Current.RecentProjects = new List<CFG.RecentProject>();
                    CFG.Save();
                }

                ImGui.Separator();

                var useLoose = Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams;
                if (Smithbox.ProjectHandler.CurrentProject.Config.GameType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3)
                {
                    if (ImGui.Checkbox("Use loose params", ref useLoose))
                        Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams = useLoose;
                    ImguiUtils.ShowHoverTooltip("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                }
            }

            ImGui.EndTabItem();
        }
    }
}
