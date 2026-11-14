#nullable enable
using StudioCore.Application;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Renderer;

/// <summary>
/// Which texture containers of a game hold each texture name, from the shipped
/// <c>Assets/Texture Indexes/&lt;GAME&gt;.json</c>. It covers textures whose model names a container that doesn't
/// have them, e.g. DES c6050's head textures are only in map/m06/m06_0020.tpf. It is the last rule of
/// <see cref="ResourceFallbacks.LoadMissingTextures" />.
/// <para>
/// The files are generated from vanilla game data by Claude/Tools/TextureIndexGen, so users never scan anything.
/// Textures a project adds in new containers aren't in them. Names the AET rule finds, and containers no model reads
/// from (envmaps, DOL lightmaps, colour grading, menu, font), are left out of the files.
/// See Claude/Documentation/Texture Name Index Plan.md.
/// </para>
/// </summary>
public static class TextureNameIndex
{
    public const int SupportedVersion = 1;

    /// <summary>A container the pool can load by path, and how much it is preferred (lower first)</summary>
    public readonly record struct Container(string Path, int Rank);

    public sealed class Index
    {
        public Container[] Containers = Array.Empty<Container>();
        public Dictionary<string, int[]> Textures = new(StringComparer.Ordinal);

        /// <summary>The containers holding a texture, by its normalized name (<see cref="ResourceName.Normalize" />)</summary>
        public IEnumerable<Container> Find(string name)
        {
            return Textures.TryGetValue(name, out var ids) ? ids.Select(i => Containers[i]) : Enumerable.Empty<Container>();
        }
    }

    private static readonly ConcurrentDictionary<ProjectType, Lazy<Index?>> _indexes = new();

    /// <summary>The shipped index of a game, read once; null when the game has none or it can't be read</summary>
    public static Index? Get(ProjectType game)
    {
        return _indexes.GetOrAdd(game, g => new Lazy<Index?>(() => Load(g))).Value;
    }

    private static Index? Load(ProjectType game)
    {
        var path = Path.Join(AppContext.BaseDirectory, "Assets", "Texture Indexes", $"{game}.json");

        if (!File.Exists(path))
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;

            if (root.GetProperty("Version").GetInt32() != SupportedVersion)
            {
                Smithbox.LogError(typeof(TextureNameIndex), $"The texture index {path} has an unsupported version, so it isn't used.");
                return null;
            }

            var containers = root.GetProperty("Containers").EnumerateArray()
                .Select(c => new Container(c.GetProperty("Path").GetString() ?? "", c.GetProperty("Rank").GetInt32()))
                .ToArray();

            var textures = new Dictionary<string, int[]>(StringComparer.Ordinal);
            foreach (var t in root.GetProperty("Textures").EnumerateObject())
            {
                textures[t.Name] = t.Value.EnumerateArray().Select(v => v.GetInt32()).ToArray();
            }

            return new Index { Containers = containers, Textures = textures };
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(TextureNameIndex), $"The texture index {path} could not be read.", e);
            return null;
        }
    }

    /// <summary>
    /// The container to load for a missing texture: none when the index doesn't know the name, a holder is loaded
    /// already (so loading again would add nothing) or is in <paramref name="queued" /> (being loaded for another
    /// texture), else the most preferred holder that exists and hasn't been tried.
    /// </summary>
    public static string? ChooseContainer(ProjectEntry project, ResourcePool pool, string name, ISet<string> tried,
        ISet<string>? queued = null)
    {
        var index = Get(project.Descriptor.ProjectType);
        if (index == null)
            return null;

        var holders = index.Find(name).ToList();
        if (holders.Count == 0 || holders.Any(h => pool.IsContainerLoaded(h.Path) || queued?.Contains(h.Path) == true))
            return null;

        foreach (var holder in holders.OrderBy(h => h.Rank))
        {
            if (tried.Contains(holder.Path))
                continue;

            if (project.VFS.FS.FileExists(holder.Path))
                return holder.Path;
        }

        return null;
    }
}
