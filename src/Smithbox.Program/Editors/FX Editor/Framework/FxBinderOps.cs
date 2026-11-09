using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Adding a new effect's files to an sfx binder the way vanilla lays them out (FX Effect Tree Actions Plan §E).
/// Only SoulsFormats is used, so the check tool compiles this file on its own.
/// </summary>
public static class FxBinderOps
{
    /// <summary>The internal name a file would have next to another: same folder, new stem and extension.</summary>
    public static string SiblingName(string internalName, string stem, string extension)
    {
        int slash = internalName.LastIndexOfAny(new[] { '\\', '/' });
        return $"{(slash >= 0 ? internalName[..(slash + 1)] : "")}{stem}{extension}";
    }

    /// <summary>
    /// Adds an empty FXR file named <paramref name="name"/> and returns it. FXRs take file ids 0, 1, 2... in file
    /// order, so the new one takes the next id after the last FXR and goes after it. Where the binder keeps
    /// ffxreslists (ids 400000+, in sfx\ResourceList), the new effect gets one too, a copy of
    /// <paramref name="copyListOf"/>'s (an effect stem) when the binder has it, else empty.
    /// </summary>
    public static BinderFile AddEffectFile(BND4 bnd, string name, string stem, string copyListOf)
    {
        var fxrs = bnd.Files.Where(f => f.Name.EndsWith(".fxr", StringComparison.OrdinalIgnoreCase)).ToList();
        int id = fxrs.Count > 0 ? fxrs.Max(f => f.ID) + 1 : 0;
        var flags = fxrs.FirstOrDefault()?.Flags ?? Binder.FileFlags.Flag1;
        var file = new BinderFile(flags, id, name, Array.Empty<byte>());
        int after = fxrs.Count > 0 ? bnd.Files.IndexOf(fxrs[^1]) + 1 : bnd.Files.Count;
        bnd.Files.Insert(after, file);

        var lists = bnd.Files.Where(f => f.Name.EndsWith(".ffxreslist", StringComparison.OrdinalIgnoreCase)).ToList();
        if (lists.Count > 0)
        {
            var listName = SiblingName(lists[0].Name, stem, ".ffxreslist");
            if (!bnd.Files.Any(f => f.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)))
            {
                var from = copyListOf == null ? null : lists.FirstOrDefault(f =>
                    Path.GetFileName(f.Name.Replace('\\', '/')).Equals($"{copyListOf}.ffxreslist", StringComparison.OrdinalIgnoreCase));
                var content = from != null ? from.Bytes.ToArray() : Array.Empty<byte>();
                var list = new BinderFile(lists[0].Flags, lists.Max(f => f.ID) + 1, listName, content);
                bnd.Files.Insert(bnd.Files.IndexOf(lists[^1]) + 1, list);
            }
        }
        return file;
    }
}
