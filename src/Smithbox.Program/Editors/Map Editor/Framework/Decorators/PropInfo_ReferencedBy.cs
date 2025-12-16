using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class PropInfo_ReferencedBy
{
    public static void Display(MapEditorScreen editor, Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.GetReferencingObjects().Count == 0)
            return;

        var scale = DPI.UIScale();

        ImGui.Separator();
        ImGui.Text("Referenced By:");
        ImGui.Separator();
        UIHelper.Tooltip("The current selection is referenced by these map objects.");

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

                if (m.IsPartEnemy() || m.IsPartDummyEnemy())
                {
                    aliasName = AliasHelper.GetCharacterAlias(editor.Project, modelName);
                }
                if (m.IsPartAsset() || m.IsPartDummyAsset())
                {
                    aliasName = AliasHelper.GetAssetAlias(editor.Project, modelName);
                }
                if (m.IsPartMapPiece())
                {
                    aliasName = AliasHelper.GetMapPieceAlias(editor.Project, modelName);
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
                selection.ClearSelection(editor);
                selection.AddSelection(editor, m);
            }

            refID++;
        }
    }
}

