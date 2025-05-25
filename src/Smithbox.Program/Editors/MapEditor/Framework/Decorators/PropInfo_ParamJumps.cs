using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using StudioCore.ViewportNS;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.Framework.Decorators;

public static class PropInfo_ParamJumps
{
    public static void Display(MapEditorScreen editor, Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        var scale = DPI.GetUIScale();
        var width = ImGui.GetWindowWidth() / 100;

        if (firstEnt.References.Count == 0)
            return;

        // Only relevant to assets
        if (editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 && firstEnt.IsPartPureAsset())
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            UIHelper.Tooltip("The current selection references these rows in params");

            if (ImGui.Button(Icons.Binoculars + "##ParamJump_ViewRef_Asset" + refID, new Vector2(width * 5, 20 * scale)))
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
                        aliasName = AliasUtils.GetAssetAlias(editor.Project, modelName);
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##AssetEnvironmentGeometryParam_ParamJump" + refID, new Vector2(width * 94, 20 * scale)))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/AssetEnvironmentGeometryParam/{assetRowId}");
                }
            }
        }

        // Only relevant to characters
        if (editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6
            && (firstEnt.IsPartEnemy() || firstEnt.IsPartDummyEnemy()))
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            UIHelper.Tooltip("The current selection references these rows in params");

            if (ImGui.Button(Icons.Binoculars + "##ParamJump_ViewRef_Enemy" + refID, new Vector2(width * 5, 20 * scale)))
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
                        aliasName = AliasUtils.GetCharacterAlias(editor.Project, modelName);
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##ChrModelParam_ParamJump" + refID, new Vector2(width * 94, 20 * scale)))
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
}

