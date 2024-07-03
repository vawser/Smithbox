using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class AssetBrowserTab
{
    public AssetBrowserTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("Asset Browser"))
        {
            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display aliases in browser list", ref CFG.Current.AssetBrowser_ShowAliasesInBrowser);
                ImguiUtils.ShowHoverTooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("Display tags in browser list", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
                ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("Display low-detail parts in browser list", ref CFG.Current.AssetBrowser_ShowLowDetailParts);
                ImguiUtils.ShowHoverTooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
            }

            ImGui.EndTabItem();
        }
    }
}
