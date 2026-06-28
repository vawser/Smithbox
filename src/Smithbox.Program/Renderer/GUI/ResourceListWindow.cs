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
        if (ImGui.CollapsingHeader($"{LOC.Get("REND_Tool_Resource_Monitor")}##resourceMonitorTool"))
        {
            EditorFilters.DisplayFramedListFilter($"resourceMonitor_{menuId}", ref ResourceListFilter, ref ExactResourceListFilter);

            ImGui.BeginChild($"resourceTable_{menuId}", ImGuiChildFlags.Borders);

            ImGui.BeginTabBar("##resourceTabs");

            if (ImGui.BeginTabItem($"{LOC.Get("REND_Tool_Listener_Tab")}##listenerTab"))
            {
                ListenerTab.Display();

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("REND_Tool_Mesh_Provider_Tab")}##meshProviderTab"))
            {
                MeshProviderTab.Display();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();

            ImGui.EndChild();
        }
    }
}
