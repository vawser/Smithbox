using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Models tab's Generate group (Navigation Editor Plan, Phase 4; Navmesh Generation Plan): generates a model's n and
/// o navmeshes from collision models of the map, in place of the selected model's or into a free placeholder, as one
/// undo. The generation runs in the background; the edit is made when it is done. See NavigationGenerator.
/// </summary>
public partial class NavigationTool
{
    private readonly HashSet<string> GenerateCollisions = new();
    private bool GenerateIntoSlot;
    private bool GenerateNameAfterCollision = true;
    private bool ShowGenerateCollisions = true;
    private NavGenParameters GenerateN = NavGenParameters.DefaultN;
    private NavGenParameters GenerateO = NavGenParameters.DefaultO;

    /// <summary>The model the collision list was last preselected for.</summary>
    private int GeneratePreselectedFor = -1;

    private bool Generating;
    private readonly ConcurrentQueue<Action> GenerateResults = new();

    private void DisplayGenerate(MapContainer map, NavigationMap navigation, NavmeshModel selected, HashSet<NavmeshModel> removable, List<NavmeshModel> slots)
    {
        // Generate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Generate_Header"), LOC.Get("MAP_Navigation_Generate_Header_TT"));

        var collisionBank = View.HavokCollisionBank;
        var files = collisionBank.MapCollisions.GetValueOrDefault(map.Name)?
            .Where(n => n.StartsWith("h", StringComparison.OrdinalIgnoreCase)).OrderBy(n => n).ToList();

        if (files == null || files.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_No_Collision"));
            return;
        }

        // The collision model of the selected model's ID, when there is one
        if (selected != null && GeneratePreselectedFor != selected.ModelID)
        {
            GeneratePreselectedFor = selected.ModelID;
            GenerateCollisions.Clear();
            var own = files.FirstOrDefault(n => CollisionModelId(n) == selected.ModelID);
            if (own != null)
                GenerateCollisions.Add(own);
        }

        GenerateCollisions.RemoveWhere(n => !files.Contains(n));

        // Collision models: the count stays outside the list toggle
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Generate_Collision_Header"), LOC.Get("MAP_Navigation_Generate_Collision_Header_TT"), ref ShowGenerateCollisions);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Collision_Count", GenerateCollisions.Count, files.Count));

        if (ShowGenerateCollisions)
        {
            ImGui.BeginChild("##navigationGenerateCollisions", new Vector2(0, 140 * DPI.UIScale()), ImGuiChildFlags.Borders);
            foreach (var name in files)
            {
                bool on = GenerateCollisions.Contains(name);
                if (ImGui.Checkbox($"{name}##navigationGenerateCollision_{name}", ref on))
                {
                    if (on) GenerateCollisions.Add(name); else GenerateCollisions.Remove(name);
                }
            }
            ImGui.EndChild();
        }

        // Target
        bool canReplace = selected != null && removable.Contains(selected);
        if (!canReplace)
            GenerateIntoSlot = true;

        if (canReplace)
        {
            if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Generate_Replace", selected.ModelID)}##navigationGenerateReplace", !GenerateIntoSlot))
                GenerateIntoSlot = false;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Replace_TT"));
        }

        if (slots.Count > 0)
        {
            if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Generate_Into_Slot")}##navigationGenerateSlot", GenerateIntoSlot))
                GenerateIntoSlot = true;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Into_Slot_TT"));

            if (GenerateIntoSlot)
            {
                if (slots.All(s => s.ModelID != SlotModelID))
                    SlotModelID = slots[0].ModelID;

                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo("##navigationGenerateSlotCombo", LOC.Get("MAP_Navigation_Models_Slot", SlotModelID)))
                {
                    foreach (var slot in slots)
                        if (ImGui.Selectable($"{LOC.Get("MAP_Navigation_Models_Slot", slot.ModelID)}##navigationGenerateSlot_{slot.ModelID}", slot.ModelID == SlotModelID))
                            SlotModelID = slot.ModelID;
                    ImGui.EndCombo();
                }
                GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Slot_TT"));

                ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Generate_Name_After")}##navigationGenerateNameAfter", ref GenerateNameAfterCollision);
                GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Name_After_TT"));
            }
        }
        else if (!canReplace)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_No_Target"));
            return;
        }

        // Settings of each set
        GenerateN = DisplayGenerateParameters("n", LOC.Get("MAP_Navigation_Generate_Set_N"), GenerateN, NavGenParameters.DefaultN);
        GenerateO = DisplayGenerateParameters("o", LOC.Get("MAP_Navigation_Generate_Set_O"), GenerateO, NavGenParameters.DefaultO);

        if (Generating)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Running"));
            return;
        }

        if (GenerateCollisions.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Pick_Collision"));
            return;
        }

        if (ImGui.Button($"{LOC.Get("MAP_Navigation_Generate")}##navigationGenerate"))
            StartGenerate(map, navigation, GenerateIntoSlot ? slots.FirstOrDefault(s => s.ModelID == SlotModelID) : selected, GenerateIntoSlot);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_TT"));
    }

    private NavGenParameters DisplayGenerateParameters(string id, string title, NavGenParameters p, NavGenParameters defaults)
    {
        GUI.Spacer();
        GUI.SimpleHeader(title, LOC.Get("MAP_Navigation_Generate_Set_TT"));

        float cell = p.CellSize, radius = p.AgentRadius, height = p.AgentHeight, climb = p.MaxClimb, slope = p.MaxSlope, edge = p.MaxEdgeLength;

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateCell_{id}", ref cell, 0.1f, 1f, $"{LOC.Get("MAP_Navigation_Generate_Cell")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Cell_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateRadius_{id}", ref radius, 0f, 2f, $"{LOC.Get("MAP_Navigation_Generate_Radius")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Radius_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateHeight_{id}", ref height, 0.5f, 8f, $"{LOC.Get("MAP_Navigation_Generate_Height")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Height_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateClimb_{id}", ref climb, 0f, 3f, $"{LOC.Get("MAP_Navigation_Generate_Climb")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Climb_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateSlope_{id}", ref slope, 5f, 89f, $"{LOC.Get("MAP_Navigation_Generate_Slope")} %.0f°");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Slope_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateEdge_{id}", ref edge, 0f, 30f, $"{LOC.Get("MAP_Navigation_Generate_Edge")} %.1f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Edge_TT"));

        p = p with { CellSize = cell, AgentRadius = radius, AgentHeight = height, MaxClimb = climb, MaxSlope = slope, MaxEdgeLength = edge };

        if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Generate_Reset")}##navigationGenerateReset_{id}"))
            p = defaults;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Reset_TT"));

        return p;
    }

    /// <summary>Makes the edits of finished generations, on the thread that draws. Called every frame by the viewport.</summary>
    private void DrainGenerateResults()
    {
        while (GenerateResults.TryDequeue(out var apply))
            apply();
    }

    /// <summary>
    /// Where each collision part of a map that draws a collision file puts it, in the space of the map's navmeshes: the
    /// part's world transform, taken back out of the map's own placement (the navmesh parent's), as navmesh vertices
    /// are stored relative to it. A model several parts use is placed once for each.
    /// </summary>
    private static List<Matrix4x4> CollisionPlacements(MapContainer map, string fileName)
    {
        var result = new List<Matrix4x4>();
        var normalized = Renderer.ResourceName.Normalize(fileName);

        if (!Matrix4x4.Invert(MapWorld(map), out var toNavmeshSpace))
            toNavmeshSpace = Matrix4x4.Identity;

        foreach (var entity in map.Objects)
        {
            if (entity is not MsbEntity part || !EntityHelper.IsPartCollision(part))
                continue;

            bool draws = false;
            if (part.RenderSceneMesh is Renderer.MeshRenderableProxy mesh && string.Equals(mesh.ResourceHandle?.Name, normalized, StringComparison.OrdinalIgnoreCase))
                draws = true;
            foreach (var layer in part.CollisionLayerMeshes ?? Enumerable.Empty<Renderer.RenderableProxy>())
                if (layer is Renderer.MeshRenderableProxy layerMesh && string.Equals(layerMesh.ResourceHandle?.Name, normalized, StringComparison.OrdinalIgnoreCase))
                    draws = true;

            // A part whose high collision isn't drawn still names its model: h + the file's six digits
            if (!draws && part.WrappedObject is SoulsFormats.MSBE.Part.Collision collision)
                draws = CollisionModelId(fileName) >= 0 && collision.ModelName == $"h{CollisionModelId(fileName):D6}";

            if (draws)
                result.Add(part.GetWorldMatrix() * toNavmeshSpace);
        }

        return result;
    }

    /// <summary>The model number of a collision file name, e.g. 1000 for h10_01_00_00_001000.hkx.dcx, or -1.</summary>
    private static int CollisionModelId(string fileName)
    {
        var stem = fileName.Split('.')[0];
        var digits = stem.Substring(stem.LastIndexOf('_') + 1);
        return int.TryParse(digits, out var id) ? id : -1;
    }

    /// <summary>
    /// Reads the chosen collision's triangles here (the collision bank isn't safe to read from another thread), then
    /// generates both sets and builds their files in the background, and makes the edit when that is done.
    /// </summary>
    private void StartGenerate(MapContainer map, NavigationMap navigation, NavmeshModel target, bool intoSlot)
    {
        ModelMessage = null;

        if (target == null)
            return;

        var template = NavGenFile.FindTemplate(navigation);
        if (template == null)
        {
            ModelMessage = LOC.Get("MAP_Navigation_Generate_Error_Template");
            ModelMessageIsError = true;
            return;
        }

        var triangles = new List<Vector3>();
        int unplaced = 0;
        foreach (var name in GenerateCollisions)
        {
            var document = View.HavokCollisionBank.GetMapDocument(name);
            if (document == null)
                continue;

            var local = Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).ToList();

            // Where the map's collision parts put the model, as the viewport draws it; as stored if no part uses it
            var placements = CollisionPlacements(map, name);
            if (placements.Count == 0)
            {
                placements.Add(Matrix4x4.Identity);
                unplaced++;
            }

            foreach (var placement in placements)
                foreach (var point in local)
                    triangles.Add(Vector3.Transform(point, placement));
        }

        // TEMPORARY diagnostics: how the chosen collision was placed
        Smithbox.Log(typeof(NavigationTool), $"[NavDiag] generate from {string.Join(", ", GenerateCollisions)}: {triangles.Count / 3} placed triangles, " +
            $"{unplaced} models without a collision part; placements {string.Join("; ", GenerateCollisions.SelectMany(n => CollisionPlacements(map, n)).Select(m => $"({m.M41:0.##}, {m.M42:0.##}, {m.M43:0.##})"))}");

        int? modelId = null;
        if (intoSlot && GenerateNameAfterCollision && GenerateCollisions.Count > 0)
        {
            int id = CollisionModelId(GenerateCollisions.OrderBy(n => n).First());
            if (id >= 0)
                modelId = id;
        }

        var pn = GenerateN;
        var po = GenerateO;
        var nTemplate = target.NFile?.Mesh;
        var oTemplate = target.OFile?.Mesh;
        Generating = true;

        Task.Run(() =>
        {
            NavGenResult n = null, o = null;
            Havok.HavokDocument nDoc = null, oDoc = null;
            string error = null;

            try
            {
                n = NavigationGenerator.Generate(triangles, pn, nTemplate);
                o = NavigationGenerator.Generate(triangles, po, oTemplate);
                error = n.Error ?? o.Error;

                if (error == null)
                {
                    nDoc = NavGenFile.Build(template, n.Mesh);
                    oDoc = NavGenFile.Build(template, o.Mesh);
                    if (nDoc == null || oDoc == null)
                        error = "MAP_Navigation_Generate_Error_Template";
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Generate_Failed"), e);
                error = "MAP_Navigation_Generate_Error_Recast";
            }

            GenerateResults.Enqueue(() =>
            {
                Generating = false;

                if (error != null)
                {
                    ModelMessage = LOC.Get(error);
                    ModelMessageIsError = true;
                    return;
                }

                // The map may have changed while the generation ran; the edit is made against it as it is now
                var current = Bank.GetMap(map);
                var currentTarget = current?.Models.FirstOrDefault(m => ReferenceEquals(m.NEntry, target.NEntry));
                if (currentTarget == null)
                {
                    ModelMessage = LOC.Get("MAP_Navigation_Model_Error_Slot");
                    ModelMessageIsError = true;
                    return;
                }

                int faces = n.Mesh.m_faces.Count, oFaces = o.Mesh.m_faces.Count;
                EditModel(map, current, () => NavModelEdit.Generate(current, currentTarget, nDoc, oDoc, modelId),
                    LOC.Get("MAP_Navigation_Generate_Done", faces, oFaces, modelId ?? currentTarget.ModelID, triangles.Count / 3));
            });
        });
    }
}
