using Andre.IO.VFS;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public ConcurrentDictionary<FileDictionaryEntry, MTDWrapper> MTDs = new();
    public ConcurrentDictionary<FileDictionaryEntry, MATBINWrapper> MATBINs = new();

    private ConcurrentDictionary<string, MTD> MTDLookup = new();
    private ConcurrentDictionary<string, MATBIN> MATBINLookup = new();

    private ConcurrentDictionary<string, MaterialReference> MTDReferences = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, MaterialReference> MATBINReferences = new(StringComparer.OrdinalIgnoreCase);

    private ConcurrentDictionary<string, MaterialDefinition> Definitions = new();

    public MaterialShaderTable ShaderTable { get; set; } = new();

    /// <summary>
    /// What the materials of each shader have, built in the background on first use
    /// </summary>
    public MaterialTemplates Templates { get; private set; }

    // Shader info is the same for every material of a shader and permutation
    private readonly ConcurrentDictionary<(string, int), MaterialShaderInfo> ShaderInfos = new();

    /// <summary>
    /// What the shader table says about a material's shader
    /// </summary>
    public MaterialShaderInfo GetShaderInfo(string shaderPath, int staticSwitch) =>
        ShaderInfos.GetOrAdd(((shaderPath ?? "").ToLowerInvariant(), staticSwitch),
            _ => MaterialShaderInfo.Build(ShaderTable, shaderPath, staticSwitch));

    public MaterialBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
        Templates = new MaterialTemplates(this);
    }

    public async Task<bool> Setup()
    {
        // Binders load in parallel; the name lookups are filled afterwards, in load order, so a name held by
        // more than one binder always resolves the same way (see LoadOrder)
        var mtdTasks = Project.Locator.MTD_Files.Entries.Select(async entry =>
        {
            var wrapper = new MTDWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MTDs[entry] = wrapper;
            }
        });

        var matbinTasks = Project.Locator.MATBIN_Files.Entries.Select(async entry =>
        {
            var wrapper = new MATBINWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MATBINs[entry] = wrapper;
            }
        });

        await Task.WhenAll(mtdTasks.Concat(matbinTasks));

        RebuildLookups();

        return true;
    }

    /// <summary>
    /// Refills the name lookups from the binders, in load order, and forgets resolved definitions.
    /// Called after loading and whenever a material file is added, removed or renamed.
    /// </summary>
    public void RebuildLookups()
    {
        lock (LookupLock)
        {
            MTDLookup.Clear();
            MTDReferences.Clear();
            MATBINLookup.Clear();
            MATBINReferences.Clear();
            Definitions.Clear();

            foreach (var (entry, wrapper) in LoadOrder(MTDs))
            {
                foreach (var key in wrapper.FileOrder)
                {
                    if (!wrapper.Entries.TryGetValue(key, out var file))
                        continue;

                    var shortName = ShortName(key);
                    MTDLookup[shortName] = file.MTD;
                    MTDReferences[shortName] = new MaterialReference(MaterialSourceType.MTD, entry, key, file);
                }
            }

            foreach (var (entry, wrapper) in LoadOrder(MATBINs))
            {
                foreach (var key in wrapper.FileOrder)
                {
                    if (!wrapper.Entries.TryGetValue(key, out var file))
                        continue;

                    var shortName = ShortName(key);
                    MATBINLookup[shortName] = file.MATBIN;
                    MATBINReferences[shortName] = new MaterialReference(MaterialSourceType.MATBIN, entry, key, file);
                }
            }
        }
    }

    private readonly object LookupLock = new();

    /// <summary>
    /// Whether a material of this kind already has this short name, in any binder. FLVERs name their material
    /// by short name, so two files sharing one can't both be reached.
    /// </summary>
    public bool IsShortNameTaken(MaterialSourceType type, string shortName)
    {
        IEnumerable<IMaterialBinder> binders = type is MaterialSourceType.MTD ? MTDs.Values : MATBINs.Values;

        return binders.Any(b => b.FileOrder.Any(k => string.Equals(ShortName(k), shortName, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// The file a short name resolves to among materials of one kind
    /// </summary>
    public MaterialReference FindReference(MaterialSourceType type, string shortName)
    {
        var references = type is MaterialSourceType.MTD ? MTDReferences : MATBINReferences;

        return references.TryGetValue(shortName, out var reference) ? reference : null;
    }

    /// <summary>
    /// How many binders of this kind hold a material with this short name
    /// </summary>
    public int CountBindersWithShortName(MaterialSourceType type, string shortName)
    {
        IEnumerable<IMaterialBinder> binders = type is MaterialSourceType.MTD ? MTDs.Values : MATBINs.Values;

        return binders.Count(b => b.FileOrder.Any(k => string.Equals(ShortName(k), shortName, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// The binder entry and wrapper holding a binder, for jumping to a file in it
    /// </summary>
    public FileDictionaryEntry FindBinderEntry(IMaterialBinder binder)
    {
        foreach (var kv in MTDs)
            if (kv.Value == binder) return kv.Key;

        foreach (var kv in MATBINs)
            if (kv.Value == binder) return kv.Key;

        return null;
    }

    /// <summary>
    /// A file name in the same folder as <paramref name="key"/> with another short name, keeping the binder's
    /// naming style (full source paths in ER/NR/AC6, bare names elsewhere) and the extension
    /// </summary>
    public static string WithShortName(string key, string shortName)
    {
        var cut = Math.Max(key.LastIndexOf('\\'), key.LastIndexOf('/'));
        var folder = cut >= 0 ? key[..(cut + 1)] : "";

        return $"{folder}{shortName}{Path.GetExtension(key)}";
    }

    public static string ShortNameOf(string key) => ShortName(key);

    /// <summary>
    /// The binders in the order their names override each other, later winning: by path, so a base binder
    /// comes before its DLC ones (allmaterial, allmaterial_dlc01, allmaterial_dlc02), with devpatch binders last.
    /// ER and NR hold a few names in both a base and a DLC binder with different content, and DES has the same
    /// MTDs in mtd.mtdbnd and mtd.mtdbnd.dcx; see Documentation/Claude/Material Editor Plan.md, R2.
    /// Within a binder, the later file of a repeated name wins.
    /// </summary>
    private static IEnumerable<(FileDictionaryEntry entry, TWrapper wrapper)> LoadOrder<TWrapper>(
        ConcurrentDictionary<FileDictionaryEntry, TWrapper> binders)
    {
        return binders.ToArray()
            .OrderBy(kv => kv.Key.Path.Contains("devpatch", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenBy(kv => kv.Key.Path, StringComparer.OrdinalIgnoreCase)
            .Select(kv => (kv.Key, kv.Value));
    }

    private static string ShortName(string fileName) =>
        Path.GetFileNameWithoutExtension(fileName.Replace('\\', Path.DirectorySeparatorChar));

    public MTD GetMaterial(string name) =>
        MTDLookup.TryGetValue(name, out var mtd) ? mtd : null;

    public MATBIN GetMatbin(string name) =>
        MATBINLookup.TryGetValue(name, out var matbin) ? matbin : null;

    /// <summary>
    /// The resolved form of a material, or null if there is no such material.
    /// The MATBIN takes priority if both exist, matching the renderer's texture lookup.
    /// </summary>
    public MaterialDefinition GetDefinition(string name)
    {
        if (Definitions.TryGetValue(name, out var definition))
            return definition;

        definition = ResolveDefinition(name);

        return definition == null ? null : Definitions.GetOrAdd(name, definition);
    }

    /// <summary>
    /// Resolves every material as it is now, whatever has been resolved before.
    /// </summary>
    public List<MaterialDefinition> ResolveAllDefinitions()
    {
        return MATBINLookup.Keys.Union(MTDLookup.Keys).Select(ResolveDefinition).ToList();
    }

    private MaterialDefinition ResolveDefinition(string name)
    {
        if (MATBINLookup.TryGetValue(name, out var matbin))
            return MaterialDefinition.FromMatbin(name, matbin, ShaderTable);

        if (!MTDLookup.TryGetValue(name, out var mtd))
            return null;

        var definition = MaterialDefinition.FromMtd(name, mtd, ShaderTable);

        // Demon's Souls' g_LightingType 0 (sky, clouds, light shafts, additive effects) is the game's unlit shaders:
        // the texture as it is, lit by nothing. Their shader names are shared with lit materials, so the MTD decides.
        if (Project?.Descriptor.ProjectType is ProjectType.DES && definition.Surface == MaterialSurface.Standard &&
            definition.Params.TryGetValue("g_LightingType", out var lightingType) && lightingType.Length > 0 &&
            (int)lightingType[0] == 0)
        {
            definition.Surface = MaterialSurface.Unlit;
        }

        return definition;
    }

    /// <summary>
    /// Finds the MTD or MATBIN for a FLVER material's virtual path.
    /// The MATBIN takes priority if both exist, matching the renderer's texture lookup.
    /// </summary>
    public MaterialReference FindReference(string materialPath)
    {
        var shortName = Path.GetFileNameWithoutExtension(materialPath.Replace('\\', Path.DirectorySeparatorChar));

        if (MATBINReferences.TryGetValue(shortName, out var matbin))
            return matbin;

        return MTDReferences.TryGetValue(shortName, out var mtd) ? mtd : null;
    }

    /// <summary>
    /// The binder paths and file names of the materials with unsaved changes
    /// </summary>
    public List<string> GetDirtyFiles()
    {
        var files = new List<string>();

        IEnumerable<IMaterialBinder> binders = (MTDs?.Values ?? Enumerable.Empty<MTDWrapper>()).Cast<IMaterialBinder>()
            .Concat(MATBINs?.Values ?? Enumerable.Empty<MATBINWrapper>());

        foreach (var wrapper in binders)
        {
            var dirtyFiles = wrapper.GetFiles().Where(e => e.IsDirty).Select(e => $"{wrapper.FilePath} : {e.Key}").ToList();

            files.AddRange(dirtyFiles);

            // Only files added, removed or renamed, with no file dirty to name
            if (dirtyFiles.Count == 0 && wrapper.IsDirty())
                files.Add($"{wrapper.FilePath} : {LOC.Get("MAT_Data_Binder_Structure_Changed")}");
        }

        return files.OrderBy(e => e).ToList();
    }

    /// <summary>
    /// Saves every binder holding a material with unsaved changes, without needing a view. Returns false if any
    /// binder failed to save.
    /// </summary>
    public bool SaveAllDirty()
    {
        var success = true;
        var warnings = 0;

        IEnumerable<IMaterialBinder> binders = (MTDs?.Values ?? Enumerable.Empty<MTDWrapper>()).Cast<IMaterialBinder>()
            .Concat(MATBINs?.Values ?? Enumerable.Empty<MATBINWrapper>());

        foreach (var wrapper in binders.Where(e => e.IsDirty()).ToList())
        {
            // A binder with a modified material that can't be written correctly isn't saved at all, so the
            // project's copy stays readable
            var findings = wrapper.GetFiles().Where(f => f.IsDirty)
                .Select(f => (file: f, findings: MaterialChecks.Check(f switch
                {
                    MTDFileWrapper m => (object)m.MTD,
                    MATBINFileWrapper m => m.MATBIN,
                    _ => null
                })))
                .ToList();

            var blocked = findings.Where(x => x.findings.Any(f => f.Severity is MaterialCheckSeverity.Error))
                .Select(x => ShortName(x.file.Key)).ToList();

            warnings += findings.Sum(x => x.findings.Count(f => f.Severity is MaterialCheckSeverity.Warning));

            if (blocked.Count > 0)
            {
                Smithbox.LogError(this, LOC.Get("MAT_Check_Save_Blocked", wrapper.Name, string.Join(", ", blocked)));
                success = false;
                continue;
            }

            if (!wrapper.SaveDirtyFiles())
                success = false;
        }

        if (warnings > 0)
            Smithbox.Log(this, LOC.Get("MAT_Check_Save_Warnings", warnings));

        return success;
    }

    #region Dispose
    public void Dispose()
    {
        MTDs.Clear();
        MATBINs.Clear();
        MTDLookup.Clear();
        MATBINLookup.Clear();
        MTDReferences.Clear();
        MATBINReferences.Clear();
        Definitions.Clear();

        MTDs = null;
        MATBINs = null;
        MTDLookup = null;
        MATBINLookup = null;
        MTDReferences = null;
        MATBINReferences = null;
        Definitions = null;
    }
    #endregion
}

/// <summary>
/// The location of a material file within the bank
/// </summary>
public record MaterialReference(MaterialSourceType SourceType, FileDictionaryEntry BinderEntry, string FileKey, MaterialFileWrapper File);
