using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class PropInfo_ReferencesTo
{
    public static void Display(Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.References.Count == 0)
            return;

        var scale = Smithbox.GetUIScale();

        ImGui.Separator();
        ImGui.Text("References:");
        ImGui.Separator();
        ImguiUtils.ShowHoverTooltip("The current selection references these map objects.");

        var width = (ImGui.GetWindowWidth() / 100);

        foreach (KeyValuePair<string, object[]> m in firstEnt.References)
        {
            foreach (var n in m.Value)
            {
                if (n is Entity e)
                {
                    // View Reference in Viewport
                    if (ImGui.Button(ForkAwesome.Binoculars + "##MSBRefBy" + refID, new Vector2((width * 5), 20 * scale)))
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

                        _viewport.FrameBox(box);
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
                            aliasName = AliasUtils.GetCharacterAlias(modelName);
                        }
                        if (e.IsPartAsset() || e.IsPartDummyAsset())
                        {
                            aliasName = AliasUtils.GetAssetAlias(modelName);
                        }
                        if (e.IsPartMapPiece())
                        {
                            aliasName = AliasUtils.GetMapPieceAlias(modelName);
                        }

                        if (aliasName != "")
                        {
                            displayName = displayName + " - " + aliasName;
                        }
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.Button(displayName + "##MSBRefTo" + refID, new Vector2((width * 94), 20 * scale)))
                    {
                        selection.ClearSelection();
                        selection.AddSelection(e);
                    }
                }
                else if (n is ObjectContainerReference r)
                {
                    // Try to select the map's RootObject if it is loaded, and the reference otherwise.
                    // It's not the end of the world if we choose the wrong one, as SceneTree can use either,
                    // but only the RootObject has the TransformNode and Viewport integration.
                    var mapid = r.Name;
                    var prettyName = $"{ForkAwesome.Cube} {mapid}";
                    prettyName = $"{prettyName} {AliasUtils.GetMapNameAlias(mapid)}";

                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.Button(prettyName + "##MSBRefTo" + refID, new Vector2((width * 94), 20 * scale)))
                    {
                        ISelectable rootTarget = r.GetSelectionTarget();
                        selection.ClearSelection();
                        selection.AddSelection(rootTarget);
                        // For this type of connection, jump to the object in the list to actually load the map
                        // (is this desirable in other cases?). It could be possible to have a Load context menu
                        // here, but that should be shared with SceneTree.
                        selection.GotoTreeTarget = rootTarget;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        MapContainer map = firstEnt.Universe.GetLoadedMap(mapid);
                        if (map == null)
                        {
                            if (ImGui.Selectable("Load Map"))
                            {
                                firstEnt.Universe.LoadMap(mapid);
                            }
                        }
                        else
                        {
                            if (ImGui.Selectable("Unload Map"))
                            {
                                firstEnt.Universe.UnloadContainer(map);
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

