using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource;

public static class ResourceListWindow
{
    public static void DisplayWindow(string menuId)
    {
        if (!ImGui.Begin($"Resource List##{menuId}"))
        {
            ImGui.End();
            return;
        }

        ImGui.Checkbox("Display Resource Loading Window", ref UI.Current.System_DisplayResourceLoadingWindow);
        UIHelper.ShowHoverTooltip("Toggles the appearance of the map resource loading window during the map load process");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("List of Resources Loaded & Unloaded");

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button("Unload All"))
        {
            foreach (KeyValuePair<string, IResourceHandle> item in ResourceManager.GetResourceDatabase())
            {
                item.Value.Release(true);
            }
        }

        ImGui.Columns(5);
        ImGui.Separator();
        var id = 0;

        foreach (KeyValuePair<string, IResourceHandle> item in ResourceManager.GetResourceDatabase())
        {
            if (item.Key == "")
                continue;

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Key);
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Value.IsLoaded() ? "Loaded" : "Unloaded");
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Value.AccessLevel.ToString());
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Value.GetReferenceCounts().ToString());
            ImGui.NextColumn();
            if (ImGui.Button("Unload"))
            {
                item.Value.Release(true);
            }
            ImGui.NextColumn();
            ImGui.PopID();
            id++;
        }

        ImGui.Columns(1);
        ImGui.Separator();
        ImGui.End();
    }
}
