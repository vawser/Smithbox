using ImGuiNET;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Development;

public static class ResourceManagerWindow
{
    public static void Display()
    {
        if (ImGui.CollapsingHeader("Resources"))
        {
            ImGui.PushID("Resources");

            foreach (KeyValuePair<string, IResourceHandle> entry in ResourceManager.GetResourceDatabase())
            {
                var open = ImGui.TreeNodeEx(entry.Key, ImGuiTreeNodeFlags.DefaultOpen);
                if (open)
                {
                    ResourceHandlerInformation(entry.Value);
                }

                ImGui.TreePop();
            }

            ImGui.PopID();
        }
    }

    public static void ResourceHandlerInformation(IResourceHandle handler)
    {
        ImGui.Text($"{handler.GetType()}");
    }
}
