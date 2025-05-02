using Hexa.NET.ImGui;
using StudioCore.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        if (ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var width = ImGui.GetWindowWidth();

            if (ImGui.Button("Clear Recent Project List", new Vector2(width, 24)))
            {
                var result = MessageBox.Show("This will irreversibly clear the recent projects list.", "Warning");

                if (result is DialogResult.OK)
                {
                    CFG.Current.RecentProjects = new List<CFG.RecentProject>();
                    CFG.Save();
                }
            }
            UIHelper.ShowHoverTooltip("Removes all entries in the stored Recent Project list.");
        }
    }
}
