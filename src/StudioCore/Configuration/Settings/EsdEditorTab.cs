using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class EsdEditorTab
{
    public EsdEditorTab() { }

    public void Display()
    {
        // Search Filters
        if (ImGui.CollapsingHeader("Search Filters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Propagate filter commands to all search bars", ref CFG.Current.EsdEditor_PropagateFilterCommands);
            UIHelper.Tooltip("Apply the special filter commands to all three of the search bars when used.");
        }
    }
}
