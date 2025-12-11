using Hexa.NET.ImGui;
using HKLib.hk2018.hk;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Resource.Types;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource;

public static class ResourceListWindow
{
    public static string SelectedResource = "";
    public static IResourceHandle SelectedResourceHandle = null;
    public static string ResourceFilter = "";

    public static bool DisplayFilled = true;

    public static void DisplayWindow(string menuId, EditorScreen editor)
    {
        if (!ImGui.Begin($"Resource List##{menuId}"))
        {
            ImGui.End();
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();

        DPI.ApplyInputWidth(windowWidth * 0.5f);
        ImGui.InputText("##resourceTableFilter", ref ResourceFilter, 255);

        ImGui.SameLine();

        ImGui.Checkbox("Display Filled", ref DisplayFilled);
        UIHelper.Tooltip("Display filled listeners.");

        ImGui.BeginTabBar("##resourceTabs");

        if (ImGui.BeginTabItem("Listeners"))
        {
            DisplayResourceListenerTable();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Model Information"))
        {
            DisplayModelInformation(editor);
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

        ImGui.End();
    }

    public static void DisplayResourceListenerTable()
    {
        var resDatabase = ResourceManager.GetResourceDatabase();

        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        var imguiId = 0;

        if (ImGui.BeginTable($"resourceListTable", 6, tableFlags))
        {
            ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Load State", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Access Level", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Reference Count", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Unload", ImGuiTableColumnFlags.WidthStretch);

            // Header
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.TableSetColumnIndex(1);

            ImGui.Text("Name");
            UIHelper.Tooltip("Name of this resource.");

            ImGui.TableSetColumnIndex(2);

            ImGui.Text("Load State");
            UIHelper.Tooltip("The load state of this resource.");

            ImGui.TableSetColumnIndex(3);

            // Access Level
            ImGui.Text("Access Level");
            UIHelper.Tooltip("The access level of this resource.");

            ImGui.TableSetColumnIndex(4);

            // Reference Count
            ImGui.Text("Reference Count");
            UIHelper.Tooltip("The reference count for this resource.");

            ImGui.TableSetColumnIndex(5);

            // Unload

            // Contents
            foreach (var item in resDatabase)
            {
                var resName = item.Key;
                var resHandle = item.Value;

                if(!DisplayFilled)
                {
                    if(resHandle.IsLoaded())
                    {
                        continue;
                    }
                }

                if(ResourceFilter != "" && !resName.Contains(ResourceFilter))
                {
                    continue;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if(ImGui.Button($"{Icons.Bars}##{imguiId}_select", DPI.IconButtonSize))
                {
                    SelectedResource = resName;
                }
                UIHelper.Tooltip("Select this resource.");

                ImGui.TableSetColumnIndex(1);

                // Name
                ImGui.AlignTextToFramePadding();
                if(SelectedResource == resName)
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{resName}");
                }
                else
                {
                    ImGui.Text(resName);
                }

                ImGui.TableSetColumnIndex(2);

                // Load State
                if (resHandle.IsLoaded())
                {
                    ImGui.Text("Loaded");
                }
                else
                {
                    ImGui.Text("Unloaded");
                }

                ImGui.TableSetColumnIndex(3);

                // Access Level
                ImGui.Text($"{resHandle.AccessLevel}");
                ImGui.TableSetColumnIndex(4);

                // Reference Count
                ImGui.Text($"{resHandle.GetReferenceCounts()}");
                ImGui.TableSetColumnIndex(5);

                // Unload
                if (ImGui.Button($"{Icons.Times}##{imguiId}_unload", DPI.IconButtonSize))
                {
                    resHandle.Release(true);
                }
                UIHelper.Tooltip("Unload this resource.");

                imguiId++;
            }

            ImGui.EndTable();
        }
    }

    public static void DisplayModelInformation(EditorScreen editor)
    {
        if(editor is MapEditorScreen)
        {
            var mapEditor = (MapEditorScreen)editor;

            var curEntity = mapEditor.ViewportSelection.GetSelection().FirstOrDefault();
            if (curEntity == null)
                return;

            if(curEntity is MsbEntity msbEntity)
            {

            }
        }
    }
}
