using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Renderer;

public class ResourceListTool
{
    public string ResourceListFilter = "";
    public bool ExactResourceListFilter = false;

    private ListenerTab ListenerTab;
    private MeshProviderTab MeshProviderTab;

    public ResourceListTool()
    {
        ListenerTab = new(this);
        MeshProviderTab = new(this);
    }

    public void Display(string menuId, IUniverse owner)
    {
        if (ImGui.CollapsingHeader("Resource Monitor"))
        {
            ImGui.BeginChild($"resourceTable_{menuId}", ImGuiChildFlags.Borders);

            EditorFilters.DisplayListFilter("resourceMonitor", ref ResourceListFilter, ref ExactResourceListFilter);

            ImGui.BeginTabBar("##resourceTabs");

            if (ImGui.BeginTabItem("Listeners"))
            {
                ListenerTab.Display();

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Mesh Providers"))
            {
                MeshProviderTab.Display();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();

            ImGui.EndChild();
        }
    }
}
