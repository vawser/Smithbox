#nullable enable
using StudioCore.Application;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace StudioCore.Renderer;

/// <summary>
/// Larger copies of character textures that another character's container holds under a prefixed name, from the
/// shipped <c>Assets/Texture Upgrades/&lt;GAME&gt;.json</c>. BB's c1020 chrbnd has only small textures
/// (c1020_Coat_a 64x128), while c4110's holds c4110_c1020_Coat_a at 512x1024. A model asking for the original is given
/// the copy, which the texture name index (or the character rule) then loads from its container.
/// <para>
/// Only copies strictly larger than the original are listed: many prefixed copies are the same size or smaller.
/// Generated from vanilla data by Claude/Tools/BbChrTexProbe (--pairs). See Claude/Documentation/Materials and Textures.md (Material fidelity).
/// </para>
/// </summary>
public static class TextureUpgrades
{
    public const int SupportedVersion = 1;

    private readonly record struct Upgrade(string To, string From);

    private static readonly ConcurrentDictionary<ProjectType, Lazy<Dictionary<string, Upgrade>?>> _tables = new();

    /// <summary>
    /// The name to look a texture up by: its larger copy where the table has one, else the name itself. A project that
    /// has its own copy of the original's container keeps the original, as it may have edited it.
    /// </summary>
    public static string Resolve(ProjectEntry project, string normalizedName)
    {
        var table = _tables.GetOrAdd(project.Descriptor.ProjectType, g => new Lazy<Dictionary<string, Upgrade>?>(() => Load(g))).Value;

        if (table == null || !table.TryGetValue(normalizedName, out var upgrade))
            return normalizedName;

        if (project.VFS.ProjectFS.FileExists(upgrade.From))
            return normalizedName;

        return upgrade.To;
    }

    private static Dictionary<string, Upgrade>? Load(ProjectType game)
    {
        var path = Path.Join(AppContext.BaseDirectory, "Assets", "Texture Upgrades", $"{game}.json");

        if (!File.Exists(path))
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;

            if (root.GetProperty("Version").GetInt32() != SupportedVersion)
            {
                Smithbox.LogError(typeof(TextureUpgrades), $"The texture upgrade table {path} has an unsupported version, so it isn't used.");
                return null;
            }

            var table = new Dictionary<string, Upgrade>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in root.GetProperty("Upgrades").EnumerateObject())
            {
                var to = entry.Value.GetProperty("To").GetString();
                var from = entry.Value.GetProperty("From").GetString();

                if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                    table[entry.Name] = new Upgrade(to, from);
            }

            return table;
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(TextureUpgrades), $"The texture upgrade table {path} could not be read.", e);
            return null;
        }
    }
}
