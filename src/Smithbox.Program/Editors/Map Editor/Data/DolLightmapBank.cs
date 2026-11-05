using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// DS3's lightmaps of its map pieces, the DOL textures: what the game lights the pieces with in place of the light
/// probes, which only light its objects and characters. See Documentation/Older Games/Analysis.md.
/// <para>
/// They are in map/mAA/gi_env_mAA.tpfbhd, every map of the area together, as
/// AABBCCDD_mXXXXXX_gi_NNNN_00_dol_0K[_atlasIII][_l]: the map, the piece's model, the bake NNNN (the map's BTAB and
/// BTPB _NNNN files), DOL 1 (K = 0, the light's color, the sun's shadow in alpha, an HDR scale in its TPF entry's
/// FloatStruct) or DOL 2 (K = 1, the light's main direction in the model's space, its sharpness in alpha), the atlas
/// of an instanced piece, and a low detail copy. A piece with BTAB entries uses an atlas per material, at the entry's
/// UV scale and offset; any other uses its own textures whole. The second UV set reads them, as the game's
/// GXFlver_*DOL* shaders do.
/// </para>
/// </summary>
public class DolLightmapBank
{
    public ProjectEntry Project;

    /// <summary>The bake read, the map's _0000 files</summary>
    private const string Bake = "0000";

    private class AreaIndex
    {
        public readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, Vector3> HdrScales = new(StringComparer.OrdinalIgnoreCase);
        public volatile bool Ready;
    }

    private class PartState
    {
        public bool Done;
        public LightmapBinding Binding;

        // The mesh the lightmap was last put on
        public MeshRenderableProxy Proxy;
    }

    private readonly Dictionary<string, AreaIndex> Areas = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<object, PartState> Parts = new(ReferenceEqualityComparer.Instance);

    // The maps whose parts were reported once settled (temporary diagnostics)
    private readonly HashSet<string> ReportedMaps = new(StringComparer.OrdinalIgnoreCase);

    public DolLightmapBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        return CFG.Current.Viewport_Map_Lightmaps &&
               GparamProfile.For(Project.Descriptor.ProjectType).HasDolLightmaps;
    }

    /// <summary>How many parts are lit with lightmaps</summary>
    public int LightmappedPartCount => Parts.Values.Count(p => p.Binding != null);

    /// <summary>How many parts are waiting for their lightmaps to load</summary>
    public int PendingPartCount { get; private set; }

    /// <summary>
    /// A map piece to light: <see cref="Key" /> names it from frame to frame (the Map Editor's entity), and the
    /// lightmap goes on <see cref="Proxy" />, its current mesh
    /// </summary>
    public record LightmapPart(object Key, MSB3.Part.MapPiece Piece, MeshRenderableProxy Proxy);

    /// <summary>
    /// A map's pieces, and its BTAB entries by part name, read only if a piece needs them
    /// </summary>
    public record LightmapMap(string MapId, IEnumerable<LightmapPart> Parts,
        Func<Dictionary<string, List<BTAB.Entry>>> GetAtlas);

    /// <summary>
    /// Gives the loaded maps' pieces their lightmaps once their textures are on the GPU, lets go of those of parts no
    /// longer loaded, and uploads the table with the GPARAM's Dominant Light Map values. Called every frame.
    /// </summary>
    public void Update(IEnumerable<MapContainer> maps, GparamLighting lighting)
    {
        Update(maps.Select(map => new LightmapMap(
            map.Name,
            map.Parts
                .Where(entity => entity.WrappedObject is MSB3.Part.MapPiece)
                .Select(entity => new LightmapPart(entity, (MSB3.Part.MapPiece)entity.WrappedObject,
                    entity.RenderSceneMesh as MeshRenderableProxy)),
            () => GetAtlasEntries(map))), lighting);
    }

    /// <summary>
    /// <see cref="Update(IEnumerable{MapContainer}, GparamLighting)" /> for any map pieces, which don't have to be
    /// the Map Editor's: the Gparam Editor's viewport draws its map on its own, with a bank of its own
    /// </summary>
    public void Update(IEnumerable<LightmapMap> maps, GparamLighting lighting)
    {
        var table = SceneRenderer.Lightmaps;

        if (table == null)
            return;

        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var pending = 0;

        if (CanUse())
        {
            foreach (var map in maps)
            {
                var code = GetMapCode(map.MapId);

                if (code == null)
                    continue;

                var index = GetAreaIndex(map.MapId.Substring(0, 3));
                Dictionary<string, List<BTAB.Entry>> atlas = null;
                var mapPending = 0;
                var mapLit = 0;
                var mapPieces = 0;

                foreach (var part in map.Parts)
                {
                    mapPieces++;

                    seen.Add(part.Key);

                    if (!Parts.TryGetValue(part.Key, out var state))
                    {
                        state = new PartState();
                        Parts[part.Key] = state;
                    }

                    if (!state.Done)
                    {
                        if (index is not { Ready: true })
                        {
                            pending++;
                            mapPending++;
                            continue;
                        }

                        atlas ??= map.GetAtlas();

                        if (!TryResolve(table, index, code, part.Piece, atlas, state))
                        {
                            pending++;
                            mapPending++;
                            continue;
                        }
                    }

                    if (state.Binding != null)
                        mapLit++;

                    // Also after the part's mesh is made again, as for a new model
                    state.Proxy = part.Proxy;

                    if (part.Proxy != null && !ReferenceEquals(part.Proxy.Lightmap, state.Binding))
                        part.Proxy.Lightmap = state.Binding;
                }

                // Temporary diagnostics until checked in the app
                if (mapPending == 0 && mapPieces > 0 && ReportedMaps.Add(map.MapId))
                    Smithbox.Log(this, $"Lightmaps of {map.MapId}: {mapLit} of {mapPieces} map pieces lit, {table.Count} entries");
            }
        }

        PendingPartCount = pending;

        foreach (var key in Parts.Keys.Where(k => !seen.Contains(k)).ToList())
        {
            Release(table, Parts[key]);
            Parts.Remove(key);
        }

        // The game's scale of the lightmaps isn't known, so the profile's and the viewport's go on the GPARAM's color
        var scale = GparamProfile.For(Project.Descriptor.ProjectType).LightmapScale *
                    Math.Max(CFG.Current.Viewport_Map_Lightmap_Intensity, 0.0f);

        table.SetGlobals(lighting?.DominantSharpnessOffset ?? 0.0f, (lighting?.DominantColor ?? Vector3.One) * scale);
        table.Upload();
    }

    /// <summary>
    /// The part's entries, if its textures are on the GPU; true once it is settled, with or without lightmaps
    /// </summary>
    private bool TryResolve(LightmapTable table, AreaIndex index, string code, MSB3.Part.MapPiece piece,
        Dictionary<string, List<BTAB.Entry>> atlas, PartState state)
    {
        var model = piece.ModelName;

        if (string.IsNullOrEmpty(model))
        {
            state.Done = true;
            return true;
        }

        var prefix = $"{code}_{model}_gi_{Bake}_00_dol_";

        // An instanced piece: its place in an atlas. Every part has one BTAB entry (2,370 of 2,370 in m30_00), whose
        // MaterialName is only one of the model's materials, so the atlas lights all of them
        if (atlas.TryGetValue(piece.Name, out var entries) && entries.Count > 0)
        {
            var entry = entries[0];
            var suffix = $"_atlas{entry.AtlasID:D3}";
            var color = $"{prefix}00{suffix}";
            var direction = $"{prefix}01{suffix}";

            if (index.Names.Contains(color) && index.Names.Contains(direction))
            {
                if (!TryGetTexture(color, out var colorTexture) || !TryGetTexture(direction, out var directionTexture))
                    return false;

                if (table.Add(new LightmapData
                    {
                        ColorTexture = colorTexture,
                        DirectionTexture = directionTexture,
                        UvScaleOffset = new Vector4(entry.UVScale, entry.UVOffset.X, entry.UVOffset.Y),
                        HdrScale = new Vector4(index.HdrScales.GetValueOrDefault(color, Vector3.One), 1.0f)
                    }) is { } added)
                {
                    state.Binding = LightmapBinding.ForAll(added);
                }
            }

            state.Done = true;
            return true;
        }

        // A unique piece: its own textures, whole
        var ownColor = $"{prefix}00";
        var ownDirection = $"{prefix}01";

        if (!index.Names.Contains(ownColor) || !index.Names.Contains(ownDirection))
        {
            state.Done = true;
            return true;
        }

        if (!TryGetTexture(ownColor, out var ownColorTexture) || !TryGetTexture(ownDirection, out var ownDirectionTexture))
            return false;

        if (table.Add(new LightmapData
            {
                ColorTexture = ownColorTexture,
                DirectionTexture = ownDirectionTexture,
                UvScaleOffset = new Vector4(1.0f, 1.0f, 0.0f, 0.0f),
                HdrScale = new Vector4(index.HdrScales.GetValueOrDefault(ownColor, Vector3.One), 1.0f)
            }) is { } entryIndex)
        {
            state.Binding = LightmapBinding.ForAll(entryIndex);
        }

        state.Done = true;
        return true;
    }

    private static bool TryGetTexture(string name, out uint texture)
    {
        texture = 0;

        if (!ResourceManager.DefaultPool.TryFind<TextureResource>(name, out var handle))
            return false;

        var gpu = handle?.Get()?.GPUTexture;

        if (gpu == null || !gpu.Resident || gpu.Pool != SceneRenderer.GlobalTexturePool)
            return false;

        texture = gpu.TexHandle;
        return true;
    }

    private static void Release(LightmapTable table, PartState state)
    {
        if (state.Binding == null)
            return;

        foreach (var entry in state.Binding.Entries)
            table.Remove(entry);

        if (state.Proxy != null && ReferenceEquals(state.Proxy.Lightmap, state.Binding))
            state.Proxy.Lightmap = null;

        state.Binding = null;
    }

    /// <summary>
    /// The BTAB entries of a map's _0000 file (the first otherwise) by part name, as the map holds them, so edits
    /// count
    /// </summary>
    private static Dictionary<string, List<BTAB.Entry>> GetAtlasEntries(MapContainer map)
    {
        var byPart = new Dictionary<string, List<BTAB.Entry>>(StringComparer.OrdinalIgnoreCase);
        var parents = map.LightAtlasParents;

        if (parents == null || parents.Count == 0)
            return byPart;

        var parent = parents.FirstOrDefault(p => p.Name != null && p.Name.Contains($"_{Bake}", StringComparison.OrdinalIgnoreCase))
                     ?? parents[0];

        foreach (var child in parent.Children)
        {
            if (child.WrappedObject is not BTAB.Entry entry || string.IsNullOrEmpty(entry.PartName))
                continue;

            if (!byPart.TryGetValue(entry.PartName, out var list))
            {
                list = new List<BTAB.Entry>();
                byPart[entry.PartName] = list;
            }

            list.Add(entry);
        }

        return byPart;
    }

    /// <summary>mAA_BB_CC_DD as the lightmaps name it, AABBCCDD; null for any other name</summary>
    private static string GetMapCode(string mapId)
    {
        if (mapId == null || mapId.Length < 12 || mapId[0] is not ('m' or 'M'))
            return null;

        return $"{mapId.Substring(1, 2)}{mapId.Substring(4, 2)}{mapId.Substring(7, 2)}{mapId.Substring(10, 2)}";
    }

    /// <summary>
    /// The names of an area's lightmaps and the HDR scales of their DOL 1 textures, read once in the background. Null
    /// or not yet ready while it is read.
    /// </summary>
    private AreaIndex GetAreaIndex(string area)
    {
        lock (Areas)
        {
            if (Areas.TryGetValue(area, out var existing))
                return existing;

            var index = new AreaIndex();
            Areas[area] = index;

            Task.Run(() => ReadAreaIndex(area, index));

            return index;
        }
    }

    private void ReadAreaIndex(string area, AreaIndex index)
    {
        var bhdPath = $"/map/{area}/gi_env_{area}.tpfbhd";
        var bdtPath = $"/map/{area}/gi_env_{area}.tpfbdt";

        try
        {
            var fs = Project.VFS.FS;

            if (!fs.FileExists(bhdPath) || !fs.FileExists(bdtPath))
                return;

            using var binder = BXF4.Read(fs.ReadFile(bhdPath).Value, fs.ReadFile(bdtPath).Value);

            foreach (var file in binder.Files)
            {
                var name = Path.GetFileName(file.Name ?? "");

                if (!name.EndsWith(".tpf.dcx", StringComparison.OrdinalIgnoreCase))
                    continue;

                name = name[..^".tpf.dcx".Length];

                if (name.EndsWith("_l", StringComparison.OrdinalIgnoreCase))
                    continue;

                index.Names.Add(name);

                // The HDR scale is DOL 1's, in its TPF entry
                if (!name.Contains("_dol_00", StringComparison.OrdinalIgnoreCase) ||
                    !name.Contains($"_gi_{Bake}_", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var tpf = TPF.Read(file.Bytes);
                    var values = tpf.Textures.FirstOrDefault()?.FloatStruct?.Values;

                    if (values is { Count: >= 3 })
                        index.HdrScales[name] = new Vector3(values[0], values[1], values[2]);
                }
                catch (Exception)
                {
                    // Left at 1
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"Failed to read the lightmaps of {area}: {bhdPath}", e);
        }
        finally
        {
            // Temporary diagnostics until checked in the app
            Smithbox.Log(this, $"Lightmaps of {area}: {index.Names.Count} textures, {index.HdrScales.Count} HDR scales of bake {Bake}");
            index.Ready = true;
        }
    }

    /// <summary>Forget every part's lightmaps, so they are found again (after the setting is switched)</summary>
    public void Reset()
    {
        var table = SceneRenderer.Lightmaps;

        foreach (var state in Parts.Values)
        {
            if (table != null)
                Release(table, state);
        }

        Parts.Clear();
    }
}
