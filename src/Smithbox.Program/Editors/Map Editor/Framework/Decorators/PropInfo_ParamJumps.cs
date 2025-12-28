using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class PropInfo_ParamJumps
{
    public static void Display(MapEditorScreen editor, Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        var scale = DPI.UIScale();
        var windowWidth = ImGui.GetWindowWidth();

        if (firstEnt.References.Count == 0)
            return;

        // Only relevant to assets
        if (editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 && firstEnt.IsPartPureAsset())
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            UIHelper.Tooltip("The current selection references these rows in params");

            if (ImGui.Button(Icons.Binoculars + "##ParamJump_ViewRef_Asset" + refID, 
                DPI.IconButtonSize))
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

                _viewport.FrameBox(box, new Vector3());
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
                        aliasName = AliasHelper.GetAssetAlias(editor.Project, modelName);
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##AssetEnvironmentGeometryParam_ParamJump" + refID, 
                    DPI.WholeWidthButton(windowWidth, 24)))
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

            if (ImGui.Button(Icons.Binoculars + "##ParamJump_ViewRef_Enemy" + refID,
                DPI.IconButtonSize))
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

                _viewport.FrameBox(box, new Vector3());
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
                        aliasName = AliasHelper.GetCharacterAlias(editor.Project, modelName);
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##ChrModelParam_ParamJump" + refID,
                    DPI.WholeWidthButton(windowWidth, 24)))
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

