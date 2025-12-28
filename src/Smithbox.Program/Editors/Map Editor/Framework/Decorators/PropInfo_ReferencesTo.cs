using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class PropInfo_ReferencesTo
{
    public static void Display(MapEditorScreen editor, Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.References.Count == 0)
            return;

        var scale = DPI.UIScale();

        ImGui.Separator();
        ImGui.Text("References:");
        ImGui.Separator();
        UIHelper.Tooltip("The current selection references these map objects.");

        var windowWidth = ImGui.GetWindowWidth();

        foreach (KeyValuePair<string, object[]> m in firstEnt.References)
        {
            foreach (var n in m.Value)
            {
                if (n is Entity e)
                {
                    // View Reference in Viewport
                    if (ImGui.Button(Icons.Binoculars + "##MSBRefBy" + refID, 
                        DPI.IconButtonSize))
                    {
                        BoundingBox box = new();

                        if (e.RenderSceneMesh != null)
                        {
                            box = e.RenderSceneMesh.GetBounds();
                        }
                        else if (e.Container.RootObject == e)
                        {
                            // Selection is transform node
                            Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                            Vector3 pos = e.GetLocalTransform().Position;
                            BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                            box = nodeBox;
                        }

                        _viewport.FrameBox(box, new Vector3());
                    }

                    // Change Selection to Reference
                    var displayName = $"{e.WrappedObject.GetType().Name}: {e.Name}";
                    var modelName = e.GetPropertyValue<string>("ModelName");
                    var aliasName = "";

                    if (modelName != null)
                    {
                        modelName = modelName.ToLower();

                        if (e.IsPartEnemy() || e.IsPartDummyEnemy())
                        {
                            aliasName = AliasHelper.GetCharacterAlias(editor.Project, modelName);
                        }
                        if (e.IsPartAsset() || e.IsPartDummyAsset())
                        {
                            aliasName = AliasHelper.GetAssetAlias(editor.Project, modelName);
                        }
                        if (e.IsPartMapPiece())
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
                    if (ImGui.Button(displayName + "##MSBRefTo" + refID,
                        DPI.WholeWidthButton(windowWidth, 24)))
                    {
                        selection.ClearSelection(editor);
                        selection.AddSelection(editor, e);
                    }
                }
                else if (n is ObjectContainerReference r)
                {
                    // Try to select the map's RootObject if it is loaded, and the reference otherwise.
                    // It's not the end of the world if we choose the wrong one, as SceneTree can use either,
                    // but only the RootObject has the TransformNode and Viewport integration.
                    var mapid = r.Name;
                    var prettyName = $"{Icons.Cube} {mapid}";
                    prettyName = $"{prettyName} {AliasHelper.GetMapNameAlias(editor.Project, mapid)}";

                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.Button(prettyName + "##MSBRefTo" + refID,
                        DPI.WholeWidthButton(windowWidth, 24)))
                    {
                        ISelectable rootTarget = r.GetSelectionTarget(editor);
                        selection.ClearSelection(editor);
                        selection.AddSelection(editor, rootTarget);
                        // For this type of connection, jump to the object in the list to actually load the map
                        // (is this desirable in other cases?). It could be possible to have a Load context menu
                        // here, but that should be shared with SceneTree.
                        selection.GotoTreeTarget = rootTarget;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        var universe = editor.Universe;
                        MapContainer map = editor.Selection.GetMapContainerFromMapID(mapid);
                        if (map == null)
                        {
                            if (ImGui.Selectable("Load Map"))
                            {
                                editor.Universe.LoadMap(mapid);
                            }
                        }
                        else
                        {
                            if (ImGui.Selectable("Unload Map"))
                            {
                                editor.Universe.UnloadMap(mapid);
                            }
                        }

                        ImGui.EndPopup();
                    }
                }

                refID++;
            }
        }
    }
}

