using System;

namespace StudioCore.Renderer;

/// <summary>
///     Resources are looked up by name. A name is the file name of the resource with any
///     file extensions removed, compared case-insensitively. For example
///     "N:\GR\data\Model\chr\c1234.flver.dcx", "c1234.flver" and "C1234" all refer to "c1234".
///     Every name that is stored in, or looked up from, a <see cref="ResourcePool" /> goes through
///     <see cref="Normalize" />, so callers never have to format names exactly.
/// </summary>
public static class ResourceName
{
    // Extensions that are not part of a resource's identity. Anything else (e.g. a '.' that
    // is part of a texture name) is left alone.
    private static readonly string[] StrippedExtensions =
    {
        ".dcx", ".flver", ".flv", ".hkx", ".nvm", ".tpf", ".dds", ".tga", ".tif", ".tiff", ".png", ".bmp", ".jpg"
    };

    /// <summary>
    ///     Returns the last segment of a path, accepting both '/' and '\' separators
    ///     regardless of platform (paths stored in game files use '\').
    /// </summary>
    public static string FileName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }

        var idx = path.LastIndexOfAny(new[] { '/', '\\' });
        return idx >= 0 ? path.Substring(idx + 1) : path;
    }

    /// <summary>
    ///     Converts a path or name to the key used for lookups.
    /// </summary>
    public static string Normalize(string pathOrName)
    {
        var name = FileName(pathOrName);

        var stripped = true;
        while (stripped)
        {
            stripped = false;
            foreach (var ext in StrippedExtensions)
            {
                if (name.Length > ext.Length && name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - ext.Length);
                    stripped = true;
                    break;
                }
            }
        }

        return name.ToLowerInvariant();
    }
}
