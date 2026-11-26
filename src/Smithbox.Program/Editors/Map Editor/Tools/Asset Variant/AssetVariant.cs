using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>A planned asset variant: the source, the new name and the files to copy.</summary>
public sealed class AssetVariantPlan
{
    public string SourceName { get; init; }
    public string TargetName { get; set; }
    public AssetVariantLayout Layout { get; init; }

    /// <summary>The source's files the VFS has, each with the variant's path (filled by <see cref="AssetVariant.Prepare" />).</summary>
    public List<(string Source, string Target)> Files { get; set; } = new();

    /// <summary>Copy the source's param rows (AssetEnvironmentGeometryParam and the rows keyed by the asset).</summary>
    public bool CopyParamRows { get; set; } = true;

    /// <summary>The source's FLVER materials, each shared with the source or forked (V3).</summary>
    public List<AssetVariantMaterial> Materials { get; set; } = new();

    /// <summary>The AET containers the materials' MATBINs name, each shared or forked (V4; only under forked materials).</summary>
    public List<AssetVariantTexture> Textures { get; set; } = new();

    /// <summary>Fixed by <see cref="AssetVariant.Prepare" />: the containers forked (old → new) and the TPFs to write.</summary>
    public Dictionary<string, string> TextureForks { get; set; } = new();
    public List<(string Source, string Target, string OldContainer, string NewContainer)> TextureWrites { get; set; } = new();
}

/// <summary>
/// An AET container (aet000_143: its textures AET000_143_a, _n…, and the _l twin) a material names: shared, or forked to
/// a free container of its own, so the variant's textures can be edited without changing the source's.
/// </summary>
public sealed class AssetVariantTexture
{
    public string Container { get; init; }

    /// <summary>The texture names the container's TPF holds, for the prompt.</summary>
    public List<string> TextureNames { get; init; } = new();

    public bool Fork { get; set; }

    /// <summary>The copy's container, for the current settings (<see cref="AssetVariant.UpdateTextureForkNames" />).</summary>
    public string ForkName { get; set; }
}

/// <summary>One of the source's materials: shared (the variant names the same MATBIN) or forked to a copy of its own.</summary>
public sealed class AssetVariantMaterial
{
    /// <summary>The short name the source's FLVER names (AEG001_379_Ice).</summary>
    public string ShortName { get; init; }

    /// <summary>The MATBIN's shader, for the prompt; null when no MATBIN has the name (it can't be forked).</summary>
    public string Shader { get; init; }

    /// <summary>Other models using the MATBIN, from the Material Editor's Used By scan; -1 when there is no scan.</summary>
    public int OtherUsers { get; init; } = -1;

    public bool CanFork => Shader != null;
    public bool Fork { get; set; }

    /// <summary>The AET containers the MATBIN's samplers name (lower case), in sampler order.</summary>
    public List<string> Containers { get; init; } = new();

    /// <summary>The copy's short name, for the current variant name (<see cref="AssetVariant.UpdateForkNames" />).</summary>
    public string ForkName { get; set; }
}

/// <summary>The outcome of writing a variant's files.</summary>
public sealed class AssetVariantResult
{
    public string Error { get; set; }
    public List<string> Written { get; } = new();
}

/// <summary>
/// Making a variant of an ER, NR or AC6 asset (Claude/Documentation/Model Variants Plan.md §6, V1): a free name,
/// the source's files copied under it (renamed by <see cref="AssetVariantFork" />), registered in the file lists, and
/// the source's param rows copied in the Param Editor as one undoable edit, written when the params are saved.
/// </summary>
public static class AssetVariant
{
    public const string GeometryParam = "AssetEnvironmentGeometryParam";

    /// <summary>
    /// Params whose rows are keyed by the asset besides the geometry param (§6.3): row ID = asset row × 1000 + n, or
    /// = the asset row.
    /// </summary>
    private static readonly (string Param, bool Thousands)[] KeyedParams =
    [
        ("AssetModelSfxParam", true),
        ("SpeedtreeParam", false),
        ("AssetTranscriptionParam", false)
    ];

    public static bool IsSupported(ProjectType type) => type is ProjectType.ER or ProjectType.NR or ProjectType.AC6;

    public static AssetVariantLayout LayoutOf(ProjectEntry project) =>
        project.Profile.AssetLayout is GameAssetLayout.Ac6Environment ? AssetVariantLayout.Ac6Environment : AssetVariantLayout.Aeg;

    /// <summary>The asset name of an MSB model name or file name (AEG027_053 → aeg027_053), or null.</summary>
    public static string NormalizeName(string modelName)
    {
        var name = modelName?.Trim().ToLowerInvariant();
        return AssetVariantFork.IsAssetName(name) ? name : null;
    }

    /// <summary>The MSB model name of an asset (aeg027_053 → AEG027_053).</summary>
    public static string ModelName(string assetName) => assetName.ToUpperInvariant();

    /// <summary>A plan for a variant of the asset, with a suggested name; null and an error when it can't be made.</summary>
    public static AssetVariantPlan CreatePlan(ProjectEntry project, string sourceName, out string error)
    {
        error = null;
        var source = NormalizeName(sourceName);

        if (!IsSupported(project.Descriptor.ProjectType))
        {
            error = LOC.Get("MAP_AssetVariant_Error_Unsupported");
            return null;
        }

        if (source == null)
        {
            error = LOC.Get("MAP_AssetVariant_Error_Not_Asset", sourceName ?? "");
            return null;
        }

        var layout = LayoutOf(project);

        if (!project.VFS.FS.FileExists(AssetVariantFork.PathOf(layout, source, ".geombnd.dcx")))
        {
            error = LOC.Get("MAP_AssetVariant_Error_No_Source", source);
            return null;
        }

        var plan = new AssetVariantPlan { SourceName = source, Layout = layout };
        plan.TargetName = SuggestName(project, plan);
        plan.Materials = ReadMaterials(project, plan);
        UpdateForkNames(project, plan, plan.TargetName);
        plan.Textures = ReadTextures(project, plan);
        UpdateTextureForkNames(project, plan);
        return plan;
    }

    #region Textures (V4)
    /// <summary>The AET containers the forkable materials name, with the texture names each TPF holds.</summary>
    private static List<AssetVariantTexture> ReadTextures(ProjectEntry project, AssetVariantPlan plan)
    {
        var list = new List<AssetVariantTexture>();

        foreach (var container in plan.Materials.Where(m => m.CanFork).SelectMany(m => m.Containers).Distinct())
        {
            var path = AssetVariantFork.AetPath(plan.Layout, container, false);

            // Only containers that exist can be copied
            if (!project.VFS.FS.FileExists(path))
                continue;

            var names = new List<string>();

            try
            {
                var data = project.VFS.FS.ReadFile(path);
                if (data != null)
                    names = SoulsFormats.TPF.Read(data.Value).Textures.Select(t => t.Name).ToList();
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AssetVariant), LOC.Get("MAP_AssetVariant_Textures_Read_Failed", path, e.Message), e);
                continue;
            }

            list.Add(new AssetVariantTexture { Container = container, TextureNames = names });
        }

        return list;
    }

    /// <summary>Whether a container name is used by a file or a listed texture file.</summary>
    private static bool ContainerTaken(ProjectEntry project, AssetVariantLayout layout, string container, HashSet<string> listed)
    {
        return listed.Contains(container)
            || project.VFS.FS.FileExists(AssetVariantFork.AetPath(layout, container, false))
            || project.VFS.FS.FileExists(AssetVariantFork.AetPath(layout, container, true));
    }

    /// <summary>
    /// The copies' containers (decision D2 for textures): the next free in the source container's group above it (else
    /// below), or in the project group when that option is on. Each fork takes its own.
    /// </summary>
    public static void UpdateTextureForkNames(ProjectEntry project, AssetVariantPlan plan)
    {
        var listed = (project.Locator.TextureFiles?.Entries ?? new())
            .Select(e => e.Filename?.ToLowerInvariant())
            .Where(n => n != null && n.Length >= 10 && AssetVariantFork.ContainerOf(n) == n[..10])
            .Select(n => n[..10])
            .ToHashSet();

        var reserved = new HashSet<string>();
        var useProjectGroup = CFG.Current.MapEditor.MapEditor_AssetVariant_Use_Project_Group;

        foreach (var texture in plan.Textures)
        {
            var group = useProjectGroup
                ? Math.Clamp(CFG.Current.MapEditor.MapEditor_AssetVariant_Project_Group, 0, 999)
                : AssetVariantFork.ContainerGroup(texture.Container);
            var start = useProjectGroup ? 0 : AssetVariantFork.ContainerNumber(texture.Container) + 1;

            texture.ForkName = Enumerable.Range(start, 1000 - start).Concat(Enumerable.Range(0, start).Reverse())
                .Select(n => AssetVariantFork.MakeContainer(group, n))
                .FirstOrDefault(c => c != texture.Container && !reserved.Contains(c) && !ContainerTaken(project, plan.Layout, c, listed));

            if (texture.ForkName != null)
                reserved.Add(texture.ForkName);
        }
    }

    /// <summary>The containers forked for the variant (old → new): ticked, named, and named by a forked material.</summary>
    public static Dictionary<string, string> TextureForkMap(AssetVariantPlan plan)
    {
        var used = plan.Materials.Where(m => m.Fork && m.CanFork).SelectMany(m => m.Containers).ToHashSet();

        return plan.Textures.Where(t => t.Fork && t.ForkName != null && used.Contains(t.Container))
            .ToDictionary(t => t.Container, t => t.ForkName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>The TPFs a texture fork writes (source → target): each container's TPF and its _l twin when it has one.</summary>
    public static List<(string Source, string Target, string OldContainer, string NewContainer)> TextureFiles(ProjectEntry project, AssetVariantPlan plan)
    {
        var files = new List<(string, string, string, string)>();

        foreach (var (oldContainer, newContainer) in TextureForkMap(plan))
        {
            foreach (var low in new[] { false, true })
            {
                var source = AssetVariantFork.AetPath(plan.Layout, oldContainer, low);
                if (project.VFS.FS.FileExists(source))
                    files.Add((source, AssetVariantFork.AetPath(plan.Layout, newContainer, low), oldContainer, newContainer));
            }
        }

        return files;
    }
    #endregion

    #region Materials (V3)
    /// <summary>The Material Editor and its bank, when both are loaded: material forks need them.</summary>
    public static bool MaterialsAvailable(ProjectEntry project, out MaterialEditor.MaterialEditorScreen editor, out MaterialEditor.MaterialBank bank)
    {
        editor = project.Handler.MaterialEditor;
        bank = project.Handler.MaterialData?.PrimaryBank;
        return editor?.ViewHandler?.ActiveView != null && bank != null;
    }

    /// <summary>The source's FLVER materials, with their MATBIN's shader and other users (main thread, when the prompt opens).</summary>
    private static List<AssetVariantMaterial> ReadMaterials(ProjectEntry project, AssetVariantPlan plan)
    {
        var list = new List<AssetVariantMaterial>();

        try
        {
            var data = project.VFS.FS.ReadFile(AssetVariantFork.PathOf(plan.Layout, plan.SourceName, ".geombnd.dcx"));
            if (data == null)
                return list;

            MaterialsAvailable(project, out var editor, out var bank);
            var usage = editor?.Usage;
            var hasScan = usage?.HasScan == true;

            foreach (var name in AssetVariantFork.MaterialShortNames(data.Value))
            {
                var reference = bank?.FindReference(MaterialEditor.MaterialSourceType.MATBIN, name);
                var matbin = (reference?.File as MaterialEditor.MATBINFileWrapper)?.MATBIN;
                var shader = matbin != null ? matbin.ShaderPath ?? "" : null;
                var containers = matbin?.Samplers.Select(s => AssetVariantFork.ContainerOf(s.Path)).Where(c => c != null)
                    .Distinct().ToList() ?? new();

                var others = hasScan
                    ? usage.UsesOf(name).Select(u => u.ContainerName)
                        .Where(c => !string.Equals(c, plan.SourceName, StringComparison.OrdinalIgnoreCase))
                        .Distinct(StringComparer.OrdinalIgnoreCase).Count()
                    : -1;

                list.Add(new AssetVariantMaterial { ShortName = name, Shader = shader, OtherUsers = others, Containers = containers });
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(AssetVariant), LOC.Get("MAP_AssetVariant_Materials_Read_Failed", plan.SourceName, e.Message), e);
        }

        return list;
    }

    /// <summary>
    /// The copies' short names for a variant name: a material named after the source takes the variant's name in its
    /// place (AEG001_379_Ice → AEG001_380_Ice); any other gets the variant's name appended (m41_00_092 →
    /// m41_00_092_AEG027_054). A name another MATBIN (or another fork) already has gets _2, _3…
    /// </summary>
    public static void UpdateForkNames(ProjectEntry project, AssetVariantPlan plan, string targetName)
    {
        var target = NormalizeName(targetName);
        MaterialsAvailable(project, out _, out var bank);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var material in plan.Materials)
        {
            if (target == null || !material.CanFork)
            {
                material.ForkName = null;
                continue;
            }

            var name = material.ShortName.Contains(plan.SourceName, StringComparison.OrdinalIgnoreCase)
                ? AssetVariantFork.ReplaceKeepingCase(material.ShortName, plan.SourceName, target)
                : $"{material.ShortName}_{ModelName(target)}";

            var candidate = name;
            for (var n = 2; used.Contains(candidate) || bank?.IsShortNameTaken(MaterialEditor.MaterialSourceType.MATBIN, candidate) == true; n++)
                candidate = $"{name}_{n}";

            used.Add(candidate);
            material.ForkName = candidate;
        }
    }

    /// <summary>The forked materials, old short name → new.</summary>
    public static Dictionary<string, string> ForkMap(AssetVariantPlan plan)
    {
        return plan.Materials.Where(m => m.Fork && m.CanFork && m.ForkName != null)
            .ToDictionary(m => m.ShortName, m => m.ForkName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The forked MATBINs duplicated under their new names, each beside its original in its binder, as one undoable
    /// Material Editor action, written when the materials are saved (main thread). Returns a line for the result.
    /// </summary>
    public static string CopyMaterials(ProjectEntry project, AssetVariantPlan plan, out bool added)
    {
        added = false;
        var forks = ForkMap(plan);

        if (forks.Count == 0)
            return null;

        if (!MaterialsAvailable(project, out var editor, out var bank))
            return LOC.Get("MAP_AssetVariant_Materials_Unavailable");

        var actions = new List<EditorAction>();
        var missing = new List<string>();

        foreach (var (oldName, newName) in forks)
        {
            var reference = bank.FindReference(MaterialEditor.MaterialSourceType.MATBIN, oldName);

            if (reference?.File == null || reference.BinderEntry == null || !bank.MATBINs.TryGetValue(reference.BinderEntry, out var binder))
            {
                missing.Add(oldName);
                continue;
            }

            var duplicate = new MaterialEditor.MatDuplicateFile(editor, bank, binder, reference.File, newName);

            // The copy (deep, made in the constructor) names the forked textures; undo removes it with them
            if (plan.TextureForks.Count > 0 && duplicate.NewFile is MaterialEditor.MATBINFileWrapper { MATBIN: var copy })
            {
                foreach (var sampler in copy.Samplers)
                    sampler.Path = AssetVariantFork.RetargetTexturePath(sampler.Path, plan.TextureForks);
            }

            actions.Add(duplicate);
        }

        if (actions.Count > 0)
        {
            editor.ViewHandler.ActiveView.ActionManager.ExecuteAction(new CompoundAction(actions));
            added = true;
        }

        var line = LOC.Get("MAP_AssetVariant_Materials_Added", string.Join(", ", forks.Where(f => !missing.Contains(f.Key)).Select(f => f.Value)));

        if (missing.Count > 0)
            line += "\n" + LOC.Get("MAP_AssetVariant_Materials_Missing", string.Join(", ", missing));

        return line;
    }
    #endregion

    /// <summary>
    /// The names the project's files or the asset param already use, so a variant can't take them (vanilla has
    /// param rows without files and files without rows).
    /// </summary>
    public static HashSet<string> TakenNames(ProjectEntry project)
    {
        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in project.Locator.AssetFiles.Entries.Concat(project.Locator.HavokAssetFiles.Entries))
        {
            var name = entry.Filename?.Length >= 10 ? entry.Filename[..10] : null;
            if (AssetVariantFork.IsAssetName(name))
                taken.Add(name);
        }

        if (MapPortParams.TryGetParam(project, GeometryParam, out _, out var param))
        {
            foreach (var row in param.Rows)
            {
                if (row.ID >= 0)
                    taken.Add(AssetVariantFork.MakeName(row.ID / 1000, row.ID % 1000));
            }
        }

        return taken;
    }

    /// <summary>
    /// The suggested name (decision D2): the next free in the source's AEG group above the source (else below), or
    /// the next free in the project group when that option is on. Null when the group is full.
    /// </summary>
    public static string SuggestName(ProjectEntry project, AssetVariantPlan plan)
    {
        var taken = TakenNames(project);
        var useProjectGroup = CFG.Current.MapEditor.MapEditor_AssetVariant_Use_Project_Group;
        var group = useProjectGroup
            ? Math.Clamp(CFG.Current.MapEditor.MapEditor_AssetVariant_Project_Group, 0, 999)
            : AssetVariantFork.Group(plan.SourceName);

        var start = useProjectGroup ? 0 : AssetVariantFork.Number(plan.SourceName) + 1;
        var order = Enumerable.Range(start, 1000 - start).Concat(Enumerable.Range(0, start).Reverse());

        foreach (var number in order)
        {
            var name = AssetVariantFork.MakeName(group, number);

            if (!taken.Contains(name) && ValidateName(project, plan, name, taken) == null)
                return name;
        }

        return null;
    }

    /// <summary>Null when the name can be used, else why not.</summary>
    public static string ValidateName(ProjectEntry project, AssetVariantPlan plan, string name, HashSet<string> taken = null)
    {
        var target = NormalizeName(name);

        if (target == null)
            return LOC.Get("MAP_AssetVariant_Error_Name_Format");

        if (target == plan.SourceName)
            return LOC.Get("MAP_AssetVariant_Error_Name_Source");

        taken ??= TakenNames(project);

        if (taken.Contains(target))
            return LOC.Get("MAP_AssetVariant_Error_Name_Taken", target);

        foreach (var suffix in AssetVariantFork.FileSuffixes)
        {
            var path = AssetVariantFork.PathOf(plan.Layout, target, suffix);
            if (project.VFS.FS.FileExists(path))
                return LOC.Get("MAP_AssetVariant_Error_Name_Exists", path);
        }

        return null;
    }

    /// <summary>The files to copy, for the plan's current name (main thread, before the write).</summary>
    public static void Prepare(ProjectEntry project, AssetVariantPlan plan)
    {
        plan.TargetName = NormalizeName(plan.TargetName);
        plan.Files = AssetVariantFork.Files(plan.Layout, plan.SourceName, plan.TargetName, project.VFS.FS.FileExists);
        UpdateForkNames(project, plan, plan.TargetName);

        // A fork is only kept when its MATBIN can be duplicated, so the FLVER never names a copy that won't exist
        var available = MaterialsAvailable(project, out _, out var bank);

        foreach (var material in plan.Materials.Where(m => m.Fork))
        {
            var reference = available ? bank.FindReference(MaterialEditor.MaterialSourceType.MATBIN, material.ShortName) : null;

            if (reference?.File == null || reference.BinderEntry == null || !bank.MATBINs.ContainsKey(reference.BinderEntry))
                material.Fork = false;
        }

        // Texture forks of the materials still forked, with fresh names
        UpdateTextureForkNames(project, plan);
        plan.TextureForks = TextureForkMap(plan);
        plan.TextureWrites = TextureFiles(project, plan);
    }

    /// <summary>Reads, renames and writes each file into the project (can run in the background).</summary>
    public static AssetVariantResult WriteFiles(ProjectEntry project, AssetVariantPlan plan)
    {
        var result = new AssetVariantResult();

        if (project.VFS.ProjectFS == null)
        {
            result.Error = LOC.Get("MAP_AssetVariant_Error_No_Project_FS");
            return result;
        }

        // Forked textures first: if one can't be written, nothing that names it is
        foreach (var (source, target, oldContainer, newContainer) in plan.TextureWrites)
        {
            try
            {
                var data = project.VFS.FS.ReadFile(source);

                if (data == null)
                {
                    result.Error = LOC.Get("MAP_AssetVariant_Error_Read", source);
                    return result;
                }

                project.VFS.ProjectFS.WriteFile(target, AssetVariantFork.RenameTpf(data.Value, oldContainer, newContainer));
                result.Written.Add(target);
            }
            catch (Exception e)
            {
                result.Error = LOC.Get("MAP_AssetVariant_Error_Write", target, e.Message);
                Smithbox.LogError(typeof(AssetVariant), result.Error, e);
                return result;
            }
        }

        foreach (var (source, target) in plan.Files)
        {
            try
            {
                var data = project.VFS.FS.ReadFile(source);

                if (data == null)
                {
                    result.Error = LOC.Get("MAP_AssetVariant_Error_Read", source);
                    return result;
                }

                var bytes = AssetVariantFork.Rename(data.Value, plan.SourceName, plan.TargetName);

                // Forked materials: the variant's FLVERs name the copies (and their slots the forked textures)
                var forks = ForkMap(plan);
                if (forks.Count > 0 && target.EndsWith(".geombnd.dcx", StringComparison.OrdinalIgnoreCase))
                    bytes = AssetVariantFork.RetargetMaterials(bytes, forks, plan.TextureForks);

                project.VFS.ProjectFS.WriteFile(target, bytes);
                result.Written.Add(target);
            }
            catch (Exception e)
            {
                result.Error = LOC.Get("MAP_AssetVariant_Error_Write", target, e.Message);
                Smithbox.LogError(typeof(AssetVariant), result.Error, e);
                return result;
            }
        }

        return result;
    }

    /// <summary>Adds the written files to the file lists, so the Model Selector and the other editors see them (main thread).</summary>
    public static void Register(ProjectEntry project, AssetVariantResult result)
    {
        foreach (var path in result.Written)
        {
            var slash = path.LastIndexOf('/');
            var file = path[(slash + 1)..];
            var dot = file.IndexOf('.');
            var ext = file[(dot + 1)..];
            if (ext.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
                ext = ext[..^4];

            var list = ext switch
            {
                "geombnd" => project.Locator.AssetFiles,
                "geomhkxbnd" => project.Locator.HavokAssetFiles,
                "tpf" => project.Locator.TextureFiles,
                _ => null // AC6's loose geomhkx: in no list
            };

            if (list == null || list.Entries.Any(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase)))
                continue;

            var entry = new FileDictionaryEntry
            {
                Archive = "",
                Path = path,
                Folder = path[..slash],
                Filename = file[..dot],
                Extension = ext,
                Validated = true
            };

            list.Entries.Add(entry);

            // The Model Editor and Texture Editor load through their banks, built when the project loaded
            if (ext == "geombnd")
                project.Handler.ModelData?.PrimaryBank?.AddAsset(entry);

            if (ext == "tpf" && project.Handler.TextureData?.PrimaryBank is { } textures)
            {
                // As the Texture Editor's Copy As: listed and marked as a project file
                textures.Entries[entry] = null;
                textures.ProjectFiles[path] = true;
            }
        }
    }

    /// <summary>
    /// The source's param rows copied under the variant's IDs, added in the Param Editor as one undoable action (main
    /// thread). Returns a line for the result; <paramref name="added" /> is true when rows were added.
    /// </summary>
    public static string CopyParamRows(ProjectEntry project, AssetVariantPlan plan, out bool added)
    {
        added = false;
        var editor = project.Handler.ParamEditor;

        if (editor == null || !MapPortParams.TryGetParam(project, GeometryParam, out _, out _))
            return LOC.Get("MAP_AssetVariant_Params_Unavailable");

        var sourceRow = AssetVariantFork.ParamRowId(plan.SourceName);
        var targetRow = AssetVariantFork.ParamRowId(plan.TargetName);
        var actions = new List<EditorAction>();
        var counts = new List<string>();

        foreach (var (paramName, thousands) in KeyedParams.Prepend((GeometryParam, false)))
        {
            if (!MapPortParams.TryGetParam(project, paramName, out _, out var param))
                continue;

            var rows = param.Rows
                .Where(r => thousands ? r.ID / 1000 == sourceRow && r.ID >= 0 : r.ID == sourceRow)
                .Select(r => new Param.Row(r)
                {
                    ID = thousands ? targetRow * 1000 + r.ID % 1000 : targetRow,
                    Name = LOC.Get("MAP_AssetVariant_Row_Name", string.IsNullOrEmpty(r.Name) ? ModelName(plan.SourceName) : r.Name, ModelName(plan.SourceName))
                })
                .ToList();

            if (rows.Count == 0)
                continue;

            actions.Add(new AddParamsAction(editor, param, paramName, rows, appendOnly: false, replaceParams: true));
            counts.Add($"{paramName} ×{rows.Count}");
        }

        if (actions.Count == 0)
            return LOC.Get("MAP_AssetVariant_Params_None", sourceRow);

        var compound = new CompoundAction(actions);
        editor.ActionManager.ExecuteAction(compound);
        added = true;

        return LOC.Get("MAP_AssetVariant_Params_Added", string.Join(", ", counts));
    }
}
