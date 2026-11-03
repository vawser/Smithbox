using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Holds the baked irradiance volumes of Armored Core VI, Elden Ring and Nightreign maps.
/// A volume is placed by an EnvMapEffectBox region, which its texture fills exactly, and has one bake per
/// envmap file index (probably times of day). Each bake has an IVINFO in the map's ivinfobnd and four 3D
/// textures, GIIV####_##_W/X/Y/Z, in the map's envmap TPFs.
/// A box is linked to its volume by a property only, never by its name: EnvMapEffectBox_UnkT32 in AC6,
/// UnkT0A in ER and Unk0A in NR. The game reads ER/NR IDs from the name when that property is -1, as every
/// vanilla box has it, which IrradianceVolumeIdTool can write into the property.
/// See Documentation/Full GI for how the game uses this data.
/// </summary>
public class IrradianceVolumeBank
{
    public ProjectEntry Project;

    // The quality level read; the others hold the same volumes at lower resolutions
    private const string Quality = "high";

    private static readonly Regex InfoNamePattern = new(@"_GIIV(\d{4})_(\d{2})\.ivinfo$", RegexOptions.IgnoreCase);

    private readonly Dictionary<string, MapVolumes> Maps = new(StringComparer.OrdinalIgnoreCase);

    public IrradianceVolumeBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.NR)
            return true;

        return false;
    }

    /// <summary>
    /// Whether the game reads a box's volume ID from its name when the ID property is -1
    /// </summary>
    public bool HasNameFallback()
    {
        return Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR;
    }

    /// <summary>
    /// The volumes of one map
    /// </summary>
    public class MapVolumes
    {
        /// <summary>
        /// The IVINFO of each volume and bake
        /// </summary>
        public Dictionary<(int Id, int Bake), IVINFO> Infos = new();

        /// <summary>
        /// The envmap files of each bake: one tpfbnd in ER and NR, one TPF per texture in AC6
        /// </summary>
        public Dictionary<int, List<FileDictionaryEntry>> BakeFiles = new();

        public IEnumerable<int> Bakes => Infos.Keys.Select(k => k.Bake).Distinct().OrderBy(b => b);

        public bool HasVolume(int id) => Infos.Keys.Any(k => k.Id == id);

        public bool HasVolume(int id, int bake) => Infos.ContainsKey((id, bake));

        public int CountInBake(int bake) => Infos.Keys.Count(k => k.Bake == bake);
    }

    public void LoadMap(MapContainer map)
    {
        if (!CanUse())
            return;

        var volumes = new MapVolumes();
        var infoEntry = Project.Locator.IrradianceVolumeFiles.Entries
            .FirstOrDefault(e => e.Filename.Equals($"{map.Name}_{Quality}", StringComparison.OrdinalIgnoreCase));

        if (infoEntry != null)
        {
            try
            {
                var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

                if (fs.FileExists(infoEntry.Path))
                {
                    var binder = BND4.Read(fs.ReadFile(infoEntry.Path).Value);

                    foreach (var file in binder.Files)
                    {
                        // The game builds these names as "%s_GIIV%04d_%02d" and "%s.ivInfo"
                        var match = InfoNamePattern.Match(Path.GetFileName(file.Name.Replace('\\', '/')));

                        if (!match.Success)
                            continue;

                        volumes.Infos[(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value))] = IVINFO.Read(file.Bytes);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_GI_Failed_Read_IvInfo", infoEntry.Path), e);
            }
        }

        volumes.BakeFiles = GetBakeFiles(map.Name);

        lock (Maps)
        {
            Maps[map.Name] = volumes;
        }

        // Temporary diagnostics for checking the links, see Full GI Plan.md phase 1
        var summary = Summarise(map);
        Smithbox.Log(this, LOC.Get("MAP_GI_Load_Summary",
            map.Name,
            volumes.Infos.Count,
            string.Join(" ", volumes.Bakes.Select(b => $"{b:D2} ({volumes.CountInBake(b)})")),
            volumes.BakeFiles.Values.Sum(f => f.Count),
            summary.Boxes,
            summary.Linked,
            summary.Unlinked,
            summary.LinkedWithoutVolume,
            summary.SkippedByGame));
    }

    /// <summary>
    /// The envmap files holding a map's volumes, by bake. Read from the file list, so it can be used before the
    /// map is loaded, when its textures are queued.
    /// ER/NR: mAA_BB_CC_DD_envmap_00_high_00, AC6: mAA_BB_CC_DD_envmap_00_high_w (and _x, _y, _z)
    /// </summary>
    public Dictionary<int, List<FileDictionaryEntry>> GetBakeFiles(string mapId)
    {
        var bakeFiles = new Dictionary<int, List<FileDictionaryEntry>>();

        if (!CanUse() || mapId == null)
            return bakeFiles;

        var bakePattern = new Regex($@"^{Regex.Escape(mapId)}_envmap_(\d{{2}})_{Quality}_(\w+)$", RegexOptions.IgnoreCase);

        foreach (var entry in Project.Locator.EnvMapTextureFiles.Entries)
        {
            var match = bakePattern.Match(entry.Filename);

            if (!match.Success)
                continue;

            // AC6's _rem file is the reflection cubemap, not part of the volume
            if (match.Groups[2].Value.Equals("rem", StringComparison.OrdinalIgnoreCase))
                continue;

            var bake = int.Parse(match.Groups[1].Value);

            if (!bakeFiles.TryGetValue(bake, out var files))
            {
                files = new List<FileDictionaryEntry>();
                bakeFiles[bake] = files;
            }

            files.Add(entry);
        }

        return bakeFiles;
    }

    public void OnUnloadMap(string mapId)
    {
        lock (Maps)
        {
            Maps.Remove(mapId);
        }
    }

    public MapVolumes GetMap(string mapId)
    {
        lock (Maps)
        {
            return mapId != null && Maps.TryGetValue(mapId, out var volumes) ? volumes : null;
        }
    }

    /// <summary>
    /// The texture names of a volume's bake, in the order the shaders sample them: X, Y, Z, then W
    /// </summary>
    public static string[] GetTextureNames(string mapId, int id, int bake)
    {
        return new[] { "X", "Y", "Z", "W" }.Select(c => $"{mapId}_GIIV{id:D4}_{bake:D2}_{c}").ToArray();
    }

    /// <summary>
    /// The property that links a region to its volume, or null if the region is not an EnvMapEffectBox
    /// </summary>
    public static PropertyInfo GetVolumeIdProperty(object region)
    {
        var name = region switch
        {
            MSB_AC6.Region.EnvMapEffectBox => nameof(MSB_AC6.Region.EnvMapEffectBox.EnvMapEffectBox_UnkT32),
            MSBE.Region.EnvironmentMapEffectBox => nameof(MSBE.Region.EnvironmentMapEffectBox.UnkT0A),
            MSB_NR.Region.EnvMapEffectBox => nameof(MSB_NR.Region.EnvMapEffectBox.Unk0A),
            _ => null
        };

        return name == null ? null : region.GetType().GetProperty(name);
    }

    /// <summary>
    /// The volume ID a region holds, or null if it is not an EnvMapEffectBox. Negative means no volume.
    /// </summary>
    public static short? GetVolumeId(object region)
    {
        return GetVolumeIdProperty(region)?.GetValue(region) as short?;
    }

    /// <summary>
    /// Whether the game skips an ER/NR EnvMapEffectBox entirely, even with its ID property set:
    /// it only loads boxes whose shape is a Box and whose name is exactly 10 characters
    /// </summary>
    public bool IsSkippedByGame(object region)
    {
        if (!HasNameFallback() || region is not IMsbRegion r)
            return false;

        return r.Shape is not MSB.Shape.Box || r.Name == null || r.Name.Length != 10;
    }

    /// <summary>
    /// How many volumes the last table update gave the shaders
    /// </summary>
    public int ShadedVolumeCount { get; private set; }

    private IrradianceVolumeData[] _tableEntries = new IrradianceVolumeData[IrradianceVolumeTable.Capacity];

    /// <summary>
    /// Rebuild the volume table the shaders light with, from the loaded maps' linked boxes whose four textures of
    /// the bake are on the GPU. Done every frame, so moved, resized or relinked boxes follow at once; the table is
    /// only uploaded when it changes.
    /// </summary>
    public void UpdateGpuTable(IEnumerable<MapContainer> maps, int bake)
    {
        var table = SceneRenderer.IrradianceVolumes;

        if (table == null)
            return;

        var found = new List<(IrradianceVolumeData Data, int Priority, float Size)>();

        if (CanUse())
        {
            foreach (var map in maps)
            {
                var volumes = GetMap(map.Name);

                if (volumes == null)
                    continue;

                foreach (var entity in map.Regions)
                {
                    if (TryBuildEntry(map.Name, entity, volumes, bake, out var data, out var priority, out var size))
                        found.Add((data, priority, size));
                }
            }
        }

        // Coarse first, so finer volumes blend over them, as nested boxes do in game
        found.Sort((a, b) => a.Priority != b.Priority ? a.Priority.CompareTo(b.Priority) : b.Size.CompareTo(a.Size));

        var count = Math.Min(found.Count, IrradianceVolumeTable.Capacity);

        for (var i = 0; i < count; i++)
            _tableEntries[i] = found[i].Data;

        ShadedVolumeCount = count;
        table.Upload(_tableEntries, count);
    }

    private bool TryBuildEntry(string mapId, Entity entity, MapVolumes volumes, int bake,
        out IrradianceVolumeData data, out int priority, out float size)
    {
        data = default;
        priority = 0;
        size = 0;

        var region = entity.WrappedObject;
        var id = GetVolumeId(region);

        if (id == null || id < 0 || IsSkippedByGame(region) || !volumes.HasVolume(id.Value, bake))
            return false;

        var textures = new TexturePool.TextureHandle[4];
        var names = GetTextureNames(mapId, id.Value, bake);

        for (var i = 0; i < 4; i++)
        {
            if (!ResourceManager.DefaultPool.TryFind<TextureResource>(names[i], out var handle))
                return false;

            var gpu = handle?.Get()?.GPUTexture;

            if (gpu == null || !gpu.Resident || gpu.Pool != SceneRenderer.GlobalVolumeTexturePool)
                return false;

            textures[i] = gpu;
        }

        float fadeDistance;
        float diffuseScale;

        switch (region)
        {
            case MSBE.Region.EnvironmentMapEffectBox er:
                fadeDistance = er.FadeDistance;
                priority = er.Priority;
                diffuseScale = er.AmbientDiffuseIntensity;
                break;
            case MSB_NR.Region.EnvMapEffectBox nr:
                fadeDistance = nr.FadeDistance;
                priority = nr.Priority;
                diffuseScale = nr.AmbientDiffuseIntensity;
                break;
            case MSB_AC6.Region.EnvMapEffectBox ac6:
                // The same layout as ER's: TransitionDist is the fade distance, UnkT09 the priority
                fadeDistance = ac6.TransitionDist;
                priority = ac6.EnvMapEffectBox_UnkT09;
                diffuseScale = 1.0f;
                break;
            default:
                return false;
        }

        // The region's unit box spans -0.5 to 0.5 on X and Z and 0 to 1 on Y (its position is the bottom
        // centre); the game's volume space spans -1 to 1 on every axis
        var world = entity.GetWorldMatrix();

        if (!Matrix4x4.Invert(world, out var inverse))
            return false;

        Matrix4x4.Decompose(world, out var scale, out _, out _);

        data.WorldToLocal = inverse * Matrix4x4.CreateScale(2.0f) * Matrix4x4.CreateTranslation(0.0f, -1.0f, 0.0f);
        data.ExtentsFade = new Vector4(scale * 0.5f, fadeDistance > 0 ? 1.0f / fadeDistance : 1.0e4f);
        data.VoxelCountsScale = new Vector4(textures[0].Width, textures[0].Height, textures[0].Depth, diffuseScale);
        data.TextureX = textures[0].TexHandle;
        data.TextureY = textures[1].TexHandle;
        data.TextureZ = textures[2].TexHandle;
        data.TextureW = textures[3].TexHandle;
        data.Info = new Vector4(id.Value, bake, priority, 0);

        size = MathF.Abs(scale.X * scale.Y * scale.Z);

        return true;
    }

    public struct LinkSummary
    {
        public int Boxes;
        public int Linked;
        public int Unlinked;
        public int LinkedWithoutVolume;
        public int SkippedByGame;
    }

    public LinkSummary Summarise(MapContainer map)
    {
        var summary = new LinkSummary();
        var volumes = GetMap(map.Name);

        foreach (var entity in map.Regions)
        {
            var id = GetVolumeId(entity.WrappedObject);

            if (id == null)
                continue;

            summary.Boxes++;

            if (IsSkippedByGame(entity.WrappedObject))
                summary.SkippedByGame++;

            if (id < 0)
            {
                summary.Unlinked++;
            }
            else if (volumes == null || !volumes.HasVolume(id.Value))
            {
                summary.LinkedWithoutVolume++;
            }
            else
            {
                summary.Linked++;
            }
        }

        return summary;
    }
}
