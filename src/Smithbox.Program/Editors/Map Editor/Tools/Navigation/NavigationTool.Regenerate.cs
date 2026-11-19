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
    private bool RegenRelatedTiles = true;
    private bool RegenSnapBorders = true;

    /// <summary>Regenerate from the high collision instead of the low (both Regenerate tabs). Off by default: navmeshes follow the low collision, which characters walk on; the high is for inverse kinematics.</summary>
    private bool RegenHighCollision;
    private bool Regenerating;
    private string RegenMessage;
    private bool RegenMessageIsError;
    private int RegenRun;

    /// <summary>Which collision a regeneration reads, low (the default) or high; shared by the Havok and NGP Regenerate tabs.</summary>
    private void DisplayRegenCollisionSource(string id)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(LOC.Get("MAP_Navigation_Regenerate_Collision_Source"));
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Collision_Source_TT"));
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Regenerate_Collision_Low")}##{id}CollisionLow", !RegenHighCollision))
            RegenHighCollision = false;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Collision_Low_TT"));
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Regenerate_Collision_High")}##{id}CollisionHigh", RegenHighCollision))
            RegenHighCollision = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Collision_High_TT"));
    }

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

        DisplayRegenCollisionSource("navigationRegen");

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
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Snap_Borders")}##navigationRegenBorders", ref RegenSnapBorders);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Snap_Borders_TT"));

        if (IsSmallTile(map.Name))
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Regenerate_Related_Tiles")}##navigationRegenRelated", ref RegenRelatedTiles);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Regenerate_Related_Tiles_TT"));
        }

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
            SnapBorders = RegenSnapBorders,
        };

        // The map's low collision as the game places it: each collision part's model (its l file in the map's binder),
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

            var fileName = CollisionFileName(source, RegenHighCollision);
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
            ? listed.Count(n => n.StartsWith(RegenHighCollision ? "h" : "l", StringComparison.OrdinalIgnoreCase) && !placedFiles.Contains(n)) : 0;

        // The tile's own area: where its collision lies, across, before any asset is added
        var ownBounds = XZBounds(input.Triangles);

        // Every asset of the map, placed, with its own collision; those the game cuts at runtime are left out, and the
        // barriers (evergaol and fog walls), which the game's navmesh runs through
        int assets = 0, cutAtRuntime = 0, barriers = 0, stealthAssets = 0;
        foreach (var entity in map.Objects)
        {
            if (entity is not MsbEntity part || !EntityHelper.IsPartAsset(part) || EntityHelper.IsPartDummyAsset(part) || SourceOf(part) is not GenerateSource source)
                continue;

            if (RegenSkipRuntimeCuts && RuntimeCutFlag(source) != 0)
            {
                cutAtRuntime++;
                continue;
            }

            var local = SourceTriangles(source, RegenHighCollision);

            // Bushes and grass: their stealth volumes are walked through (some have nothing else)
            if (HasStealthBodies(source, RegenHighCollision))
                stealthAssets++;

            if (local == null || local.Count == 0)
            {
                if (!HasStealthBodies(source, RegenHighCollision))
                    missing.Add(source.Model);
                continue;
            }

            if (IsBarrierAsset(source, RegenHighCollision))
            {
                barriers++;
                continue;
            }

            var placement = part.GetWorldMatrix() * toNavmeshSpace;
            input.Add(local.Select(p => Vector3.Transform(p, placement)).ToList(), -1);
            assets++;
        }

        // An open world small tile: the assets of its parent (_01), ancestor (_02) and adjacent small tiles, cut to the tile
        string relatedMessage = null;
        if (RegenRelatedTiles && IsSmallTile(map.Name))
            relatedMessage = AddRelatedTileAssets(map, input, ownBounds);

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
                    + (barriers > 0 ? " " + LOC.Get("MAP_Navigation_Regenerate_Barriers_Done", barriers) : "")
                    + (stealthAssets > 0 ? " " + LOC.Get("MAP_Navigation_Regenerate_Stealth_Done", stealthAssets) : "")
                    + (result.Skipped.Count > 0 ? " " + LOC.Get("MAP_Navigation_Regenerate_Skipped", string.Join(", ", result.Skipped)) : "")
                    + (relatedMessage != null ? " " + relatedMessage : "")
                    + (input.SnapBorders ? " " + LOC.Get("MAP_Navigation_Regenerate_Borders_Done",
                        n?.Borders ?? 0, n?.BorderVertices ?? 0, o?.Borders ?? 0, o?.BorderVertices ?? 0) : "")
                    + (result.NeighboursNotLoaded.Count > 0 ? " " + LOC.Get("MAP_Navigation_Regenerate_Neighbours_Not_Loaded", string.Join(", ", result.NeighboursNotLoaded)) : "");
                RegenMessageIsError = result.NeighboursNotLoaded.Count > 0;
            });
        });
    }

    /// <summary>Half a small open world tile's width: its origin is its centre (NavStitchPlanner.TileOffset).</summary>
    private const float HalfTile = 128f;

    /// <summary>An open world small tile (m60_XX_YY_00, _10 and the like): the scale digit is 0.</summary>
    private static bool IsSmallTile(string mapId)
        => NavigationMap.IsOpenWorld(mapId) && mapId.Length == 12 && mapId[11] == '0'
           && int.TryParse(mapId.AsSpan(4, 2), out _) && int.TryParse(mapId.AsSpan(7, 2), out _);

    /// <summary>
    /// Whether another open world map overlaps a small tile: its parent (scale 1, 2 × 2 tiles) or ancestor (scale 2,
    /// 4 × 4 tiles) of the same area and version digit, or an adjacent small tile (the eight around it).
    /// </summary>
    private static bool IsRelatedTile(string tile, string other, out bool isParent)
    {
        isParent = false;
        if (other == tile || !NavigationMap.IsOpenWorld(other) || other.Length != 12 || other[..3] != tile[..3] || other[10] != tile[10]
            || !int.TryParse(other.AsSpan(4, 2), out int ox) || !int.TryParse(other.AsSpan(7, 2), out int oz))
            return false;

        int x = int.Parse(tile.AsSpan(4, 2)), z = int.Parse(tile.AsSpan(7, 2));
        switch (other[11])
        {
            case '0': return Math.Abs(ox - x) <= 1 && Math.Abs(oz - z) <= 1;
            case '1': isParent = true; return ox == x / 2 && oz == z / 2;
            case '2': isParent = true; return ox == x / 4 && oz == z / 4;
            default: return false;
        }
    }

    /// <summary>
    /// Adds the collision of the assets of the loaded maps that overlap a small tile (its _01 parent, _02 ancestor and
    /// adjacent small tiles), cut to the tile's area across: the large assets the game places in the parent tiles lie on
    /// the tile too, and an asset of a neighbour can reach over the border. Each is placed by its part in its own map, then
    /// moved by the offset between the two tiles' origins, so it doesn't rely on how the editor placed the loaded maps.
    /// Assets without collision (most of the parents' are visual only) are skipped quietly. Returns the line for the
    /// result message, naming parents that exist but aren't loaded.
    /// </summary>
    private string AddRelatedTileAssets(MapContainer map, NavRegenInput input, (Vector2 Min, Vector2 Max)? ownBounds)
    {
        // The tile's collision, else the tile's square
        var (min, max) = ownBounds ?? (new Vector2(-HalfTile), new Vector2(HalfTile));

        int added = 0, cutAtRuntime = 0;
        var used = new List<string>();
        var loadedParents = new HashSet<string>();

        foreach (var other in LoadedMaps())
        {
            if (!IsRelatedTile(map.Name, other.Name, out bool isParent) || NavStitchPlanner.TileOffset(map.Name, other.Name) is not Vector3 offset)
                continue;

            if (isParent)
                loadedParents.Add(other.Name);

            if (!Matrix4x4.Invert(other.RootObject.GetWorldMatrix(), out var toOtherSpace))
                toOtherSpace = Matrix4x4.Identity;
            var toThisSpace = toOtherSpace * Matrix4x4.CreateTranslation(offset);

            int fromThis = 0;
            foreach (var entity in other.Objects)
            {
                if (entity is not MsbEntity part || !EntityHelper.IsPartAsset(part) || EntityHelper.IsPartDummyAsset(part) || SourceOf(part) is not GenerateSource source)
                    continue;

                if (RegenSkipRuntimeCuts && RuntimeCutFlag(source) != 0)
                {
                    cutAtRuntime++;
                    continue;
                }

                var local = SourceTriangles(source, RegenHighCollision);
                if (local == null || local.Count == 0 || IsBarrierAsset(source, RegenHighCollision))
                    continue;

                var placement = part.GetWorldMatrix() * toThisSpace;
                var clipped = ClipXZ(local.Select(p => Vector3.Transform(p, placement)).ToList(), min, max);
                if (clipped.Count == 0)
                    continue;

                input.Add(clipped, -1);
                fromThis++;
            }

            if (fromThis > 0)
            {
                used.Add($"{other.Name} ({fromThis})");
                added += fromThis;
            }
        }

        // The parents the game has for this tile that aren't loaded: their assets are missing from the input
        var existing = View.Project.Locator.MapFiles.Entries.Select(e => e.Filename).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var notLoaded = existing.Where(id => IsRelatedTile(map.Name, id, out bool parent) && parent && !loadedParents.Contains(id)).OrderBy(id => id).ToList();

        var message = LOC.Get("MAP_Navigation_Regenerate_Related_Done", added, used.Count > 0 ? string.Join(", ", used) : "-", cutAtRuntime);
        if (notLoaded.Count > 0)
            message += " " + LOC.Get("MAP_Navigation_Regenerate_Related_Not_Loaded", string.Join(", ", notLoaded));
        return message;
    }

    /// <summary>The bounds across (X, Z) of a list of points, or null for none.</summary>
    private static (Vector2 Min, Vector2 Max)? XZBounds(IReadOnlyList<Vector3> points)
    {
        if (points.Count == 0)
            return null;

        var min = new Vector2(float.MaxValue);
        var max = new Vector2(float.MinValue);
        foreach (var p in points)
        {
            min = Vector2.Min(min, new Vector2(p.X, p.Z));
            max = Vector2.Max(max, new Vector2(p.X, p.Z));
        }
        return (min, max);
    }

    /// <summary>
    /// Triangles (three points each) cut to a box across (X, Z), with any height: those inside kept as they are, those
    /// across its sides cut along them (Sutherland-Hodgman) and fanned back into triangles, those outside dropped.
    /// </summary>
    private static List<Vector3> ClipXZ(List<Vector3> triangles, Vector2 min, Vector2 max)
    {
        var result = new List<Vector3>();
        var polygon = new List<Vector3>(9);
        var next = new List<Vector3>(9);

        for (int t = 0; t + 2 < triangles.Count; t += 3)
        {
            var a = triangles[t];
            var b = triangles[t + 1];
            var c = triangles[t + 2];
            float x0 = MathF.Min(a.X, MathF.Min(b.X, c.X)), x1 = MathF.Max(a.X, MathF.Max(b.X, c.X));
            float z0 = MathF.Min(a.Z, MathF.Min(b.Z, c.Z)), z1 = MathF.Max(a.Z, MathF.Max(b.Z, c.Z));

            if (x1 < min.X || x0 > max.X || z1 < min.Y || z0 > max.Y)
                continue;

            if (x0 >= min.X && x1 <= max.X && z0 >= min.Y && z1 <= max.Y)
            {
                result.Add(a); result.Add(b); result.Add(c);
                continue;
            }

            polygon.Clear();
            polygon.Add(a); polygon.Add(b); polygon.Add(c);

            // The four sides: (axis 0 = X, 1 = Z; keep the side where sign * (value - limit) >= 0)
            foreach (var (axis, limit, sign) in new[] { (0, min.X, 1f), (0, max.X, -1f), (1, min.Y, 1f), (1, max.Y, -1f) })
            {
                next.Clear();
                for (int i = 0; i < polygon.Count; i++)
                {
                    var p = polygon[i];
                    var q = polygon[(i + 1) % polygon.Count];
                    float dp = sign * ((axis == 0 ? p.X : p.Z) - limit);
                    float dq = sign * ((axis == 0 ? q.X : q.Z) - limit);

                    if (dp >= 0)
                        next.Add(p);
                    if ((dp >= 0) != (dq >= 0))
                        next.Add(Vector3.Lerp(p, q, dp / (dp - dq)));
                }

                (polygon, next) = (next, polygon);
                if (polygon.Count < 3)
                    break;
            }

            for (int i = 1; i + 1 < polygon.Count; i++)
            {
                result.Add(polygon[0]); result.Add(polygon[i]); result.Add(polygon[i + 1]);
            }
        }

        return result;
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
