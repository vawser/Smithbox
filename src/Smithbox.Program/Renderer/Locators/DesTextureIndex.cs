using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
/// Which Demon's Souls texture containers hold each texture name. DES models often name a texture by a path whose
/// container doesn't have it (463 slots): c6050's head textures are in the m06 and m08 area TPFs, not in any chr
/// file. The path rules of <see cref="TextureLocator.GetDesReferencedTextureContainers"/> can't find those, so this
/// index is the fallback.
/// <para>
/// It is built once per project in the background, from every chr TPF, common parts TPF, partsbnd, objbnd and map
/// TPF (about 1700 containers), and cached in .smithbox/Cache. The cache is rebuilt when the list of containers
/// changes. Textures are found by name in the pool, so a container loaded after the index is ready still fills the
/// materials that asked for its textures.
/// </para>
/// </summary>
public static class DesTextureIndex
{
    private const int CacheVersion = 1;

    /// <summary>A container the pool can load, and how much it is preferred (lower first)</summary>
    public record Container(string FilePath, string VirtualPath, bool Archive, int Rank)
    {
        public ResourceDescriptor ToDescriptor()
        {
            ResourceDescriptor d = new();
            if (Archive)
                d.AssetArchiveVirtualPath = VirtualPath;
            else
                d.AssetVirtualPath = VirtualPath;
            return d;
        }
    }

    public class Index
    {
        public List<Container> Containers { get; set; } = new();

        /// <summary>Texture name (lower case) to indices into <see cref="Containers"/></summary>
        public Dictionary<string, List<int>> Textures { get; set; } = new();

        public IEnumerable<Container> Find(string textureName)
        {
            return Textures.TryGetValue(textureName.ToLowerInvariant(), out var list)
                ? list.Select(i => Containers[i])
                : Enumerable.Empty<Container>();
        }
    }

    private class CacheFile
    {
        public int Version { get; set; }
        public Index Index { get; set; }
    }

    private static readonly ConditionalWeakTable<ProjectEntry, Task<Index>> _builds = new();

    /// <summary>The project's index, built in the background on first use</summary>
    public static Task<Index> Get(ProjectEntry project)
    {
        return _builds.GetValue(project, p => Task.Run(() => LoadOrBuild(p)));
    }

    private static Index LoadOrBuild(ProjectEntry project)
    {
        try
        {
            var containers = ListContainers(project);
            var cachePath = Path.Combine(project.Descriptor.ProjectPath, ".smithbox", "Cache", "DES Texture Index.json");

            if (File.Exists(cachePath))
            {
                try
                {
                    var cached = JsonSerializer.Deserialize<CacheFile>(File.ReadAllText(cachePath));
                    if (cached?.Version == CacheVersion && cached.Index != null &&
                        cached.Index.Containers.Select(c => c.FilePath).SequenceEqual(containers.Select(c => c.FilePath), StringComparer.OrdinalIgnoreCase))
                    {
                        return cached.Index;
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(typeof(DesTextureIndex), "The DES texture index cache could not be read, so it is rebuilt.", e);
                }
            }

            var watch = Stopwatch.StartNew();
            var index = Build(project, containers);
            watch.Stop();
            Smithbox.Log(typeof(DesTextureIndex),
                $"Built the DES texture index: {index.Textures.Count} texture names in {index.Containers.Count} containers, {watch.Elapsed.TotalSeconds:0.0} s.");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
                File.WriteAllText(cachePath, JsonSerializer.Serialize(new CacheFile { Version = CacheVersion, Index = index }));
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(DesTextureIndex), "The DES texture index could not be cached.", e);
            }

            return index;
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(DesTextureIndex), "The DES texture index could not be built.", e);
            return new Index();
        }
    }

    /// <summary>
    /// Every texture container of the project, one file each (the plain copy when there is one, as it needs no
    /// decompression), in a stable order.
    /// </summary>
    private static List<Container> ListContainers(ProjectEntry project)
    {
        var byKey = new Dictionary<string, Container>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in project.Locator.FileDictionary.Entries)
        {
            var container = ToContainer(entry);
            if (container == null)
                continue;

            var key = container.VirtualPath;
            var plain = !entry.Path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase);

            if (!byKey.TryGetValue(key, out var existing) || (plain && existing.FilePath.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase)))
            {
                byKey[key] = container;
            }
        }

        return byKey.Values.OrderBy(c => c.Rank).ThenBy(c => c.VirtualPath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// The container a file is to the pool, using the same virtual paths as <see cref="TextureLocator"/>, or null when
    /// it isn't one.
    /// </summary>
    private static Container? ToContainer(FileDictionaryEntry entry)
    {
        if (entry.Path == null || entry.Folder == null || entry.Filename == null)
            return null;

        var folder = entry.Folder.Replace('\\', '/').TrimEnd('/').ToLowerInvariant();
        var name = entry.Filename.ToLowerInvariant();
        var segs = folder.Split('/', StringSplitOptions.RemoveEmptyEntries);

        switch (entry.Extension?.ToLowerInvariant())
        {
            // chr/cXXXX/cXXXX.tpf, the loose TPF the editors load for a character
            case "tpf" when segs.Length == 2 && segs[0] == "chr" && segs[1] == name:
                return new Container(entry.Path, $"chr/{name}/tex", false, 0);

            // parts/common_body.tpf, parts/common_ghost.tpf
            case "tpf" when segs.Length == 1 && segs[0] == "parts" && name.StartsWith("common_"):
                return new Container(entry.Path, $"aat/{name}/tex", false, 1);

            case "partsbnd" when segs.Length == 1 && segs[0] == "parts":
                return new Container(entry.Path, $"parts/{name}/tex", true, 2);

            case "objbnd" when segs.Length == 1 && segs[0] == "obj":
                return new Container(entry.Path, $"obj/{name}/tex", true, 3);

            // map/mAA_BB_CC_DD/mAA_BB_CC_DD.tpf, the map's own TPF
            case "tpf" when segs.Length == 2 && segs[0] == "map" && segs[1].Length > 3 && segs[1] == name:
                return new Container(entry.Path, $"map/tex/{name}/own", false, 4);

            // map/mAA/mAA_NNNN.tpf, the area TPFs
            case "tpf" when segs.Length == 2 && segs[0] == "map" && segs[1].Length == 3 && name.Length == 8 && name.StartsWith(segs[1] + "_"):
                return new Container(entry.Path, $"map/tex/{segs[1]}/{name.Substring(4, 4)}", false, 5);
        }

        return null;
    }

    private static Index Build(ProjectEntry project, List<Container> containers)
    {
        var names = new ConcurrentDictionary<string, ConcurrentBag<int>>();

        Parallel.For(0, containers.Count, i =>
        {
            var container = containers[i];

            try
            {
                var data = project.VFS.FS.ReadFile(container.FilePath);
                if (data == null)
                    return;

                IEnumerable<TPF> tpfs = container.Archive
                    ? BND3.Read(data.Value).Files
                        // Some objbnds (o0050, o3104, o7999) hold a 0-byte TPF placeholder
                        .Where(f => f.Name.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase) && f.Bytes.Length > 0)
                        .Select(f => TPF.Read(f.Bytes))
                    : new[] { TPF.Read(data.Value) };

                foreach (var tpf in tpfs)
                {
                    foreach (var texture in tpf.Textures)
                    {
                        names.GetOrAdd(texture.Name.ToLowerInvariant(), _ => new()).Add(i);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(DesTextureIndex), $"The DES texture index skipped {container.FilePath}.", e);
            }
        });

        return new Index
        {
            Containers = containers,
            Textures = names.ToDictionary(x => x.Key, x => x.Value.Distinct().OrderBy(i => i).ToList())
        };
    }
}
