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

public static class ResourceListWindow
{
    public static string SearchFilter = "";

    public static void DisplayWindow(string menuId, EditorScreen editor)
    {
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
