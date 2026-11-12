using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Reads collision triangles for the first person mode straight from the game files, without the resource manager or
/// the collision bank, so it can run off the main thread. Only depends on SoulsFormats and the unified Havok library,
/// so the survey tool links it as it is. Every method takes a file reader (project-relative path to bytes, or null if
/// missing), which is the project's VFS in Smithbox.
/// </summary>
public static class FirstPersonCollisionReader
{
    /// <summary>
    /// The lookup name of a collision file: its file name without folders, .dcx or .hkx, lower case. The same as
    /// Smithbox's ResourceName.Normalize for these files.
    /// </summary>
    public static string NormalizeName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        var i = path.LastIndexOfAny(new[] { '/', '\\' });
        var name = i >= 0 ? path.Substring(i + 1) : path;

        var stripped = true;

        while (stripped)
        {
            stripped = false;

            foreach (var ext in new[] { ".dcx", ".hkx" })
            {
                if (name.Length > ext.Length && name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - ext.Length);
                    stripped = true;
                }
            }
        }

        return name.ToLowerInvariant();
    }

    /// <summary>
    /// The binder of a map's collision of one type (h, l or f). ER, NR and AC6 keep it in the area's folder
    /// (map/m30/m30_00_00_00/l30_00_00_00.hkxbhd); DS3, SDT, BB and DB_NT in the map's own (map/m30_00_00_00/...).
    /// </summary>
    public static (string Bhd, string Bdt) MapBinderPaths(string game, string mapId, string type)
    {
        var folder = game is "DS2S" ? "model/map"
            : game is "DS3" or "SDT" or "BB" or "DB_NT" or "DS1R" ? $"map/{mapId}"
            : $"map/{mapId.Substring(0, 3)}/{mapId}";

        return ($"{folder}/{type}{mapId.Substring(1)}.hkxbhd", $"{folder}/{type}{mapId.Substring(1)}.hkxbdt");
    }

    /// <summary>
    /// Where a collision part's map collision of one type (h, l or f) is: a split binder and the entry in it, or a
    /// loose file (entry null). As the game files are laid out, checked by FirstPersonSurvey's listing:
    /// <list type="bullet">
    /// <item>ER, NR, AC6, DB_NT, DS3, SDT, BB: binder entry <c>l10_00_00_00_000100</c> (map, then the model's number)</item>
    /// <item>DS1R: binder (BXF3) entry <c>l0000B0A10</c> (the model's number, then A and the area)</item>
    /// <item>DS1: loose <c>map/m10_00_00_00/l0000B0A10.hkx</c></item>
    /// <item>DES: loose <c>map/m01_00_00_00/l0000B0.hkx</c></item>
    /// <item>DS2S: binder <c>model/map/l10_02_00_00.hkxbhd</c>, entry <c>l01_0200</c> (the model's name)</item>
    /// </list>
    /// </summary>
    public static (string Container, string Entry) MapCollisionSource(string game, string mapId, string model, string type)
    {
        var number = model.Substring(1);

        switch (game)
        {
            case "DES":
                return ($"map/{mapId}/{type}{number}.hkx", null);
            case "DS1":
                return ($"map/{mapId}/{type}{number}A{mapId.Substring(1, 2)}.hkx", null);
            case "DS1R":
                return (MapBinderPaths(game, mapId, type).Bhd, $"{type}{number}A{mapId.Substring(1, 2)}");
            case "DS2S":
                return (MapBinderPaths(game, mapId, type).Bhd, $"{type}{number}");
            default:
                return (MapBinderPaths(game, mapId, type).Bhd, $"{type}{mapId.Substring(1)}_{number}");
        }
    }

    /// <summary>
    /// Whether a collision part's hit filter is the one the player collides with: 8 in every game but DS2, whose
    /// filters are named and whose player collision is Map (100).
    /// </summary>
    public static bool IsPlayerHitFilter(string game, int hitFilter)
        => game is "DS2" or "DS2S" ? hitFilter == 100 : hitFilter == 8;

    /// <summary>
    /// A map collision binder, read once: its entries' bytes by lookup name, decoded to triangles on request.
    /// </summary>
    public sealed class MapBinder
    {
        private readonly Dictionary<string, byte[]> Entries = new();
        private readonly Havok.HavokProfile Profile;
        private readonly Havok.HavokCompendium Compendium;

        public IReadOnlyCollection<string> Names => Entries.Keys;

        private MapBinder(Havok.HavokProfile profile, Havok.HavokCompendium compendium)
        {
            Profile = profile;
            Compendium = compendium;
        }

        /// <summary>Reads a map's binder of one type, or returns null if it isn't there.</summary>
        public static MapBinder Read(Func<string, byte[]> readFile, string game, string mapId, string type)
            => ReadAt(readFile, game, MapBinderPaths(game, mapId, type).Bhd);

        /// <summary>
        /// Reads a split binder (.hkxbhd beside its .hkxbdt) at a path, or returns null if it isn't there. DES, DS1 and
        /// DS1R binders are BXF3; the others BXF4.
        /// </summary>
        public static MapBinder ReadAt(Func<string, byte[]> readFile, string game, string bhdPath)
        {
            var profile = Havok.HavokProfile.FromGame(game);

            if (profile == null)
                return null;

            var i = bhdPath.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);

            if (i < 0)
                return null;

            var bdtPath = string.Concat(bhdPath.AsSpan(0, i), "bdt", bhdPath.AsSpan(i + 3));
            var bhd = readFile(bhdPath);
            var bdt = bhd == null ? null : readFile(bdtPath);

            if (bhd == null || bdt == null)
                return null;

            using BinderReader binder = IsLegacy(game) ? new BXF3Reader(bhd, bdt) : new BXF4Reader(bhd, bdt);

            Havok.HavokCompendium compendium = null;
            var compendiumHeader = binder.Files.FirstOrDefault(f => f.Name?.Contains(".compendium", StringComparison.OrdinalIgnoreCase) ?? false);

            if (compendiumHeader != null)
            {
                var bytes = binder.ReadFile(compendiumHeader);
                var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;
                compendium = Havok.Files.HavokFile.ReadCompendium(data.ToArray(), profile);
            }

            var result = new MapBinder(profile, compendium);

            foreach (var header in binder.Files)
            {
                if (header.Name == null || !header.Name.Contains(".hkx", StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Entries.TryAdd(NormalizeName(header.Name), binder.ReadFile(header).ToArray());
            }

            return result;
        }

        public bool Contains(string name) => Entries.ContainsKey(NormalizeName(name));

        /// <summary>A file's triangles (three corners each) in the space of its part. Null if it isn't in the binder.</summary>
        public List<Vector3> ReadTriangles(string name, Action<string> onError = null)
        {
            if (!Entries.TryGetValue(NormalizeName(name), out var bytes))
                return null;

            return TrianglesOf(bytes, Profile, Compendium, name, onError);
        }
    }

    /// <summary>DES, DS1 and DS1R keep their binders as BND3 and BXF3.</summary>
    public static bool IsLegacy(string game) => game is "DES" or "DS1" or "DS1R";

    /// <summary>
    /// A collision file's triangles in the space of its part, every submesh in one list. Null if it can't be read.
    /// DS2's 32-bit collision is read by SoulsFormats' HKX, as Smithbox's viewport does.
    /// </summary>
    public static List<Vector3> TrianglesOf(byte[] bytes, Havok.HavokProfile profile, Havok.HavokCompendium compendium,
        string name, Action<string> onError = null)
    {
        try
        {
            var data = DCX.Is(bytes) ? DCX.Decompress(bytes).ToArray() : bytes;

            if (profile.Game == "DS2")
                return SoulsTriangles(HKX.Read(data));

            var document = Havok.Files.HavokFile.Read(data, profile, compendium);

            return TrianglesOf(document, name, onError);
        }
        catch (Exception e)
        {
            onError?.Invoke($"{name}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// The triangles of a SoulsFormats HKX (DS2): its extended mesh storages, each a vertex list and index quads (the
    /// fourth index unused), as HKX_Helper reads them for the viewport.
    /// </summary>
    private static List<Vector3> SoulsTriangles(HKX hkx)
    {
        var triangles = new List<Vector3>();

        foreach (var obj in hkx.DataSection.Objects)
        {
            if (obj is not HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage mesh)
                continue;

            var indices = mesh.Indices8?.Capacity > 0 ? mesh.Indices8.GetArrayData().Elements.Select(e => (uint)e.data).ToList()
                : mesh.Indices16?.Capacity > 0 ? mesh.Indices16.GetArrayData().Elements.Select(e => (uint)e.data).ToList()
                : mesh.Indices32.GetArrayData().Elements.Select(e => e.data).ToList();
            var vertices = mesh.Vertices.GetArrayData().Elements;

            for (var i = 0; i + 3 < indices.Count; i += 4)
            {
                if (indices[i] >= vertices.Count || indices[i + 1] >= vertices.Count || indices[i + 2] >= vertices.Count)
                    continue;

                foreach (var k in new[] { indices[i], indices[i + 1], indices[i + 2] })
                {
                    var v = vertices[(int)k].Vector;
                    triangles.Add(new Vector3(v.X, v.Y, v.Z));
                }
            }
        }

        return triangles;
    }

    /// <summary>
    /// A collision file wherever its locator puts it: a loose .hkx (DES and DS1 map collision), an entry of a split
    /// binder (.hkxbhd, by its name), or an entry of a binder (objbnd, bnd, geomhkxbnd). Null if it isn't there.
    /// </summary>
    public static List<Vector3> ReadCollisionFile(Func<string, byte[]> readFile, string game, string containerPath,
        IEnumerable<string> names, Action<string> onError = null)
    {
        var profile = Havok.HavokProfile.FromGame(game);

        if (profile == null || string.IsNullOrEmpty(containerPath))
            return null;

        var lower = containerPath.ToLowerInvariant();

        if (lower.EndsWith(".hkx") || lower.EndsWith(".hkx.dcx"))
        {
            var bytes = readFile(containerPath);
            return bytes == null ? null : TrianglesOf(bytes, profile, null, containerPath, onError);
        }

        if (lower.Contains(".hkxbhd"))
        {
            var binder = MapBinder.ReadAt(readFile, game, containerPath);

            if (binder == null)
                return null;

            foreach (var name in names)
            {
                if (!string.IsNullOrEmpty(name) && binder.Contains(name))
                    return binder.ReadTriangles(name, onError);
            }

            return null;
        }

        return ReadAssetTriangles(readFile, game, containerPath, names, onError);
    }

    /// <summary>A read document's triangles, every submesh in one list.</summary>
    public static List<Vector3> TrianglesOf(Havok.HavokDocument document, string name, Action<string> onError = null)
    {
        var triangles = new List<Vector3>();

        foreach (var submesh in CollisionGeometry.Submeshes(document.Objects,
            (shape, e) => onError?.Invoke($"{name}: {shape?.GetType().Name}: {e.Message}")))
        {
            triangles.AddRange(submesh);
        }

        return triangles;
    }

    /// <summary>
    /// The collision of an asset or object, from the container its locator gives (ER's geomhkxbnd, DS3's objbnd).
    /// The entry used is the one named as the asset (by any of the names given), else the binder's only collision
    /// file. Null if there is none or it can't be read.
    /// </summary>
    public static List<Vector3> ReadAssetTriangles(Func<string, byte[]> readFile, string game, string containerPath,
        IEnumerable<string> names, Action<string> onError = null)
    {
        var profile = Havok.HavokProfile.FromGame(game);

        if (profile == null)
            return null;

        var bytes = readFile(containerPath);

        if (bytes == null)
            return null;

        var wanted = new HashSet<string>(names.Where(n => !string.IsNullOrEmpty(n)).Select(NormalizeName));

        try
        {
            var entries = new List<(string Name, byte[] Bytes)>();
            Havok.HavokCompendium compendium = null;

            CollectHkx(bytes, entries, ref compendium, profile, 0);

            var entry = entries.FirstOrDefault(e => wanted.Contains(NormalizeName(e.Name)));

            if (entry.Bytes == null && entries.Count == 1)
                entry = entries[0];

            // No file of the asset's own: the others are skeletons, cloth and breakage pieces (DS3, SDT), so the
            // asset has no standing collision
            if (entry.Bytes == null)
                return null;

            return TrianglesOf(entry.Bytes, profile, compendium, entry.Name, onError);
        }
        catch (Exception e)
        {
            onError?.Invoke($"{containerPath}: {e.Message}");
            return null;
        }
    }

    /// <summary>The .hkx entries of a binder, and nested binders, with the compendium if one is found.</summary>
    private static void CollectHkx(byte[] bytes, List<(string Name, byte[] Bytes)> entries, ref Havok.HavokCompendium compendium,
        Havok.HavokProfile profile, int depth)
    {
        // BND3 for the older games (by its magic, as some of them ship both)
        var data = DCX.Is(bytes) ? DCX.Decompress(bytes).ToArray() : bytes;
        using BinderReader binder = data.Length >= 4 && data[0] == (byte)'B' && data[1] == (byte)'N' && data[2] == (byte)'D' && data[3] == (byte)'3'
            ? new BND3Reader(data) : new BND4Reader(data);

        foreach (var header in binder.Files)
        {
            var name = header.Name ?? "";
            var fileName = Path.GetFileName(name.Replace('\\', '/'));

            if (fileName.Contains(".compendium", StringComparison.OrdinalIgnoreCase))
            {
                var entry = binder.ReadFile(header);
                compendium ??= Havok.Files.HavokFile.ReadCompendium((DCX.Is(entry) ? DCX.Decompress(entry) : entry).ToArray(), profile);
            }
            else if (fileName.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".hkx.dcx", StringComparison.OrdinalIgnoreCase))
            {
                entries.Add((fileName, binder.ReadFile(header).ToArray()));
            }
            else if (depth < 2 && (fileName.Contains("bnd", StringComparison.OrdinalIgnoreCase)))
            {
                CollectHkx(binder.ReadFile(header).ToArray(), entries, ref compendium, profile, depth + 1);
            }
        }
    }

    /// <summary>Places a triangle list by a world matrix, into a new list.</summary>
    public static List<Vector3> Place(List<Vector3> local, Matrix4x4 world)
    {
        var placed = new List<Vector3>(local.Count);

        foreach (var v in local)
            placed.Add(Vector3.Transform(v, world));

        return placed;
    }
}
