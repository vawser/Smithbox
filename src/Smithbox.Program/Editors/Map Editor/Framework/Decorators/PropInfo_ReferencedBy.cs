using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class PropInfo_ReferencedBy
{
    public static void Display(MapEditorView view, Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.GetReferencingObjects().Count == 0)
            return;

        var scale = DPI.UIScale();

        if (!MapPropertySections.Header(MapPropertySections.ReferencedByKey,
            LOC.Get("MAP_Properties_Referenced_By_Header"), LOC.Get("MAP_Properties_Referenced_By_Header_TT"),
            firstEnt.GetReferencingObjects().Count))
            return;

        var windowWidth = ImGui.GetWindowWidth();

        foreach (Entity m in firstEnt.GetReferencingObjects())
        {
            // View Reference in Viewport
            if (ImGui.Button(Icons.Binoculars + "##MSBRefBy" + refID, 
                DPI.IconButtonSize))
            {
                BoundingBox box = new();

                if (m.RenderSceneMesh != null)
                {
                    box = m.RenderSceneMesh.GetBounds();
                }
                else if (m.Container.RootObject == m)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = m.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    box = nodeBox;
                }

                _viewport.FrameBox(box, new Vector3());
            }

            // Change Selection to Reference
            var displayName = $"{m.WrappedObject.GetType().Name}: {m.Name}";
            var modelName = m.GetPropertyValue<string>("ModelName");
            var aliasName = "";

            if (modelName != null)
            {
                modelName = modelName.ToLower();

                if (EntityHelper.IsPartEnemy(m) || EntityHelper.IsPartDummyEnemy(m))
                {
                    aliasName = AliasHelper.GetCharacterAlias(view.Project, modelName);
                }
                if (EntityHelper.IsPartAsset(m) || EntityHelper.IsPartDummyAsset(m))
                {
                    aliasName = AliasHelper.GetAssetAlias(view.Project, modelName);
                }
                if (EntityHelper.IsPartMapPiece(m))
                {
                    aliasName = AliasHelper.GetMapPieceAlias(view.Project, modelName);
                }

                if (aliasName != "")
                {
                    displayName = displayName + " - " + aliasName;
                }
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);

            if (ImGui.Button(displayName + "##MSBRefBy" + refID,
                DPI.WholeWidthButton(windowWidth, 24)))
            {
                selection.ClearSelection();
                selection.AddSelection(m);
            }

            refID++;
        }
    }
}

