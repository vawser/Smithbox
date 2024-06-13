﻿using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class PropInfo_ReferencedBy
{
    public static void Display(Entity firstEnt, IViewport _viewport, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.GetReferencingObjects().Count == 0)
            return;

        ImGui.Separator();
        ImGui.Text("Referenced By:");
        ImGui.Separator();
        ImguiUtils.ShowHoverTooltip("The current selection is referenced by these map objects.");

        var width = (ImGui.GetWindowWidth() / 100);

        foreach (Entity m in firstEnt.GetReferencingObjects())
        {
            // View Reference in Viewport
            if (ImGui.Button(ForkAwesome.Binoculars + "##MSBRefBy" + refID, new Vector2(width * 5, 20)))
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

                _viewport.FrameBox(box);
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
                    aliasName = GetAliasFromCache(modelName, ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                }
                if (m.IsPartAsset() || m.IsPartDummyAsset())
                {
                    aliasName = GetAliasFromCache(modelName, ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                }
                if (m.IsPartMapPiece())
                {
                    aliasName = GetAliasFromCache(modelName, ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"));
                }

                if (aliasName != "")
                {
                    displayName = displayName + " - " + aliasName;
                }
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);

            if (ImGui.Button(displayName + "##MSBRefBy" + refID, new Vector2(width * 94, 20)))
            {
                selection.ClearSelection();
                selection.AddSelection(m);
            }

            refID++;
        }
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
