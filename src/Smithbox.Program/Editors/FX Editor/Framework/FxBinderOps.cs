using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Adding and removing effect files in an sfx binder the way vanilla lays them out (FX Effect Tree Actions Plan §E,
/// FX Effect List Actions Plan §2). Only SoulsFormats is used, so the check tool compiles this file on its own.
/// </summary>
public static class FxBinderOps
{
    /// <summary>The internal name a file would have next to another: same folder, new stem and extension.</summary>
    public static string SiblingName(string internalName, string stem, string extension)
    {
        int slash = internalName.LastIndexOfAny(new[] { '\\', '/' });
        return $"{(slash >= 0 ? internalName[..(slash + 1)] : "")}{stem}{extension}";
    }

    private static bool IsFxr(BinderFile f) => f.Name.EndsWith(".fxr", StringComparison.OrdinalIgnoreCase);
    private static bool IsList(BinderFile f) => f.Name.EndsWith(".ffxreslist", StringComparison.OrdinalIgnoreCase);

    private static bool HasStem(BinderFile f, string stem, string extension) =>
        Path.GetFileName(f.Name.Replace('\\', '/')).Equals(stem + extension, StringComparison.OrdinalIgnoreCase);

    /// <summary>The effect's ffxreslist in the binder, or null.</summary>
    public static BinderFile ResourceList(BND4 bnd, string stem) => bnd.Files.FirstOrDefault(f => IsList(f) && HasStem(f, stem, ".ffxreslist"));

    /// <summary>
    /// Adds an empty FXR file named <paramref name="name"/> and returns it. FXRs take file ids 0, 1, 2... in file
    /// order, so the new one takes the next id after the last FXR and goes after it. Where the binder keeps
    /// ffxreslists (ids 400000+, in sfx\ResourceList), the new effect gets one too: <paramref name="listContent"/>
    /// when given, else a copy of <paramref name="copyListOf"/>'s (an effect stem) when the binder has it, else empty.
    /// </summary>
    public static BinderFile AddEffectFile(BND4 bnd, string name, string stem, string copyListOf, byte[] listContent = null)
    {
        var fxrs = bnd.Files.Where(IsFxr).ToList();
        int id = fxrs.Count > 0 ? fxrs.Max(f => f.ID) + 1 : 0;
        var flags = fxrs.FirstOrDefault()?.Flags ?? Binder.FileFlags.Flag1;
        var file = new BinderFile(flags, id, name, Array.Empty<byte>());
        int after = fxrs.Count > 0 ? bnd.Files.IndexOf(fxrs[^1]) + 1 : bnd.Files.Count;
        bnd.Files.Insert(after, file);

        var lists = bnd.Files.Where(IsList).ToList();
        if (lists.Count > 0)
        {
            var listName = SiblingName(lists[0].Name, stem, ".ffxreslist");
            if (!bnd.Files.Any(f => f.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)))
            {
                var content = listContent;
                if (content == null)
                {
                    var from = copyListOf == null ? null : ResourceList(bnd, copyListOf);
                    content = from != null ? from.Bytes.ToArray() : Array.Empty<byte>();
                }
                var list = new BinderFile(lists[0].Flags, lists.Max(f => f.ID) + 1, listName, content);
                bnd.Files.Insert(bnd.Files.IndexOf(lists[^1]) + 1, list);
            }
        }
        return file;
    }

    /// <summary>
    /// Removes the effect's FXR (by internal name) and its ffxreslist. The FXRs after it take the next lower ids, as
    /// do the lists, so both runs stay contiguous like vanilla's. A run that wasn't contiguous is left as it was.
    /// Returns whether the FXR was there.
    /// </summary>
    public static bool RemoveEffectFile(BND4 bnd, string name, string stem)
    {
        var file = bnd.Files.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (file == null)
            return false;

        RemoveAndCloseGap(bnd, file, IsFxr);
        if (ResourceList(bnd, stem) is { } list)
            RemoveAndCloseGap(bnd, list, IsList);
        return true;
    }

    private static void RemoveAndCloseGap(BND4 bnd, BinderFile file, Func<BinderFile, bool> kind)
    {
        bool contiguous = IsContiguous(bnd.Files.Where(kind).ToList());
        int removedId = file.ID;
        bnd.Files.Remove(file);
        if (!contiguous)
            return;
        foreach (var f in bnd.Files.Where(kind))
            if (f.ID > removedId)
                f.ID--;
    }

    /// <summary>Whether the files' ids run up by one in file order (vanilla's layout).</summary>
    public static bool IsContiguous(List<BinderFile> files)
    {
        for (int i = 1; i < files.Count; i++)
            if (files[i].ID != files[i - 1].ID + 1)
                return false;
        return true;
    }

    /// <summary>For the check tool's survey: whether the binder's FXRs and lists each have vanilla's contiguous ids.</summary>
    public static (bool fxrs, bool lists) Layout(BND4 bnd) =>
        (IsContiguous(bnd.Files.Where(IsFxr).ToList()), IsContiguous(bnd.Files.Where(IsList).ToList()));
}
