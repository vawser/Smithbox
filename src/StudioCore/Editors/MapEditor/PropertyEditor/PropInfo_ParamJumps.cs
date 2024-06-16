using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class PropInfo_ParamJumps
{
    public static void Display(Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        var scale = Smithbox.GetUIScale();
        var width = (ImGui.GetWindowWidth() / 100);

        if (firstEnt.References.Count == 0)
            return;

        if (!CFG.Current.MapEditor_Enable_Param_Quick_Links)
            return;

        // Only relevant to assets
        if ( (Project.Type is ProjectType.ER or ProjectType.AC6 ) && firstEnt.IsPartPureAsset())
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            ImguiUtils.ShowHoverTooltip("The current selection references these rows in params");

            if (ImGui.Button(ForkAwesome.Binoculars + "##ParamJump_ViewRef_Asset" + refID, new Vector2((width * 5), 20 * scale)))
            {
                BoundingBox box = new();

                if (firstEnt.RenderSceneMesh != null)
                {
                    box = firstEnt.RenderSceneMesh.GetBounds();
                }
                else if (firstEnt.Container.RootObject == firstEnt)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = firstEnt.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    box = nodeBox;
                }

                _viewport.FrameBox(box);
            }

            if (firstEnt is Entity e)
            {
                // Jump to AssetEnvironmentGeometryParam param row
                var displayName = $"AssetEnvironmentGeometryParam: {e.Name}";
                var modelName = e.GetPropertyValue<string>("ModelName");
                var aliasName = "";
                var assetRowId = GetAssetEnvironmentGeometryParamRow(modelName);

                if (modelName != null)
                {
                    modelName = modelName.ToLower();

                    if (e.IsPartAsset() || e.IsPartDummyAsset())
                    {
                        aliasName = GetAliasFromCache(modelName, ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##AssetEnvironmentGeometryParam_ParamJump" + refID, new Vector2((width * 94), 20 * scale)))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/AssetEnvironmentGeometryParam/{assetRowId}");
                }
            }
        }

        // Only relevant to characters
        if ((Project.Type is ProjectType.ER or ProjectType.AC6) 
            && (firstEnt.IsPartEnemy() || firstEnt.IsPartDummyEnemy() ) )
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            ImguiUtils.ShowHoverTooltip("The current selection references these rows in params");

            if (ImGui.Button(ForkAwesome.Binoculars + "##ParamJump_ViewRef_Enemy" + refID, new Vector2((width * 5), 20 * scale)))
            {
                BoundingBox box = new();

                if (firstEnt.RenderSceneMesh != null)
                {
                    box = firstEnt.RenderSceneMesh.GetBounds();
                }
                else if (firstEnt.Container.RootObject == firstEnt)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = firstEnt.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    box = nodeBox;
                }

                _viewport.FrameBox(box);
            }

            if (firstEnt is Entity e)
            {
                // Jump to AssetEnvironmentGeometryParam param row
                var displayName = $"ChrModelParam : {e.Name}";
                var modelName = e.GetPropertyValue<string>("ModelName");
                var aliasName = "";
                var modelRowId = GetChrModelParamRow(modelName);

                if (modelName != null)
                {
                    modelName = modelName.ToLower();

                    if (e.IsPartEnemy() || e.IsPartDummyEnemy())
                    {
                        aliasName = GetAliasFromCache(modelName, ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##ChrModelParam_ParamJump" + refID, new Vector2((width * 94), 20 * scale)))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/ChrModelParam/{modelRowId}");
                }
            }
        }
    }

    private static string GetAssetEnvironmentGeometryParamRow(string modelName)
    {
        string assetId = modelName.Replace("AEG", "").Replace("_", "");
        return assetId;
    }

    private static string GetChrModelParamRow(string modelName)
    {
        string chrId = modelName.Replace("c", "");
        return chrId;
    }

    public static string GetAliasFromCache(string name, List<AliasReference> referenceList)
    {
        foreach (var alias in referenceList)
        {
            if (name == alias.id)
            {
                return alias.name;
            }
        }

        return "";
    }
}

