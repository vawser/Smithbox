using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Standalone (non-GPU, non-async) helpers to pull raw FLVER/TPF bytes directly out of the project's
/// game archives. This intentionally bypasses the ResourceManager/GPU resource pipeline used by the
/// live viewport, since that pipeline discards the source bytes once a texture/model has been uploaded,
/// and in several games skips materializing a full FLVER2 object entirely for performance reasons.
/// </summary>
public static class SoulsContainerIO
{
    private static bool IsBnd3Era(ProjectEntry project)
    {
        return project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    }

    /// <summary>
    /// Reads the raw FLVER bytes for the given model asset, resolving whichever archive/loose file
    /// currently holds it for the active project.
    /// </summary>
    public static byte[] ReadFlverBytes(ProjectEntry project, ResourceDescriptor asset, out string error)
    {
        error = null;

        var virtPath = asset.AssetArchiveVirtualPath ?? asset.AssetVirtualPath;
        if (string.IsNullOrEmpty(virtPath))
        {
            error = "Model has no resolvable virtual path.";
            return null;
        }

        var relPath = PathBuilder.GetRelativePath(project, virtPath);
        if (string.IsNullOrEmpty(relPath))
        {
            error = $"Could not resolve a file path for '{virtPath}'.";
            return null;
        }

        var entryName = asset.AssetName;
        var containerType = ModelEditorUtils.GetContainerTypeFromRelativePath(project, relPath);

        try
        {
            if (containerType is ResourceContainerType.None)
            {
                var data = project.VFS.FS.ReadFile(relPath);
                if (data == null)
                {
                    error = $"Missing file '{relPath}'.";
                    return null;
                }

                return data.Value.ToArray();
            }

            if (containerType is ResourceContainerType.BND)
            {
                var data = project.VFS.FS.ReadFile(relPath);
                if (data == null)
                {
                    error = $"Missing file '{relPath}'.";
                    return null;
                }

                var found = FindInBinder(data.Value, project, name => IsFlverEntry(name, entryName));
                if (found == null)
                    error = $"Could not find FLVER entry '{entryName}' inside '{relPath}'.";

                return found;
            }

            if (containerType is ResourceContainerType.BXF)
            {
                var bhdPath = relPath;
                var bdtPath = relPath.Replace("bhd", "bdt");

                var bhd = project.VFS.FS.ReadFile(bhdPath);
                var bdt = project.VFS.FS.ReadFile(bdtPath);

                if (bhd == null || bdt == null)
                {
                    error = $"Missing split archive for '{relPath}'.";
                    return null;
                }

                var found = FindInSplitBinder(bhd.Value, bdt.Value, project, name => IsFlverEntry(name, entryName));
                if (found == null)
                    error = $"Could not find FLVER entry '{entryName}' inside '{relPath}'.";

                return found;
            }
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        return null;
    }

    private static bool IsFlverEntry(string entryFileName, string modelEntryName)
    {
        var lower = entryFileName.ToLowerInvariant();
        if (!(lower.Contains(".flver") || (lower.Contains(".flv") && !lower.Contains(".flvpwv"))))
            return false;

        return string.Equals(FilePathUtils.GetPureFilename(entryFileName), modelEntryName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves a texture virtual path (as produced by <c>TextureLocator.GetFlverTextureVirtualPath</c>) to
    /// the raw (usually DDS) texture bytes, by locating and reading whichever container currently holds it.
    /// </summary>
    public static byte[] ReadTextureBytes(ProjectEntry project, string textureVirtPath, out string error)
    {
        error = null;

        if (string.IsNullOrEmpty(textureVirtPath))
        {
            error = "Empty texture path.";
            return null;
        }

        var textureName = System.IO.Path.GetFileNameWithoutExtension(textureVirtPath);

        // Map textures are looked up by name inside a small set of shared, per-map packed archives
        // (0000-0003 + env), NOT one archive per texture - "map/tex/{mid}/{texturename}" is a lookup key,
        // not a literal container path, so it needs its own resolution path rather than PathBuilder.
        var pathSegments = textureVirtPath.Split('/');
        if (pathSegments.Length >= 3 && pathSegments[0] == "map" && pathSegments[1] == "tex")
        {
            return ReadMapTextureBytes(project, pathSegments[2], textureName, out error);
        }

        var relPath = PathBuilder.GetRelativePath(project, textureVirtPath);
        if (string.IsNullOrEmpty(relPath))
        {
            error = $"Could not resolve a file path for '{textureVirtPath}'.";
            return null;
        }

        var containerType = GetTextureContainerType(relPath);

        try
        {
            if (containerType is ResourceContainerType.None)
            {
                var data = project.VFS.FS.ReadFile(relPath);
                if (data == null)
                {
                    error = $"Missing file '{relPath}'.";
                    return null;
                }

                return ExtractTextureFromTpfBytes(data.Value.ToArray(), textureName);
            }

            if (containerType is ResourceContainerType.BND)
            {
                var data = project.VFS.FS.ReadFile(relPath);
                if (data == null)
                {
                    error = $"Missing file '{relPath}'.";
                    return null;
                }

                var found = FindTextureInBinder(data.Value, project, textureName);
                if (found == null)
                    error = $"Could not find texture '{textureName}' inside '{relPath}'.";

                return found;
            }

            if (containerType is ResourceContainerType.BXF)
            {
                var bhdPath = relPath;
                var bdtPath = relPath.Replace("bhd", "bdt");

                var bhd = project.VFS.FS.ReadFile(bhdPath);
                var bdt = project.VFS.FS.ReadFile(bdtPath);

                if (bhd == null || bdt == null)
                {
                    error = $"Missing split archive for '{relPath}'.";
                    return null;
                }

                var found = FindTextureInSplitBinder(bhd.Value, bdt.Value, project, textureName);
                if (found == null)
                    error = $"Could not find texture '{textureName}' inside '{relPath}'.";

                return found;
            }
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        return null;
    }

    /// <summary>
    /// Per-archive (relative path -&gt; texture name -&gt; bytes) cache, so a map's packed texture archives
    /// (which can be very large - the whole .tpfbdt is read into memory to open one) only get read and
    /// indexed once per export session, no matter how many of the map's textures get requested.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, byte[]>> _packedTextureArchiveCache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Map textures are pooled into a handful of shared packed archives per map (0000-0003, env, ...) rather
    /// than one archive per texture, so the texture has to be searched for by name across the candidate
    /// archives for that map. <paramref name="mid"/> is the short (3-char) map folder id embedded in the
    /// material's own texture path (e.g. "m21"), matching what <c>TextureLocator.GetMapTextureVirtualPaths</c>
    /// derives internally via <c>mapId.Substring(0, 3)</c> for every project type except DS2.
    /// </summary>
    private static byte[] ReadMapTextureBytes(ProjectEntry project, string mid, string textureName, out string error)
    {
        error = null;

        // GetMapTextureVirtualPaths only ever consumes the first 3 characters of the map id it's given
        // (DS2/DS2S being the one exception, which isn't reliably reconstructable from a truncated id anyway).
        var syntheticMapId = $"{mid}_00_00_00";
        var candidates = TextureLocator.GetMapTextureVirtualPaths(project, syntheticMapId);

        foreach (var candidate in candidates)
        {
            var virtPath = candidate.AssetArchiveVirtualPath ?? candidate.AssetVirtualPath;
            if (string.IsNullOrEmpty(virtPath))
                continue;

            var relPath = PathBuilder.GetRelativePath(project, virtPath);
            if (string.IsNullOrEmpty(relPath))
                continue;

            var archiveTextures = GetOrLoadPackedTextureArchive(project, relPath);
            if (archiveTextures.TryGetValue(textureName, out var bytes))
                return bytes;
        }

        error = $"Texture '{textureName}' was not found in any of map '{mid}''s packed texture archives.";
        return null;
    }

    private static Dictionary<string, byte[]> GetOrLoadPackedTextureArchive(ProjectEntry project, string relPath)
    {
        if (_packedTextureArchiveCache.TryGetValue(relPath, out var cached))
            return cached;

        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        // Cache immediately (even if loading below fails or finds nothing), so a missing/unreadable
        // archive isn't re-opened for every single texture that could have come from it.
        _packedTextureArchiveCache[relPath] = result;

        try
        {
            var containerType = GetTextureContainerType(relPath);

            if (containerType is ResourceContainerType.None)
            {
                var data = project.VFS.FS.ReadFile(relPath);
                if (data != null)
                    IndexTpfBytes(result, data.Value.ToArray());
            }
            else if (containerType is ResourceContainerType.BXF)
            {
                var bdtPath = relPath.Replace("bhd", "bdt");
                var bhd = project.VFS.FS.ReadFile(relPath);
                var bdt = project.VFS.FS.ReadFile(bdtPath);

                if (bhd != null && bdt != null)
                {
                    if (IsBnd3Era(project))
                    {
                        var binder = new BXF3Reader(bhd.Value, bdt.Value);
                        IndexBinderTextures(result, binder.Files, binder.ReadFile);
                    }
                    else
                    {
                        var binder = new BXF4Reader(bhd.Value, bdt.Value);
                        IndexBinderTextures(result, binder.Files, binder.ReadFile);
                    }
                }
            }
            else if (containerType is ResourceContainerType.BND)
            {
                var data = project.VFS.FS.ReadFile(relPath);
                if (data != null)
                {
                    if (IsBnd3Era(project))
                    {
                        var binder = new BND3Reader(data.Value);
                        IndexBinderTextures(result, binder.Files, binder.ReadFile);
                    }
                    else
                    {
                        var binder = new BND4Reader(data.Value);
                        IndexBinderTextures(result, binder.Files, binder.ReadFile);
                    }
                }
            }
        }
        catch
        {
            // Whatever was indexed before the failure is kept; the (possibly empty) result stays cached
            // so this archive isn't retried on every subsequent texture lookup.
        }

        return result;
    }

    private static void IndexBinderTextures(Dictionary<string, byte[]> dest, List<BinderFileHeader> files,
        Func<BinderFileHeader, Memory<byte>> readFile)
    {
        foreach (var entry in files)
        {
            if (!entry.Name.ToLowerInvariant().Contains(".tpf"))
                continue;

            try
            {
                var bytes = readFile(entry).ToArray();
                if (bytes.Length > 0)
                    IndexTpfBytes(dest, bytes);
            }
            catch
            {
                // Skip unreadable/irrelevant entries.
            }
        }
    }

    private static void IndexTpfBytes(Dictionary<string, byte[]> dest, byte[] tpfBytes)
    {
        var tpf = TPF.Read(tpfBytes);
        foreach (var tex in tpf.Textures)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(tex.Name);
            if (!dest.ContainsKey(name))
                dest[name] = GetExportableTextureBytes(tex);
        }
    }

    /// <summary>
    /// Returns bytes that can be handed directly to a standard DDS/PNG decoder. PC-platform TPF textures
    /// already store a normal DDS blob in <see cref="TPF.Texture.Bytes"/>; PS4-platform textures (Bloodborne)
    /// are stored "swizzled" and need to be reversed into a normal DDS layout first.
    /// </summary>
    private static byte[] GetExportableTextureBytes(TPF.Texture tex)
    {
        if (tex.Platform == TPF.TPFPlatform.PS4 && tex.Header != null)
        {
            var decoded = Ps4TextureDecoder.TryDecodeToDds(tex, out _);
            if (decoded != null)
                return decoded;
        }

        return tex.Bytes;
    }

    /// <summary>
    /// Texture containers have a wider set of extensions than model containers (texbnd, ffxbnd, commonbnd, ...),
    /// so this is intentionally separate from <see cref="ModelEditorUtils.GetContainerTypeFromRelativePath"/>.
    /// </summary>
    private static ResourceContainerType GetTextureContainerType(string relativePath)
    {
        var p = relativePath.Replace(".dcx", "").ToLowerInvariant();

        if (p.EndsWith("tpfbhd"))
            return ResourceContainerType.BXF;

        if (p.EndsWith("tpf"))
            return ResourceContainerType.None;

        if (p.EndsWith("texbnd") || p.EndsWith("chrbnd") || p.EndsWith("objbnd") ||
            p.EndsWith("partsbnd") || p.EndsWith("ffxbnd") || p.EndsWith("commonbnd") || p.EndsWith("bnd"))
            return ResourceContainerType.BND;

        return ResourceContainerType.None;
    }

    private static byte[] ExtractTextureFromTpfBytes(byte[] tpfBytes, string textureName)
    {
        if (tpfBytes == null || tpfBytes.Length == 0)
            return null;

        var tpf = TPF.Read(tpfBytes);
        var tex = tpf.Textures.FirstOrDefault(t =>
            string.Equals(System.IO.Path.GetFileNameWithoutExtension(t.Name), textureName, StringComparison.OrdinalIgnoreCase));

        return tex == null ? null : GetExportableTextureBytes(tex);
    }

    private static byte[] FindTextureInBinder(Memory<byte> data, ProjectEntry project, string textureName)
    {
        if (IsBnd3Era(project))
        {
            var binder = new BND3Reader(data);
            return SearchBinderForTexture(binder.Files, binder.ReadFile, textureName);
        }
        else
        {
            var binder = new BND4Reader(data);
            return SearchBinderForTexture(binder.Files, binder.ReadFile, textureName);
        }
    }

    private static byte[] FindTextureInSplitBinder(Memory<byte> bhd, Memory<byte> bdt, ProjectEntry project, string textureName)
    {
        if (IsBnd3Era(project))
        {
            var binder = new BXF3Reader(bhd, bdt);
            return SearchBinderForTexture(binder.Files, binder.ReadFile, textureName);
        }
        else
        {
            var binder = new BXF4Reader(bhd, bdt);
            return SearchBinderForTexture(binder.Files, binder.ReadFile, textureName);
        }
    }

    private static byte[] SearchBinderForTexture(System.Collections.Generic.List<BinderFileHeader> files,
        Func<BinderFileHeader, Memory<byte>> readFile, string textureName)
    {
        var tpfEntries = files.Where(f => f.Name.ToLowerInvariant().Contains(".tpf")).ToList();

        // Fast path: entry filename matches the texture name directly.
        foreach (var entry in tpfEntries)
        {
            if (string.Equals(FilePathUtils.GetPureFilename(entry.Name), textureName, StringComparison.OrdinalIgnoreCase))
            {
                var bytes = readFile(entry).ToArray();
                var found = ExtractTextureFromTpfBytes(bytes, textureName);
                if (found != null)
                    return found;
            }
        }

        // Slow path: parse every packed TPF and look inside for the texture by name
        // (handles archives where the internal texture name differs from the entry filename).
        foreach (var entry in tpfEntries)
        {
            var bytes = readFile(entry).ToArray();
            if (bytes.Length == 0)
                continue;

            try
            {
                var found = ExtractTextureFromTpfBytes(bytes, textureName);
                if (found != null)
                    return found;
            }
            catch
            {
                // Ignore unreadable/irrelevant entries.
            }
        }

        return null;
    }

    private static byte[] FindInBinder(Memory<byte> data, ProjectEntry project, Func<string, bool> nameMatch)
    {
        if (IsBnd3Era(project))
        {
            var binder = new BND3Reader(data);
            foreach (var file in binder.Files)
            {
                if (nameMatch(file.Name))
                    return binder.ReadFile(file).ToArray();
            }
        }
        else
        {
            var binder = new BND4Reader(data);
            foreach (var file in binder.Files)
            {
                if (nameMatch(file.Name))
                    return binder.ReadFile(file).ToArray();
            }
        }

        return null;
    }

    private static byte[] FindInSplitBinder(Memory<byte> bhd, Memory<byte> bdt, ProjectEntry project, Func<string, bool> nameMatch)
    {
        if (IsBnd3Era(project))
        {
            var binder = new BXF3Reader(bhd, bdt);
            foreach (var file in binder.Files)
            {
                if (nameMatch(file.Name))
                    return binder.ReadFile(file).ToArray();
            }
        }
        else
        {
            var binder = new BXF4Reader(bhd, bdt);
            foreach (var file in binder.Files)
            {
                if (nameMatch(file.Name))
                    return binder.ReadFile(file).ToArray();
            }
        }

        return null;
    }
}
