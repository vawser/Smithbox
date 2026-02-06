using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

public class ResourceListWindow
{
    public string SearchFilter = "";

    private ListenerTab ListenerTab;
    private MeshProviderTab MeshProviderTab;

    public ResourceListWindow()
    {
        ListenerTab = new(this);
        MeshProviderTab = new(this);
    }

    public void DisplayWindow(string menuId, IUniverse owner, bool hide = false)
    {
        if (hide)
            return;

        if (!ImGui.Begin($"Resource List##{menuId}"))
        {
            ImGui.End();
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();

        DPI.ApplyInputWidth(windowWidth * 0.5f);
        ImGui.InputText("##resourceTableFilter", ref SearchFilter, 255);

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

        ImGui.End();
    }
}
