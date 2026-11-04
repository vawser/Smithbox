using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The light probes of DS3 and Bloodborne maps (BTPB) as volumes for the flver shader, which lights every surface
/// inside them with their light instead of the GPARAM hemisphere, as the game lights its objects with them.
/// <para>
/// Each BTPB group is a regular grid: Unk10/14/18 probes along X/Y/Z (31 x 4 x 7 = 868 in m30_00's first), Unk1C/20/24
/// metres apart (2, sometimes 1.5 or 5), from Unk28. Each probe has 12 coefficients, R0G0B0 R1G1B1 ..., and LightMask,
/// the share of the sun that reaches it (1024 = all). The game's GXEnvMapRegion_IvDiff pass (DS3 DXBC) reads four 3D
/// textures, g_ShVolumeR/G/B/A, as max(dot((n, 1), (R.c, G.c, B.c, A.c)), 0): three directional bands and the
/// constant one, already scaled for irradiance. Here the probes go into four such textures per group.
/// </para>
/// <para>
/// Not traced, taken from the data: the coefficients are fixed point over 1024, as LightMask is (typical L0 500 or so
/// outdoors, 0.49, near DS3's GPARAM hemisphere of 0.5); bands 1-3 are along Y, Z, X, the usual order of L1 spherical
/// harmonics, with Y up (band 1 is mostly positive, light from above); no sign flips.
/// See Documentation/Older Games/Analysis.md.
/// </para>
/// </summary>
public class LightProbeGridBank
{
    public ProjectEntry Project;

    /// <summary>The fixed point scale of a probe's coefficients and its LightMask</summary>
    public const float CoefficientScale = 1.0f / 1024.0f;

    private class Grid
    {
        public int CountX;
        public int CountY;
        public int CountZ;
        public Vector3 Origin;
        public Vector3 Spacing;
        public int ProbeCount;

        // X, Y, Z (the directional bands) and W (the constant band, with LightMask in alpha)
        public TexturePool.TextureHandle[] Textures;
        public bool Uploaded;
    }

    // Per group object, so a group edited in place keeps its textures until its probe count changes
    private readonly Dictionary<BTPB.Group, Grid> Grids = new(ReferenceEqualityComparer.Instance);

    public LightProbeGridBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        return GparamProfile.For(Project.Descriptor.ProjectType).HasLightProbes;
    }

    /// <summary>How many probe grids the shaders lit with last frame</summary>
    public int ShadedGridCount { get; private set; }

    /// <summary>
    /// Adds the shaders' entries for the probe grids of the loaded maps whose textures are on the GPU, uploading
    /// those not made yet, and lets go of the textures of grids no longer loaded. Called every frame with the
    /// irradiance volume table.
    /// </summary>
    public void AppendEntries(IEnumerable<MapContainer> maps, List<(IrradianceVolumeData Data, int Priority, float Size)> found)
    {
        var seen = new HashSet<BTPB.Group>(ReferenceEqualityComparer.Instance);
        var shaded = 0;

        // The GPARAM's Dominant Light Map SH Color Scale x SH R/G/B Scale (DS3 m30: 0.5), from the lighting resolved
        // last frame
        var shColor = SceneRenderer.MapLighting?.ShColor ?? Vector3.One;

        if (CanUse() && SceneRenderer.GlobalVolumeTexturePool != null)
        {
            foreach (var map in maps)
            {
                foreach (var (world, group) in GetGroups(map))
                {
                    seen.Add(group);

                    if (!TryGetGrid(group, out var grid) || !grid.Uploaded ||
                        grid.Textures.Any(t => t == null || !t.Resident))
                        continue;

                    found.Add((BuildEntry(grid, world, found.Count, shColor), 0, MinSpacing(grid)));
                    shaded++;
                }
            }
        }

        ShadedGridCount = shaded;

        foreach (var group in Grids.Keys.Where(g => !seen.Contains(g)).ToList())
        {
            Release(Grids[group]);
            Grids.Remove(group);
        }
    }

    // The table is sorted by this, largest first, so coarser grids come first and finer ones blend over them
    private static float MinSpacing(Grid grid) => Math.Min(grid.Spacing.X, Math.Min(grid.Spacing.Y, grid.Spacing.Z));

    /// <summary>
    /// The groups of a map's light probe file, with the world matrix they are placed by (the map's offset). The
    /// file ending _0000 if the map has several, else the first.
    /// </summary>
    private static IEnumerable<(Matrix4x4 World, BTPB.Group Group)> GetGroups(MapContainer map)
    {
        var parents = map.LightProbeParents;

        if (parents == null || parents.Count == 0)
            yield break;

        var parent = parents.FirstOrDefault(p => p.Name != null && p.Name.EndsWith("_0000", StringComparison.OrdinalIgnoreCase))
                     ?? parents[0];

        foreach (var child in parent.Children)
        {
            if (child is Entity { WrappedObject: BTPB.Group group } entity)
                yield return (entity.GetWorldMatrix(), group);
        }
    }

    private bool TryGetGrid(BTPB.Group group, out Grid grid)
    {
        if (Grids.TryGetValue(group, out grid))
        {
            if (grid.ProbeCount == group.Probes.Count)
                return true;

            // Probes added or removed: made again
            Release(grid);
            Grids.Remove(group);
        }

        grid = null;

        var spacing = new Vector3(group.Unk1C, group.Unk20, group.Unk24);

        if (group.Unk10 <= 0 || group.Unk14 <= 0 || group.Unk18 <= 0 || group.Probes.Count == 0 ||
            spacing.X <= 0 || spacing.Y <= 0 || spacing.Z <= 0)
            return false;

        grid = new Grid
        {
            CountX = group.Unk10,
            CountY = group.Unk14,
            CountZ = group.Unk18,
            Origin = group.Unk28,
            Spacing = spacing,
            ProbeCount = group.Probes.Count
        };

        Grids[group] = grid;
        Upload(grid, group);

        return true;
    }

    private static void Upload(Grid grid, BTPB.Group group)
    {
        var pool = SceneRenderer.GlobalVolumeTexturePool;
        var (nx, ny, nz) = (grid.CountX, grid.CountY, grid.CountZ);
        var texels = nx * ny * nz;

        var x = new float[texels * 4];
        var y = new float[texels * 4];
        var z = new float[texels * 4];
        var w = new float[texels * 4];

        // Placed by position, so the order the probes are listed in doesn't matter
        foreach (var probe in group.Probes)
        {
            var cell = (probe.Position - grid.Origin) / grid.Spacing;
            var i = (int)MathF.Round(cell.X);
            var j = (int)MathF.Round(cell.Y);
            var k = (int)MathF.Round(cell.Z);

            if (i < 0 || j < 0 || k < 0 || i >= nx || j >= ny || k >= nz || probe.Coefficients.Length < 12)
                continue;

            var o = ((k * ny + j) * nx + i) * 4;
            var c = probe.Coefficients;

            for (var channel = 0; channel < 3; channel++)
            {
                w[o + channel] = c[channel] * CoefficientScale;
                y[o + channel] = c[3 + channel] * CoefficientScale;
                z[o + channel] = c[6 + channel] * CoefficientScale;
                x[o + channel] = c[9 + channel] * CoefficientScale;
            }

            w[o + 3] = Math.Clamp(probe.LightMask * CoefficientScale, 0.0f, 1.0f);
        }

        grid.Textures = new TexturePool.TextureHandle[4];

        for (var t = 0; t < 4; t++)
        {
            grid.Textures[t] = pool.AllocateTextureDescriptor();

            if (grid.Textures[t] == null)
            {
                Release(grid);
                return;
            }

            grid.Textures[t].Width = (uint)nx;
            grid.Textures[t].Height = (uint)ny;
            grid.Textures[t].Depth = (uint)nz;
        }

        var data = new[] { x, y, z, w };
        var handles = grid.Textures;

        SceneRenderer.AddBackgroundUploadTask((gd, cl) =>
        {
            // Released before this ran (the map was unloaded)
            if (!ReferenceEquals(grid.Textures, handles))
                return;

            for (var t = 0; t < 4; t++)
            {
                if (handles[t] == null)
                    return;

                handles[t].FillWithGPUTexture(CreateTexture(gd, data[t], (uint)nx, (uint)ny, (uint)nz, $"Light Probes {t}"));
            }

            grid.Uploaded = true;
        });
    }

    private static unsafe Texture CreateTexture(GraphicsDevice gd, float[] texels, uint width, uint height, uint depth, string name)
    {
        TextureDescription description = TextureDescription.Texture3D(width, height, depth, 1,
            VkFormat.R32G32B32A32Sfloat, VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
            VkImageCreateFlags.None, VkImageTiling.Optimal);

        var texture = gd.ResourceFactory.CreateTexture(ref description);
        texture.Name = name;

        fixed (float* pointer = texels)
        {
            gd.UpdateTexture(texture, (IntPtr)pointer, (uint)(texels.Length * sizeof(float)), 0, 0, 0, width, height,
                depth, 0, 0);
        }

        return texture;
    }

    private static void Release(Grid grid)
    {
        if (grid.Textures == null)
            return;

        foreach (var handle in grid.Textures)
            handle?.Dispose();

        grid.Textures = null;
        grid.Uploaded = false;
    }

    /// <summary>
    /// The shader's entry for a grid: a box from half a spacing before the first probe to half a spacing after the
    /// last, so each probe is a texel centre, fading in over one spacing. Params.y marks it as a probe grid.
    /// </summary>
    private static IrradianceVolumeData BuildEntry(Grid grid, Matrix4x4 world, int index, Vector3 shColor)
    {
        var counts = new Vector3(grid.CountX, grid.CountY, grid.CountZ);
        var min = grid.Origin - grid.Spacing * 0.5f;
        var max = grid.Origin + grid.Spacing * (counts - new Vector3(0.5f));
        var center = (min + max) * 0.5f;
        var half = (max - min) * 0.5f;

        var data = new IrradianceVolumeData();

        Matrix4x4.Invert(world, out var inverse);

        // World to the map's space, then to the box's -1 to 1
        data.WorldToLocal = inverse * Matrix4x4.CreateTranslation(-center) *
                            Matrix4x4.CreateScale(new Vector3(1.0f) / Vector3.Max(half, new Vector3(1e-3f)));
        data.ExtentsFade = new Vector4(half, 1.0f / Math.Max(MinSpacing(grid), 0.1f));
        data.VoxelCountsScale = new Vector4(counts, 1.0f);

        var t = grid.Textures;
        data.Bake0X = t[0].TexHandle;
        data.Bake0Y = t[1].TexHandle;
        data.Bake0Z = t[2].TexHandle;
        data.Bake0W = t[3].TexHandle;
        data.Bake1X = t[0].TexHandle;
        data.Bake1Y = t[1].TexHandle;
        data.Bake1Z = t[2].TexHandle;
        data.Bake1W = t[3].TexHandle;

        // Probe entries have no IVINFO: its place holds the GPARAM's scale on the probes' light
        data.ShLuminance0 = new Vector4(shColor, 1.0f);
        data.ShLuminance1 = data.ShLuminance0;
        data.Info = new Vector4(index, 0, 0, 0);
        data.Params = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);

        var boundsMin = new Vector3(float.MaxValue);
        var boundsMax = new Vector3(float.MinValue);

        for (var c = 0; c < 8; c++)
        {
            var corner = new Vector3((c & 1) == 0 ? min.X : max.X, (c & 2) == 0 ? min.Y : max.Y, (c & 4) == 0 ? min.Z : max.Z);
            var p = Vector3.Transform(corner, world);
            boundsMin = Vector3.Min(boundsMin, p);
            boundsMax = Vector3.Max(boundsMax, p);
        }

        data.BoundsMin = new Vector4(boundsMin, 0);
        data.BoundsMax = new Vector4(boundsMax, 0);

        return data;
    }
}
