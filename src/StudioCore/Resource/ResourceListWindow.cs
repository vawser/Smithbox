using HKLib.hk2018.hk;
using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.Resource.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Configuration;

namespace StudioCore.Resource;

public static class ResourceListWindow
{
    public static string SelectedResource = "";
    public static IResourceHandle SelectedResourceHandle = null;
    public static string ResourceFilter = "";
    
    public static void DisplayWindow(string menuId)
    {
        if (!ImGui.Begin($"Resource List##{menuId}"))
        {
            ImGui.End();
            return;
        }

        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();
        var tableSize = new Vector2(width * 0.98f, height * 0.98f);

        ImGui.SetNextItemWidth(width * 0.98f);
        ImGui.InputText("##resourceTableFilter", ref ResourceFilter, 255);

        // Table
        //ImGui.BeginChild("resourceTableSection", tableSize);

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

                if(ResourceFilter != "" && !resName.Contains(ResourceFilter))
                {
                    continue;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if(ImGui.Button($"{Icons.Bars}##{imguiId}_select"))
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
                if (ImGui.Button($"{Icons.Times}##{imguiId}_unload"))
                {
                    resHandle.Release(true);
                }
                UIHelper.Tooltip("Unload this resource.");

                imguiId++;
            }

            ImGui.EndTable();
        }

        //ImGui.EndChild();

        /*
        ImGui.BeginChild("resourceDetailsSection");

        if(SelectedResource == "")
        {
            ImGui.Text($"No resource selected.");
        }
        else if(SelectedResourceHandle != null)
        {
            var resHandle = SelectedResourceHandle;

            // FLVER
            if(resHandle is ResourceHandle<FlverResource>)
            {
                var resource = (ResourceHandle<FlverResource>)resHandle;
                var res = resource.Get();

            }

            // Havok Collision
            if (resHandle is ResourceHandle<HavokCollisionResource>)
            {
                var resource = (ResourceHandle<HavokCollisionResource>)resHandle;
                var res = resource.Get();

            }

            // Havok Navmesh
            if (resHandle is ResourceHandle<HavokNavmeshResource>)
            {
                var resource = (ResourceHandle<HavokNavmeshResource>)resHandle;
                var res = resource.Get();

            }

            // NVM Navmesh
            if (resHandle is ResourceHandle<NVMNavmeshResource>)
            {
                var resource = (ResourceHandle<NVMNavmeshResource>)resHandle;
                var res = resource.Get();

            }

            // Texture
            if (resHandle is ResourceHandle<TextureResource>)
            {
                var resource = (ResourceHandle<TextureResource>)resHandle;
                var res = resource.Get();

            }
        }

        ImGui.EndChild();
        */

        ImGui.End();
    }
}
