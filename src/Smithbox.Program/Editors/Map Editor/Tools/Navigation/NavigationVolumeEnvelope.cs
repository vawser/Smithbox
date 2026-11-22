using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The overall shape of an Armored Core VI flying navigation volume (hkaiNavVolume), as an outline for the Navigation
/// tool's overlay: the volume's outer envelope seen from above.
///
/// The volume is a grid of ~10 m voxels whose free space the cells tile. The space under the terrain is free too (the
/// volume isn't filled inside collision), so the lowest free voxel of a column is just the grid's floor. The envelope is
/// therefore each column's top air band: from the highest free voxel down to the first solid voxel below it, i.e. the
/// flight space over the terrain or a roof. Columns are grouped into tiles (at most <see cref="MaxTiles"/> on a side)
/// so the largest maps stay light, and a tile counts a height as air when at least half its columns are free there.
///
/// It draws, as lines: the floor shell and the ceiling shell (a line between each two neighbouring tiles' heights), a
/// vertical line at each tile on the edge of the flight space, and the grid's bounding box. Shell lines that lie on the
/// grid's own top or bottom are left out, as the bounding box already draws those planes.
/// </summary>
public static class NavVolumeEnvelope
{
    /// <summary>The most tiles on the longer horizontal side of the grid.</summary>
    public const int MaxTiles = 128;

    /// <summary>The envelope's lines in the volume's own space (map space).</summary>
    public static List<(Vector3 A, Vector3 B)> Build(Havok.hk2019.hkaiNavVolume volume, int maxTiles = MaxTiles)
    {
        var lines = new List<(Vector3, Vector3)>();
        int rx = volume.m_res[0], ry = volume.m_res[1], rz = volume.m_res[2];
        if (rx <= 0 || ry <= 0 || rz <= 0 || volume.m_cells.Count == 0)
            return lines;

        var scale = new Vector3(volume.m_quantizationScale.X, volume.m_quantizationScale.Y, volume.m_quantizationScale.Z);
        var offset = new Vector3(volume.m_quantizationOffset.X, volume.m_quantizationOffset.Y, volume.m_quantizationOffset.Z);
        Vector3 World(float x, float y, float z) => offset + new Vector3(x, y, z) * scale;

        int tile = Math.Max(1, (Math.Max(rx, rz) + maxTiles - 1) / maxTiles);
        int ntx = (rx + tile - 1) / tile, ntz = (rz + tile - 1) / tile;

        // Free columns per tile and height
        var cover = new int[(long)ntx * ntz * ry];
        long Index(int tx, int tz, int y) => ((long)tx * ntz + tz) * ry + y;

        foreach (var c in volume.m_cells)
        {
            for (int tx = c.m_min[0] / tile; tx <= (c.m_max[0] - 1) / tile; tx++)
            {
                int ox = Math.Min(c.m_max[0], (tx + 1) * tile) - Math.Max(c.m_min[0], tx * tile);
                for (int tz = c.m_min[2] / tile; tz <= (c.m_max[2] - 1) / tile; tz++)
                {
                    int oz = Math.Min(c.m_max[2], (tz + 1) * tile) - Math.Max(c.m_min[2], tz * tile);
                    int area = ox * oz;
                    for (int y = c.m_min[1]; y < c.m_max[1]; y++)
                        cover[Index(tx, tz, y)] += area;
                }
            }
        }

        // Each tile's top air band, in voxel heights; -1 where the tile has no air
        var floor = new int[ntx, ntz];
        var ceiling = new int[ntx, ntz];
        for (int tx = 0; tx < ntx; tx++)
            for (int tz = 0; tz < ntz; tz++)
            {
                int cols = (Math.Min(rx, (tx + 1) * tile) - tx * tile) * (Math.Min(rz, (tz + 1) * tile) - tz * tile);
                bool Air(int y) => cover[Index(tx, tz, y)] * 2 >= cols;

                int top = ry - 1;
                while (top >= 0 && !Air(top)) top--;
                if (top < 0) { floor[tx, tz] = ceiling[tx, tz] = -1; continue; }

                int bottom = top;
                while (bottom > 0 && Air(bottom - 1)) bottom--;
                floor[tx, tz] = bottom;
                ceiling[tx, tz] = top + 1;
            }

        Vector3 Centre(int tx, int tz, int y) => World(
            (tx * tile + Math.Min(rx, (tx + 1) * tile)) / 2f, y,
            (tz * tile + Math.Min(rz, (tz + 1) * tile)) / 2f);

        for (int tx = 0; tx < ntx; tx++)
            for (int tz = 0; tz < ntz; tz++)
            {
                if (ceiling[tx, tz] < 0) continue;
                bool edge = false;

                foreach (var (dx, dz) in new[] { (1, 0), (0, 1), (-1, 0), (0, -1) })
                {
                    int nx = tx + dx, nz = tz + dz;
                    if (nx < 0 || nz < 0 || nx >= ntx || nz >= ntz || ceiling[nx, nz] < 0) { edge = true; continue; }

                    // Each pair once, from the tile with the lower index
                    if (dx < 0 || dz < 0) continue;

                    if (!(floor[tx, tz] == 0 && floor[nx, nz] == 0))
                        lines.Add((Centre(tx, tz, floor[tx, tz]), Centre(nx, nz, floor[nx, nz])));
                    if (!(ceiling[tx, tz] == ry && ceiling[nx, nz] == ry))
                        lines.Add((Centre(tx, tz, ceiling[tx, tz]), Centre(nx, nz, ceiling[nx, nz])));
                }

                if (edge)
                    lines.Add((Centre(tx, tz, floor[tx, tz]), Centre(tx, tz, ceiling[tx, tz])));
            }

        // The grid's bounding box
        var lo = World(0, 0, 0);
        var hi = World(rx, ry, rz);
        Vector3 Corner(int i) => new((i & 1) != 0 ? hi.X : lo.X, (i & 2) != 0 ? hi.Y : lo.Y, (i & 4) != 0 ? hi.Z : lo.Z);
        for (int i = 0; i < 8; i++)
            for (int bit = 1; bit < 8; bit <<= 1)
                if ((i & bit) == 0)
                    lines.Add((Corner(i), Corner(i | bit)));

        return lines;
    }
}
