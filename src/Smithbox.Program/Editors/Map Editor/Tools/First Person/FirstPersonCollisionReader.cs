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
        var folder = game is "DS3" or "SDT" or "BB" or "DB_NT"
            ? $"map/{mapId}"
            : $"map/{mapId.Substring(0, 3)}/{mapId}";

        return ($"{folder}/{type}{mapId.Substring(1)}.hkxbhd", $"{folder}/{type}{mapId.Substring(1)}.hkxbdt");
    }

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

        /// <summary>Reads a binder, or returns null if it isn't there.</summary>
        public static MapBinder Read(Func<string, byte[]> readFile, string game, string mapId, string type)
        {
            var profile = Havok.HavokProfile.FromGame(game);

            if (profile == null)
                return null;

            var (bhdPath, bdtPath) = MapBinderPaths(game, mapId, type);
            var bhd = readFile(bhdPath);
            var bdt = bhd == null ? null : readFile(bdtPath);

            if (bhd == null || bdt == null)
                return null;

            using var binder = new BXF4Reader(bhd, bdt);

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

    /// <summary>
    /// A collision file's triangles in the space of its part, every submesh in one list. Null if it can't be read.
    /// </summary>
    public static List<Vector3> TrianglesOf(byte[] bytes, Havok.HavokProfile profile, Havok.HavokCompendium compendium,
        string name, Action<string> onError = null)
    {
        try
        {
            var data = DCX.Is(bytes) ? DCX.Decompress(bytes).ToArray() : bytes;
            var document = Havok.Files.HavokFile.Read(data, profile, compendium);

            return TrianglesOf(document, name, onError);
        }
        catch (Exception e)
        {
            onError?.Invoke($"{name}: {e.Message}");
            return null;
        }
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
        using var binder = new BND4Reader(bytes);

        foreach (var header in binder.Files)
        {
            var name = header.Name ?? "";
            var fileName = Path.GetFileName(name.Replace('\\', '/'));

            if (fileName.Contains(".compendium", StringComparison.OrdinalIgnoreCase))
            {
                var data = binder.ReadFile(header);
                compendium ??= Havok.Files.HavokFile.ReadCompendium((DCX.Is(data) ? DCX.Decompress(data) : data).ToArray(), profile);
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
