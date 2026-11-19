using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Regenerate tab's Regenerate Map group (Navigation Full Regeneration Analysis.md, steps 1-5): the whole navigation of
/// the map built again from its high collision and its assets' collision, as one undo. The generation runs in the
/// background (NavRegenerate.Prepare); the NVA, connections, gates, node banks and NVC files are rebuilt when it is done
/// (NavRegenerate.Apply). It uses the Generate group's n and o settings.
/// </summary>
public partial class NavigationTool
{
    private bool RegenSeedFromCurrent = true;
    private bool RegenKeepModels = true;
    private bool RegenKeepFaceData = true;
    private bool RegenDeriveFaceData = true;
    private bool RegenSkipRuntimeCuts = true;
    private bool RegenStitch = true;
    private bool RegenLinksN = true;
    private bool RegenLinksO;
    private bool RegenKeepLinks = true;
    private bool Regenerating;
    private string RegenMessage;
    private bool RegenMessageIsError;
    private int RegenRun;

    /// <summary>The Regenerate tab: the Regenerate Map group, over the map picked above.</summary>
    private void DisplayRegenerateTab(MapContainer map)
    {
        // The same cached map data as the Models tab, dropped by every navigation edit
        if (ModelsNavigation == null || ModelsMap != map.Name)
        {
            ModelsNavigation = Bank.GetMap(map);
            ModelsMap = map.Name;
        }

        if (ModelsNavigation == null)
            return;

        DisplayRegenerate(map, ModelsNavigation);
    }

    private void DisplayRegenerate(MapContainer map, NavigationMap navigation)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Regenerate_Header"), LOC.Get("MAP_Navigation_Regenerate_Header_TT"));

        if (navigation.Models.Any(m => m.Quarters.Count > 0))
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Regenerate_Error_Quarters"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Seed_Current")}##navigationRegenSeed", ref RegenSeedFromCurrent);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Seed_Current_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Keep_Models")}##navigationRegenModels", ref RegenKeepModels);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Keep_Models_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Keep_Face_Data")}##navigationRegenFaceData", ref RegenKeepFaceData);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Keep_Face_Data_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Derive_Face_Data")}##navigationRegenDerive", ref RegenDeriveFaceData);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Derive_Face_Data_TT"));
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Skip_Runtime_Cuts")}##navigationRegenSkipCuts", ref RegenSkipRuntimeCuts);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Skip_Runtime_Cuts_TT"));

        // Links are mesh user edges, which DS3, BB and SDT don't have
        if (navigation.Profile.HasMeshUserEdges)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Links_N")}##navigationRegenLinksN", ref RegenLinksN);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Links_N_TT"));
            if (navigation.Profile.HasOSet)
            {
                ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Links_O")}##navigationRegenLinksO", ref RegenLinksO);
                GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Links_O_TT"));
            }
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Keep_Links")}##navigationRegenKeepLinks", ref RegenKeepLinks);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Keep_Links_TT"));
        }
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Stitch")}##navigationRegenStitch", ref RegenStitch);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Stitch_TT"));

        if (Regenerating)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Regenerate_Running"));
            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Regenerate_Cancel")}##navigationRegenCancel"))
            {
                RegenRun++;
                Regenerating = false;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Cancel_TT"));
        }
        else
        {
            GUI.MultiButtonInput("navigationRegenActions",
                "navigationRegen", LOC.Get("MAP_Navigation_Regenerate"), LOC.Get("MAP_Navigation_Regenerate_TT"),
                () => StartRegenerate(map, navigation));
        }

        if (RegenMessage != null)
        {
            if (RegenMessageIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, RegenMessage);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, RegenMessage);
        }
    }

    private void StartRegenerate(MapContainer map, NavigationMap navigation)
    {
        RegenMessage = null;

        if (!Matrix4x4.Invert(MapWorld(map), out var toNavmeshSpace))
            toNavmeshSpace = Matrix4x4.Identity;

        var input = new NavRegenInput
        {
            // The Generate group's settings, with this group's link choices (links are mesh user edges, which DS3, BB and SDT don't have)
            N = GenerateN with { Links = RegenLinksN && navigation.Profile.HasMeshUserEdges },
            O = GenerateO with { Links = RegenLinksO && navigation.Profile.HasMeshUserEdges },
            KeepLinks = RegenKeepLinks && navigation.Profile.HasMeshUserEdges,
            SeedFromCurrent = RegenSeedFromCurrent,
            KeepModels = RegenKeepModels,
            KeepFaceData = RegenKeepFaceData,
            DeriveFaceData = RegenDeriveFaceData,
            StitchNeighbours = RegenStitch,
        };

        // The map's high collision as the game places it: each collision part's model (its h file in the map's binder),
        // moved by the part. The collision bank's editable documents hold the user's new and edited files (the Collision
        // Editor's Create places new collision with a new part, in that part's space); files no part places aren't loaded
        // by the game, so they are left out
        var missing = new List<string>();
        int collisionParts = 0;
        var collisionCache = new Dictionary<string, (List<Vector3> Triangles, List<uint> Materials)>(StringComparer.OrdinalIgnoreCase);
        var placedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entity in map.Objects)
        {
            if (entity is not MsbEntity part || !EntityHelper.IsPartCollision(part) || SourceOf(part) is not GenerateSource source)
                continue;

            var fileName = CollisionFileName(source);
            int model = CollisionModelId(source.Model);
            if (model < 0)
                continue;

            if (!collisionCache.TryGetValue(fileName, out var collision))
            {
                var document = View.HavokCollisionBank.GetEditableDocument(fileName);
                collision = document == null ? (null, null) : CollisionWithMaterials(document);
                collisionCache[fileName] = collision;
            }

            if (collision.Triangles == null || collision.Triangles.Count == 0)
            {
                missing.Add(source.Model);
                continue;
            }

            var placement = part.GetWorldMatrix() * toNavmeshSpace;
            input.Add(collision.Triangles.Select(p => Vector3.Transform(p, placement)).ToList(), model, collision.Materials);
            placedFiles.Add(fileName);
            collisionParts++;
        }

        int unplaced = View.HavokCollisionBank.MapCollisions.TryGetValue(map.Name, out var listed)
            ? listed.Count(n => n.StartsWith("h", StringComparison.OrdinalIgnoreCase) && !placedFiles.Contains(n)) : 0;

        // Every asset of the map, placed, with its own collision; those the game cuts at runtime are left out
        int assets = 0, cutAtRuntime = 0;
        foreach (var entity in map.Objects)
        {
            if (entity is not MsbEntity part || !EntityHelper.IsPartAsset(part) || EntityHelper.IsPartDummyAsset(part) || SourceOf(part) is not GenerateSource source)
                continue;

            if (RegenSkipRuntimeCuts && RuntimeCutFlag(source) != 0)
            {
                cutAtRuntime++;
                continue;
            }

            var local = SourceTriangles(source);
            if (local == null || local.Count == 0)
            {
                missing.Add(source.Model);
                continue;
            }

            var placement = part.GetWorldMatrix() * toNavmeshSpace;
            input.Add(local.Select(p => Vector3.Transform(p, placement)).ToList(), -1);
            assets++;
        }

        if (missing.Count > 0)
            Smithbox.Log(this, LOC.Get("MAP_Navigation_Generate_Missing", string.Join(", ", missing.Distinct())));

        // Seeds: the map's enemies and players
        foreach (var entity in map.Objects)
            if (entity is MsbEntity part && (EntityHelper.IsPartEnemy(part) || EntityHelper.IsPartPlayer(part)))
                input.Seeds.Add(Vector3.Transform(Vector3.Zero, part.GetWorldMatrix() * toNavmeshSpace));

        if (input.Triangles.Count < 3)
        {
            RegenMessage = LOC.Get("MAP_Navigation_Regenerate_Error_Input");
            RegenMessageIsError = true;
            return;
        }

        Regenerating = true;
        int run = ++RegenRun;
        var loaded = LoadedMaps();

        Task.Run(() =>
        {
            NavRegenerate.Prepared prepared = null;
            string error = null;
            try
            {
                prepared = NavRegenerate.Prepare(navigation, input);
                error = prepared.Error;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Regenerate_Failed"), e);
                error = "MAP_Navigation_Generate_Error_Recast";
            }

            GenerateResults.Enqueue(() =>
            {
                if (run != RegenRun)
                    return;

                Regenerating = false;

                if (error != null)
                {
                    RegenMessage = LOC.Get(error);
                    RegenMessageIsError = true;
                    return;
                }

                var current = Bank.GetMap(map);
                if (current == null)
                    return;

                NavigationMap Neighbour(string id)
                {
                    var other = loaded.FirstOrDefault(m => m.Name == id);
                    return other != null && other != map && Bank.HasNavmeshFiles(other) ? Bank.GetMap(other) : null;
                }

                var before = new NavModelState(current);
                NavRegenResult result;
                try
                {
                    result = NavRegenerate.Apply(current, prepared, input, RegenStitch ? Neighbour : null);
                }
                catch (Exception e)
                {
                    before.Restore();
                    Smithbox.LogError(this, LOC.Get("MAP_Navigation_Regenerate_Failed"), e);
                    RegenMessage = LOC.Get("MAP_Navigation_Regenerate_Failed");
                    RegenMessageIsError = true;
                    return;
                }

                if (!result.Ok)
                {
                    before.Restore();
                    RegenMessage = LOC.Get(result.Error);
                    RegenMessageIsError = true;
                    return;
                }

                ModelEditMap = map;
                var after = new NavModelState(current);
                View.ViewportActionManager.ExecuteAction(new NavigationModelEditAction(map, Bank, before, after, result.Files, result.Changes, OnModelEdited));

                var n = result.Sets.GetValueOrDefault(NavmeshSet.N);
                var o = result.Sets.GetValueOrDefault(NavmeshSet.O);
                RegenMessage = LOC.Get("MAP_Navigation_Regenerate_Done",
                    collisionParts, assets, cutAtRuntime, input.Seeds.Count,
                    n?.Kept ?? 0, n?.Generated ?? 0, o?.Kept ?? 0, o?.Generated ?? 0,
                    result.Replaced, result.Added, result.Removed,
                    result.Connections, result.CrossMapConnections, result.Gates, (n?.Links ?? 0) + (o?.Links ?? 0),
                    result.NodeBankFacesKept, result.NodeBankFacesDropped, (n?.LinksKept ?? 0) + (o?.LinksKept ?? 0), unplaced)
                    + (result.Skipped.Count > 0 ? " " + LOC.Get("MAP_Navigation_Regenerate_Skipped", string.Join(", ", result.Skipped)) : "");
                RegenMessageIsError = false;
            });
        });
    }

    /// <summary>
    /// A collision document's triangles in its own space, with the collision material of each (the FromSoftware compressed
    /// meshes' parameters; uint.MaxValue for shapes without them).
    /// </summary>
    private static (List<Vector3> Triangles, List<uint> Materials) CollisionWithMaterials(Havok.HavokDocument document)
    {
        var triangles = new List<Vector3>();
        var materials = new List<uint>();

        foreach (var body in CollisionGeometry.Bodies(document.Objects))
        {
            var world = CollisionGeometry.BodyTransform(body);

            if (body.Shape is IHavokCompressedMesh mesh && CollisionEditorShapes.IsCompressedMeshShape(body.Shape))
            {
                var byKey = CollisionEditorShapes.GetMaterialsByKey(body.Shape);
                int shift = 32 - mesh.BitsPerKey;
                foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(mesh))
                {
                    triangles.Add(Vector3.Transform(a, world));
                    triangles.Add(Vector3.Transform(b, world));
                    triangles.Add(Vector3.Transform(c, world));
                    materials.Add(byKey.TryGetValue(key >> shift, out var material) ? material : uint.MaxValue);
                }
                continue;
            }

            var local = new List<Vector3>();
            try
            {
                CollisionGeometry.AppendTriangles(body.Shape, world, local);
            }
            catch (Exception)
            {
                continue;
            }

            triangles.AddRange(local);
            materials.AddRange(Enumerable.Repeat(uint.MaxValue, local.Count / 3));
        }

        return (triangles, materials);
    }
}
