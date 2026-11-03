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
/// Boxes are linked to volumes as the game links them, see <see cref="ResolveVolumeId" />: in ER and NR by the
/// ID property (UnkT0A in ER, Unk0A in NR) when it is 0 or more, or else by the name (Env_Box203 is volume 203);
/// every vanilla box leaves the property at -1. In AC6 by the EnvMapEffectBox_UnkT32 property.
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

        lock (RequestedBakes)
        {
            RequestedBakes.RemoveWhere(r => r.MapId.Equals(mapId, StringComparison.OrdinalIgnoreCase));
            ResidentBakes.RemoveWhere(r => r.MapId.Equals(mapId, StringComparison.OrdinalIgnoreCase));
        }
    }

    // The bakes whose envmap files have been queued for each map, and those whose textures are all on the GPU
    private readonly HashSet<(string MapId, int Bake)> RequestedBakes = new();
    private readonly HashSet<(string MapId, int Bake)> ResidentBakes = new();

    // The bakes the volumes are lit with while they follow the time of day
    private (int From, int To, float Ratio)? FollowedBakes;

    /// <summary>
    /// Whether the bakes follow the map lighting's time of day instead of the Global Illumination tab's settings. Only Elden Ring's
    /// bakes are known to match the GPARAM timezones (bake N = timezone N).
    /// </summary>
    public bool BakesFollowTime => Project.Descriptor.ProjectType is ProjectType.ER &&
        CFG.Current.Viewport_Map_Lighting && CFG.Current.Viewport_Map_Lighting_Follow_Bakes;

    /// <summary>
    /// The bakes whose envmap files are loaded with a map: those of the time of day when the bakes follow it,
    /// otherwise the Global Illumination tab's bake and the bake it blends towards
    /// </summary>
    public HashSet<int> GetBakesToLoad()
    {
        if (BakesFollowTime)
        {
            var (from, to, _) = GparamLighting.TimezonesAt(CFG.Current.Viewport_Map_Lighting_Hours);
            return new HashSet<int> { from, to };
        }

        var bakes = new HashSet<int> { CFG.Current.Viewport_GI_Bake };

        if (CFG.Current.Viewport_GI_Bake_Blend)
            bakes.Add(CFG.Current.Viewport_GI_Blend_Bake);

        return bakes;
    }

    /// <summary>
    /// Notes that a map's load queued a bake's envmap files, so <see cref="RequestBake" /> doesn't queue them again
    /// </summary>
    public void MarkBakeRequested(string mapId, int bake)
    {
        lock (RequestedBakes)
        {
            RequestedBakes.Add((mapId, bake));
        }
    }

    /// <summary>
    /// Queues the envmap files of a loaded map's bake, once, so the volumes can change to it
    /// </summary>
    public void RequestBake(string mapId, int bake)
    {
        lock (RequestedBakes)
        {
            if (!RequestedBakes.Add((mapId, bake)))
                return;
        }

        if (!GetBakeFiles(mapId).TryGetValue(bake, out var files) || files.Count == 0)
            return;

        var assets = files.Select(f => TextureLocator.GetEnvMapTextureVirtualPath(mapId, f.Filename)).ToList();
        var jobName = LOC.Get("MAP_GI_Load_Bake", mapId, bake);

        ResourceManager.DefaultPool.LoadAssets(Project, assets, new LoadOptions
        {
            JobName = jobName,
            Types = ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly
        });
    }

    /// <summary>
    /// Whether every volume of a map's bake has its textures on the GPU. True if the map has no such bake, as there
    /// is nothing to wait for.
    /// </summary>
    public bool IsBakeResident(string mapId, int bake)
    {
        lock (RequestedBakes)
        {
            if (ResidentBakes.Contains((mapId, bake)))
                return true;
        }

        var volumes = GetMap(mapId);

        if (volumes == null || !volumes.Bakes.Contains(bake))
            return true;

        foreach (var key in volumes.Infos.Keys.Where(k => k.Bake == bake))
        {
            if (!TryGetBakeTextures(mapId, key.Id, bake, out _))
                return false;
        }

        lock (RequestedBakes)
        {
            ResidentBakes.Add((mapId, bake));
        }

        return true;
    }

    /// <summary>
    /// The bake, the bake to blend towards (-1 for none) and how far, to light the loaded maps' volumes with this
    /// frame. When the bakes follow the time of day, the timezones either side of it, queued for every loaded map;
    /// the volumes keep the bakes they have until the new ones are on the GPU.
    /// </summary>
    public (int Bake, int BlendBake, float BlendRatio) ResolveBakes(IEnumerable<MapContainer> maps)
    {
        if (!BakesFollowTime)
        {
            FollowedBakes = null;

            return (CFG.Current.Viewport_GI_Bake,
                CFG.Current.Viewport_GI_Bake_Blend ? CFG.Current.Viewport_GI_Blend_Bake : -1,
                CFG.Current.Viewport_GI_Blend_Ratio);
        }

        var wanted = GparamLighting.TimezonesAt(CFG.Current.Viewport_Map_Lighting_Hours);
        var ready = true;

        foreach (var map in maps)
        {
            RequestBake(map.Name, wanted.From);
            RequestBake(map.Name, wanted.To);

            ready &= IsBakeResident(map.Name, wanted.From) && IsBakeResident(map.Name, wanted.To);
        }

        var current = FollowedBakes;

        // The same pair only moves its ratio; a new pair waits for its textures, unless there is nothing yet
        if (ready || current == null || (current.Value.From == wanted.From && current.Value.To == wanted.To))
        {
            current = wanted;
            FollowedBakes = current;
        }

        return (current.Value.From, current.Value.Ratio > 0.0f ? current.Value.To : -1, current.Value.Ratio);
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
    /// The value of a region's volume ID property, or null if it is not an EnvMapEffectBox. Negative means none.
    /// </summary>
    public static short? GetVolumeId(object region)
    {
        return GetVolumeIdProperty(region)?.GetValue(region) as short?;
    }

    /// <summary>
    /// Why a box's name gives no volume ID
    /// </summary>
    public enum NameIssue
    {
        None,
        NotBox,
        NameLength,
        NotDigits
    }

    /// <summary>
    /// The ID an ER/NR box's name gives, by the game's rule: it loads only Box shapes with names of exactly
    /// 10 characters (e.g. Env_Box203), and takes characters 7 to 9 as a decimal number
    /// </summary>
    public static NameIssue ReadIdFromName(IMsbRegion region, out short id)
    {
        id = -1;

        if (region.Shape is not MSB.Shape.Box)
            return NameIssue.NotBox;

        var name = region.Name;

        if (name == null || name.Length != 10)
            return NameIssue.NameLength;

        // The game doesn't check these are digits, but anything else gives an ID that means nothing
        if (!char.IsAsciiDigit(name[7]) || !char.IsAsciiDigit(name[8]) || !char.IsAsciiDigit(name[9]))
            return NameIssue.NotDigits;

        id = (short)((name[7] - '0') * 100 + (name[8] - '0') * 10 + (name[9] - '0'));

        return NameIssue.None;
    }

    /// <summary>
    /// Where a box's volume ID came from
    /// </summary>
    public enum LinkSource
    {
        None,
        Name,
        Property
    }

    /// <summary>
    /// The volume a box is linked to, as the game finds it. -1 if it has none, the game skips the box, or the
    /// region is not an EnvMapEffectBox.
    /// ER/NR (traced in eldenring.exe): boxes that aren't a Box shape or whose name isn't exactly 10 characters
    /// are skipped; the ID is characters 7 to 9 of the name, unless the ID property is 0 or more, which wins.
    /// AC6: the ID property.
    /// </summary>
    public short ResolveVolumeId(object region, out LinkSource source)
    {
        source = LinkSource.None;

        var property = GetVolumeId(region);

        if (property == null)
            return -1;

        if (!HasNameFallback())
        {
            if (property < 0)
                return -1;

            source = LinkSource.Property;
            return property.Value;
        }

        if (IsSkippedByGame(region))
            return -1;

        if (property >= 0)
        {
            source = LinkSource.Property;
            return property.Value;
        }

        // The game's arithmetic, which doesn't check for digits: other characters give some other number
        var name = ((IMsbRegion)region).Name;
        var nameId = (name[7] - '0') * 100 + (name[8] - '0') * 10 + (name[9] - '0');

        if (nameId < 0 || nameId > short.MaxValue)
            return -1;

        source = LinkSource.Name;
        return (short)nameId;
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

    // The light of the viewport's default environment map, which models are lit by outside the volumes
    private const double AmbientTarget = 0.5;

    /// <summary>
    /// Rebuild the volume table the shaders light with, from the loaded maps' linked boxes whose four textures of
    /// the bake are on the GPU. Done every frame, so moved, resized or relinked boxes follow at once; the table is
    /// only uploaded when it changes.
    /// </summary>
    /// <param name="blendBake">The bake to blend towards, as the game does between times of day, or -1</param>
    /// <param name="blendRatio">How far towards it, 0 to 1</param>
    public void UpdateGpuTable(IEnumerable<MapContainer> maps, int bake, int blendBake, float blendRatio)
    {
        var table = SceneRenderer.IrradianceVolumes;

        if (table == null)
            return;

        var found = new List<(IrradianceVolumeData Data, int Priority, float Size)>();
        var luminanceSum = 0.0;
        var luminanceCount = 0;

        if (CanUse())
        {
            foreach (var map in maps)
            {
                var volumes = GetMap(map.Name);

                if (volumes == null)
                    continue;

                foreach (var entity in map.Regions)
                {
                    if (TryBuildEntry(map.Name, entity, volumes, bake, blendBake, blendRatio,
                            out var data, out var priority, out var size))
                    {
                        found.Add((data, priority, size));

                        // The IVINFO's L0 is the volume's average luminance
                        var luminance = volumes.Infos[((int)data.Info.X, bake)].ShLuminance.W;

                        if (luminance > 0)
                        {
                            luminanceSum += luminance;
                            luminanceCount++;
                        }
                    }
                }
            }
        }

        // Coarse first, so finer volumes blend over them, as nested boxes do in game
        found.Sort((a, b) => a.Priority != b.Priority ? a.Priority.CompareTo(b.Priority) : b.Size.CompareTo(a.Size));

        var count = Math.Min(found.Count, IrradianceVolumeTable.Capacity);

        for (var i = 0; i < count; i++)
            _tableEntries[i] = found[i].Data;

        ShadedVolumeCount = count;

        // Their average light becomes the viewport's default ambient light, 0.5
        table.AutoExposure = luminanceCount > 0 ? (float)(AmbientTarget / (luminanceSum / luminanceCount)) : 1.0f;

        table.Upload(_tableEntries, count);
    }

    /// <summary>
    /// The four textures of a volume's bake, if all are on the GPU in the volume pool
    /// </summary>
    private static bool TryGetBakeTextures(string mapId, int id, int bake, out TexturePool.TextureHandle[] textures)
    {
        textures = new TexturePool.TextureHandle[4];
        var names = GetTextureNames(mapId, id, bake);

        for (var i = 0; i < 4; i++)
        {
            if (!ResourceManager.DefaultPool.TryFind<TextureResource>(names[i], out var handle))
                return false;

            var gpu = handle?.Get()?.GPUTexture;

            if (gpu == null || !gpu.Resident || gpu.Pool != SceneRenderer.GlobalVolumeTexturePool)
                return false;

            textures[i] = gpu;
        }

        return true;
    }

    private bool TryBuildEntry(string mapId, Entity entity, MapVolumes volumes, int bake, int blendBake,
        float blendRatio, out IrradianceVolumeData data, out int priority, out float size)
    {
        data = default;
        priority = 0;
        size = 0;

        var region = entity.WrappedObject;
        var id = ResolveVolumeId(region, out _);

        // AC6 boxes aren't checked by name, so their shape is checked here; the region's scale is a Box's size
        if (id < 0 || region is not IMsbRegion { Shape: MSB.Shape.Box } || !volumes.HasVolume(id, bake))
            return false;

        if (!TryGetBakeTextures(mapId, id, bake, out var textures))
            return false;

        // The bake to blend towards, if the volume has it and its textures are loaded; otherwise the first again
        var blendTextures = textures;
        var blendInfo = volumes.Infos[(id, bake)];
        var ratio = 0.0f;

        if (blendBake >= 0 && blendBake != bake && blendRatio > 0 && volumes.HasVolume(id, blendBake)
            && TryGetBakeTextures(mapId, id, blendBake, out var loaded))
        {
            blendTextures = loaded;
            blendInfo = volumes.Infos[(id, blendBake)];
            ratio = Math.Clamp(blendRatio, 0.0f, 1.0f);
        }

        float fadeDistance;
        float diffuseScale;
        float specularScale;

        switch (region)
        {
            case MSBE.Region.EnvironmentMapEffectBox er:
                fadeDistance = er.FadeDistance;
                priority = er.Priority;
                diffuseScale = er.AmbientDiffuseIntensity;
                specularScale = er.AmbientSpecularIntensity;
                break;
            case MSB_NR.Region.EnvMapEffectBox nr:
                fadeDistance = nr.FadeDistance;
                priority = nr.Priority;
                diffuseScale = nr.AmbientDiffuseIntensity;
                specularScale = nr.AmbientSpecularIntensity;
                break;
            case MSB_AC6.Region.EnvMapEffectBox ac6:
                // The same layout as ER's: TransitionDist is the fade distance, UnkT09 the priority.
                // SpecularLightMult sits where ER's specular intensity does, but reaches 8, so it isn't used
                // until it is confirmed to scale the reflections.
                fadeDistance = ac6.TransitionDist;
                priority = ac6.EnvMapEffectBox_UnkT09;
                diffuseScale = 1.0f;
                specularScale = 1.0f;
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

        data.Bake0X = textures[0].TexHandle;
        data.Bake0Y = textures[1].TexHandle;
        data.Bake0Z = textures[2].TexHandle;
        data.Bake0W = textures[3].TexHandle;
        data.Bake1X = blendTextures[0].TexHandle;
        data.Bake1Y = blendTextures[1].TexHandle;
        data.Bake1Z = blendTextures[2].TexHandle;
        data.Bake1W = blendTextures[3].TexHandle;

        data.ShLuminance0 = volumes.Infos[(id, bake)].ShLuminance;
        data.ShLuminance1 = blendInfo.ShLuminance;
        data.Info = new Vector4(id, bake, ratio > 0 ? blendBake : bake, ratio);
        data.Params = new Vector4(specularScale, 0, 0, 0);

        // World bounds of the unit box's corners, so the shader can skip the volume before any other work
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (var c = 0; c < 8; c++)
        {
            var corner = new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? 0.0f : 1.0f, (c & 4) == 0 ? -0.5f : 0.5f);
            var p = Vector3.Transform(corner, world);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        data.BoundsMin = new Vector4(min, 0);
        data.BoundsMax = new Vector4(max, 0);

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
            if (GetVolumeId(entity.WrappedObject) == null)
                continue;

            summary.Boxes++;

            if (IsSkippedByGame(entity.WrappedObject))
                summary.SkippedByGame++;

            var id = ResolveVolumeId(entity.WrappedObject, out _);

            if (id < 0)
            {
                summary.Unlinked++;
            }
            else if (volumes == null || !volumes.HasVolume(id))
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
