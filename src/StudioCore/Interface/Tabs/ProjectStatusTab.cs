using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        var widthUnit = ImGui.GetWindowWidth();

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

            if(Smithbox.ProjectType is ProjectType.DS1R)
            {
                ImGui.Text($"Project Collision Directory: {CFG.Current.PTDE_Collision_Root}");
                ImGui.SameLine();
                if (ImGui.Button($@"{ForkAwesome.FileO}##collisionDirPicker"))
                {
                    if (PlatformUtils.Instance.OpenFileDialog("Select Dark Souls: Prepare to Die Edition .exe", new string[] { "exe" }, out var ptdeRoot))
                    {
                        var filename = Path.GetFileName(ptdeRoot);
                        CFG.Current.PTDE_Collision_Root = ptdeRoot.Replace(filename, "");
                        CFG.Save();
                    }
                }
                ImguiUtils.ShowHoverTooltip("This will allow collision to be visible whilst editing Dark Souls: Remastered maps.");
            }

            ImGui.Separator();

            if (ImGui.Button("Open Project.JSON", new Vector2(widthUnit * 0.5f, 32)))
            {
                var projectPath = CFG.Current.LastProjectFile;
                Process.Start("explorer.exe", projectPath);
            }
            ImGui.SameLine();
            if (ImGui.Button("Clear Recent Project List", new Vector2(widthUnit * 0.5f, 32)))
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
    }
}
