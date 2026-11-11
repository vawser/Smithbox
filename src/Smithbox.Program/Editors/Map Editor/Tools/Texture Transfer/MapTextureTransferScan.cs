using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MaterialEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace StudioCore.Editors.MapEditor;

/// <summary>A model the scanned parts use, and the file it is read from</summary>
public record TextureTransferModel(string ModelName, bool IsObject, string ContainerPath, string InnerName, List<string> PartNames);

/// <summary>Everything a scan needs, read on the main thread</summary>
public record TextureTransferScanInput(
    ProjectType Type,
    VirtualFileSystem FS,
    string TargetKey,
    List<string> BinderPaths,
    List<TextureTransferModel> Models);

/// <summary>A texture the models use that the target binders don't hold, but another map texture binder does</summary>
public class TextureTransferMissing
{
    public string Name;

    /// <summary>The binders holding it, the preferred one (the area the texture path names) first</summary>
    public List<string> Sources = new();

    /// <summary>The texture paths the models give for it</summary>
    public HashSet<string> TexturePaths = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The models (and how many parts place them) that use it</summary>
    public SortedSet<string> UsedBy = new(StringComparer.OrdinalIgnoreCase);

    public int SourceIndex;
    public bool Checked = true;

    public string Source => Sources.Count > 0 ? Sources[Math.Clamp(SourceIndex, 0, Sources.Count - 1)] : null;
}

/// <summary>A texture the models use that no map texture binder holds (expected for _s maps, lightmaps, SysTex, dummies)</summary>
public class TextureTransferNotFound
{
    public string Name;
    public string TexturePath;
    public SortedSet<string> UsedBy = new(StringComparer.OrdinalIgnoreCase);
}

public class TextureTransferResult
{
    public string TargetKey;
    public List<string> TargetBinders = new();
    public int ModelCount;
    public int UsedCount;
    public int PresentCount;
    public int OwnTpfCount;
    public List<TextureTransferMissing> Missing = new();
    public List<TextureTransferNotFound> NotFound = new();
    public List<string> Problems = new();

    /// <summary>Texture name to the binders holding it, over every map texture binder</summary>
    public Dictionary<string, List<string>> Index = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// The background part of the Texture Transfer tool: reads the map texture binders' names (headers only) and the
/// models' FLVER texture paths, and sorts every used texture into present / missing / not found.
/// </summary>
public static class MapTextureTransferScan
{
    public static TextureTransferResult Run(TextureTransferScanInput input, CancellationToken token, Action<int, int, string> progress)
    {
        var result = new TextureTransferResult { TargetKey = input.TargetKey };
        var total = input.BinderPaths.Count + input.Models.Count;
        var done = 0;

        // The map texture index: only the .tpfbhd headers are read, their file names are the texture names
        foreach (var path in input.BinderPaths)
        {
            token.ThrowIfCancellationRequested();
            progress(done++, total, path);

            var key = MapTextureTransferProfile.KeyOfBinder(input.Type, path);

            if (key == input.TargetKey)
                result.TargetBinders.Add(path);

            try
            {
                var bhd = input.FS.FileExists(path) ? input.FS.ReadFile(path) : null;
                var names = bhd != null ? TpfTextureNames.PackedFileNames(bhd.Value) : null;

                if (names == null)
                {
                    result.Problems.Add(LOC.Get("MAP_TextureTransfer_Problem_Binder", path));
                    continue;
                }

                foreach (var name in names)
                {
                    var stem = MapTextureTransferProfile.Stem(name);
                    if (!result.Index.TryGetValue(stem, out var list))
                        result.Index[stem] = list = new();
                    list.Add(path);
                }
            }
            catch (Exception e)
            {
                result.Problems.Add(LOC.Get("MAP_TextureTransfer_Problem_Binder_Error", path, e.Message));
            }
        }

        var targets = new HashSet<string>(result.TargetBinders, StringComparer.OrdinalIgnoreCase);
        var missing = new Dictionary<string, TextureTransferMissing>(StringComparer.OrdinalIgnoreCase);
        var notFound = new Dictionary<string, TextureTransferNotFound>(StringComparer.OrdinalIgnoreCase);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ownTpf = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var model in input.Models)
        {
            token.ThrowIfCancellationRequested();
            progress(done++, total, model.ModelName);

            List<string> texturePaths;
            HashSet<string> ownNames;

            try
            {
                if (!ReadModel(input.FS, model, out texturePaths, out ownNames))
                {
                    result.Problems.Add(LOC.Get("MAP_TextureTransfer_Problem_Model", model.ModelName, model.ContainerPath ?? "-"));
                    continue;
                }
            }
            catch (Exception e)
            {
                result.Problems.Add(LOC.Get("MAP_TextureTransfer_Problem_Model_Error", model.ModelName, e.Message));
                continue;
            }

            result.ModelCount++;
            var usedBy = model.PartNames.Count > 1 ? $"{model.ModelName} ({model.PartNames.Count})" : model.ModelName;

            foreach (var texturePath in texturePaths)
            {
                var stem = MapTextureTransferProfile.Stem(texturePath);
                if (stem.Length == 0)
                    continue;

                used.Add(stem);

                // An object's own TPF wins over the map's
                if (ownNames.Contains(stem))
                {
                    ownTpf.Add(stem);
                    continue;
                }

                if (!result.Index.TryGetValue(stem, out var holders))
                {
                    if (!notFound.TryGetValue(stem, out var nf))
                        notFound[stem] = nf = new TextureTransferNotFound { Name = stem, TexturePath = texturePath };
                    nf.UsedBy.Add(usedBy);
                    continue;
                }

                if (holders.Any(targets.Contains))
                {
                    present.Add(stem);
                    continue;
                }

                if (!missing.TryGetValue(stem, out var entry))
                {
                    missing[stem] = entry = new TextureTransferMissing { Name = stem };

                    // The area the path names first, then by path
                    var pathKey = MapTextureTransferProfile.KeyOfTexturePath(input.Type, texturePath);
                    entry.Sources = holders
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(h => MapTextureTransferProfile.KeyOfBinder(input.Type, h) == pathKey ? 0 : 1)
                        .ThenBy(h => h, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                entry.TexturePaths.Add(texturePath);
                entry.UsedBy.Add(usedBy);
            }
        }

        result.UsedCount = used.Count;
        result.PresentCount = present.Count;
        result.OwnTpfCount = ownTpf.Count;
        result.Missing = missing.Values.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList();
        result.NotFound = notFound.Values.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList();

        progress(total, total, "");
        return result;
    }

    /// <summary>
    /// The texture paths of a model's FLVERs, and the texture names of the TPFs beside it (an objbnd's own textures).
    /// The file is a loose FLVER, a binder holding it (mapbnd, objbnd, DS2's model/obj bnd), or DS2's mapbhd/mapbdt.
    /// </summary>
    private static bool ReadModel(VirtualFileSystem fs, TextureTransferModel model, out List<string> texturePaths, out HashSet<string> ownNames)
    {
        texturePaths = new();
        ownNames = new(StringComparer.OrdinalIgnoreCase);

        var path = model.ContainerPath;
        if (string.IsNullOrEmpty(path) || !fs.FileExists(path))
            return false;

        var data = fs.ReadFile(path);
        if (data == null)
            return false;

        var flvers = new List<IFlver>();

        if (path.EndsWith("bhd", StringComparison.OrdinalIgnoreCase))
        {
            // DS2's mapbhd: every map piece of the map, so only the one named
            var bdtPath = path[..^3] + "bdt";
            var bdt = fs.FileExists(bdtPath) ? fs.ReadFile(bdtPath) : null;
            if (bdt == null)
                return false;

            var bhd = data.Value;
            IBinder binder = bhd.Length >= 4 && bhd.Span[3] == '3' ? BXF3.Read(bhd, bdt.Value) : BXF4.Read(bhd, bdt.Value);
            var inner = MapTextureTransferProfile.Stem(model.InnerName ?? model.ModelName);

            foreach (var file in binder.Files.Where(f => MapTextureTransferProfile.Stem(f.Name) == inner))
                AddFile(file.Name, file.Bytes, flvers, ownNames, 1);
        }
        else
        {
            AddFile(path, data.Value, flvers, ownNames, 0);
        }

        if (flvers.Count == 0)
            return false;

        foreach (var flver in flvers)
        {
            foreach (var material in flver.Materials)
            {
                foreach (var texture in material.Textures)
                {
                    if (!string.IsNullOrWhiteSpace(texture.Path))
                        texturePaths.Add(texture.Path);
                }
            }
        }

        texturePaths = texturePaths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return true;
    }

    private static void AddFile(string name, Memory<byte> data, List<IFlver> flvers, HashSet<string> ownNames, int depth)
    {
        if (DCX.Is(data))
            data = DCX.Decompress(data);

        var span = data.Span;
        var magic = span.Length >= 4 ? Encoding.ASCII.GetString(span[..4]) : "";

        if (magic == "FLVE")
        {
            // FLVER0 (DES) and FLVER2 share the magic; the version tells them apart
            if (FLVER2.Is(data.ToArray()))
                flvers.Add(FLVER2.Read(data));
            else if (FLVER0.Is(data.ToArray()))
                flvers.Add(FLVER0.Read(data));
        }
        else if (TpfTextureNames.IsTpf(span))
        {
            foreach (var texture in TpfTextureNames.Read(data) ?? new List<string>())
                ownNames.Add(MapTextureTransferProfile.Stem(texture));
        }
        else if (depth < 2 && (magic == "BND3" || magic == "BND4"))
        {
            IBinder binder = magic == "BND3" ? BND3.Read(data) : BND4.Read(data);

            foreach (var file in binder.Files)
                AddFile(file.Name, file.Bytes, flvers, ownNames, depth + 1);
        }
    }
}
