using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP mode's Regenerate tab (Phase G6): the whole map's navmeshes generated again from its collision parts, as the
/// Havok mode's Regenerate does. The faces the seeds reach are split into the map's navmeshes (one per MSB navmesh part,
/// so the MSB is not touched), keep the flags of the faces under them, and steep ones become drops; ladders are made
/// again at the ladder objects. One undoable edit. See NgpRegenerate.
/// </summary>
public partial class NavigationTool
{
    private readonly NgpRegenInput NgpRegenOptions = new();
    private NavGenParameters NgpRegenParameters = NgpModelEdit.DefaultParameters;
    private bool NgpRegenLadders = true;
    private bool NgpRegenSeedNew = true;
    private bool NgpRegenerating;

    private void DisplayNgpRegenerateTab(MapContainer map)
    {
        var data = Ngp.GetMap(map);
        if (data == null)
            return;

        // Regenerate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Regen_Header"), LOC.Get("MAP_Navigation_Ngp_Regen_Header_TT"));

        var o = NgpRegenOptions;
        bool seed = o.SeedFromCurrent, meshes = o.KeepMeshes, flags = o.KeepFlags, steep = o.MarkSteep;
        float slope = o.SteepSlope;

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Regen_Seed")}##navigationNgpRegenSeed", ref seed);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Regen_Seed_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Regen_Seed_New")}##navigationNgpRegenSeedNew", ref NgpRegenSeedNew);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Regen_Seed_New_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Regen_Keep_Meshes")}##navigationNgpRegenMeshes", ref meshes);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Regen_Keep_Meshes_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Regen_Keep_Flags")}##navigationNgpRegenFlags", ref flags);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Regen_Keep_Flags_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Regen_Steep")}##navigationNgpRegenSteep", ref steep);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Regen_Steep_TT"));
        if (steep)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat("##navigationNgpRegenSlope", ref slope, 20f, 89f, $"{LOC.Get("MAP_Navigation_Ngp_Steep_Slope")} %.0f°");
            GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Steep_Slope_TT"));
        }
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Regen_Ladders")}##navigationNgpRegenLadders", ref NgpRegenLadders);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Regen_Ladders_TT"));

        o.SeedFromCurrent = seed;
        o.KeepMeshes = meshes;
        o.KeepFlags = flags;
        o.MarkSteep = steep;
        o.SteepSlope = slope;

        NgpRegenParameters = DisplayGenerateParameters("navigationNgpRegen", LOC.Get("MAP_Navigation_Ngp_Generate_Settings"),
            NgpRegenParameters, NgpModelEdit.DefaultParameters) with { Links = false };

        if (!NgpPartsMatch(data))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Ngp_Models_Mismatch", data.PartCount, data.Meshes.Count));
        if (HasNgpVertexEdits)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Ngp_Strips_Pending"));

        GUI.ConditionalMultiButtonInput("navigationNgpRegenActions",
            "navigationNgpRegen", LOC.Get("MAP_Navigation_Ngp_Regen"), LOC.Get("MAP_Navigation_Ngp_Regen_TT"),
            () => StartNgpRegenerate(map, data), !NgpRegenerating && !NgpGenerating && NgpPartsMatch(data) && !HasNgpVertexEdits,

            "navigationNgpRegenCancel", LOC.Get("MAP_Navigation_Generate_Cancel"), LOC.Get("MAP_Navigation_Ngp_Regen_Cancel_TT"),
            () => { NgpGenerateRun++; NgpRegenerating = false; }, NgpRegenerating);

        if (NgpRegenerating)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Regen_Running"));
    }

    private void StartNgpRegenerate(MapContainer map, NgpMap data)
    {
        if (!Matrix4x4.Invert(data.World, out var toLocal))
            toLocal = Matrix4x4.Identity;

        var input = new NgpRegenInput
        {
            Parameters = NgpRegenParameters with { Links = false },
            SeedFromCurrent = NgpRegenOptions.SeedFromCurrent,
            KeepMeshes = NgpRegenOptions.KeepMeshes,
            KeepFlags = NgpRegenOptions.KeepFlags,
            MarkSteep = NgpRegenOptions.MarkSteep,
            SteepSlope = NgpRegenOptions.SteepSlope,
        };

        // Every collision part's triangles, map-local, with its NvmGroup bit: the navmesh it belongs to. The collision the
        // user added (not in the game's binder, or made or changed this session) and the selected collision parts seed:
        // a new area isn't joined to the current navmesh, so nothing else would reach it
        int parts = 0;
        var missing = new List<string>();
        var seedParts = new List<string>();
        var selected = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToHashSet();
        foreach (var entity in map.Objects)
        {
            if (entity is not MsbEntity part || part.WrappedObject is not MSB2.Part.Collision collision || SourceOf(part) is not GenerateSource source)
                continue;

            if (NvmSourceTriangles(source) is not List<Vector3> triangles)
            {
                missing.Add(source.Model);
                continue;
            }

            bool seedPart = NgpRegenSeedNew && (selected.Contains(part)
                || NvmSourceFile(source) is string file && View.HavokCollisionBank.IsAddedOrChangedCollision(map.Name, file));
            if (seedPart)
                seedParts.Add(part.Name);

            input.Add(triangles.Select(p => Vector3.Transform(p, toLocal)).ToList(), NgpModelEdit.GroupBit(CollisionGroups(collision)), seedPart);
            parts++;
        }

        if (seedParts.Count > 0)
            Smithbox.Log(this, LOC.Get("MAP_Navigation_Ngp_Regen_Seed_Parts", string.Join(", ", seedParts)));

        if (missing.Count > 0)
            Smithbox.Log(this, LOC.Get("MAP_Navigation_Generate_Missing", string.Join(", ", missing.Distinct())));

        foreach (var mesh in data.Meshes)
            input.MeshGroups.Add((mesh.Part as MsbEntity)?.WrappedObject is MSB2.Part.Navmesh p ? NgpModelEdit.GroupBit(NavmeshGroups(p)) : -1);

        // Seeds: the generators (DS2's enemies) and the start points
        foreach (var entity in map.Objects)
            if (entity is MsbEntity e && (e.Type == MsbEntityType.DS2Generator || e.WrappedObject is MSB2.Region.StartPoint))
                input.Seeds.Add(Vector3.Transform(e.GetWorldMatrix().Translation, toLocal));

        var ladders = NgpRegenLadders
            ? map.Objects.OfType<MsbEntity>().Select(e => NgpLadderSourceOf(e, toLocal))
                .Where(s => s != null && NgpLadderModel.IsMatch(s.Model ?? "")).ToList()
            : new List<NgpLadderSource>();

        if (input.Triangles.Count < 3)
        {
            SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Regen_Error_Input"), true);
            return;
        }

        var before = data.Ngp;
        int run = ++NgpGenerateRun;
        NgpRegenerating = true;
        SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Regen_Started", parts, input.Triangles.Count / 3, input.Seeds.Count, missing.Count, seedParts.Count));

        Task.Run(() =>
        {
            NgpRegenResult result;
            (NGP Ngp, int Both, int One, int Failed) withLadders = default;
            try
            {
                result = NgpRegenerate.Run(before, input);
                if (result.Ngp != null && ladders.Count > 0)
                    withLadders = NgpRegenerate.WithLadders(result.Ngp, ladders.Select(l => (l.Foot, l.Forward, l.Top)));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Ngp_Regen_Failed"), e);
                result = new NgpRegenResult { Error = "MAP_Navigation_Ngp_Regen_Failed" };
            }

            NgpGenerateResults.Enqueue(() =>
            {
                if (run != NgpGenerateRun)
                    return;

                NgpRegenerating = false;

                if (result.Ngp == null)
                {
                    SetNgpMessage(LOC.Get(result.Error), true);
                    return;
                }

                var current = Ngp.GetMap(map);
                if (current == null || !ReferenceEquals(current.Ngp, before))
                {
                    SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Regen_Stale"), true);
                    return;
                }

                var after = withLadders.Ngp ?? result.Ngp;

                if (NgpContainer != map)
                    RebuildNgpOverlay(map);

                View.ViewportActionManager.ExecuteAction(new NavigationNgpEditAction(map, Ngp, before, after, OnNgpEdited));
                SetNgpPicked(-1, -1, default);
                SelectNgpGate(-1);

                var text = LOC.Get("MAP_Navigation_Ngp_Regen_Done", result.Generated, result.Kept, result.SeedsPlaced, result.Seeds,
                    result.MeshFaces.Count(c => c >= 0), result.KeptAsBefore, result.ByCurrent, result.ByCollision, result.BySpread, result.Dropped,
                    result.FlagsKept, result.Steep, before.Gates.Count, after.Gates.Count, seedParts.Count, result.SeededByCollision, result.ByNearest);
                if (ladders.Count > 0)
                    text += " " + LOC.Get("MAP_Navigation_Ngp_Regen_Ladders_Done", ladders.Count, withLadders.Both, withLadders.One, withLadders.Failed);
                SetNgpMessage(text, result.KeptAsBefore > 0 || withLadders.One + withLadders.Failed > 0);
                Smithbox.Log(this, NgpMessage);
            });
        });
    }
}
