using Hexa.NET.ImGui;
using StudioCore.Resource;
using System.Collections.Generic;

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
