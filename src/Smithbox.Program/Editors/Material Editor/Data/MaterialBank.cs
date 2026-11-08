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

    public MaterialBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        var mtdTasks = Project.Locator.MTD_Files.Entries.Select(async entry =>
        {
            var wrapper = new MTDWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MTDs[entry] = wrapper;
                foreach (var kv in wrapper.Entries)
                {
                    var shortName = Path.GetFileNameWithoutExtension(kv.Key.Replace('\\', Path.DirectorySeparatorChar));
                    MTDLookup[shortName] = kv.Value.MTD;
                    MTDReferences[shortName] = new MaterialReference(MaterialSourceType.MTD, entry, kv.Key, kv.Value);
                }
            }
        });

        var matbinTasks = Project.Locator.MATBIN_Files.Entries.Select(async entry =>
        {
            var wrapper = new MATBINWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MATBINs[entry] = wrapper;
                foreach (var kv in wrapper.Entries)
                {
                    var shortName = Path.GetFileNameWithoutExtension(kv.Key.Replace('\\', Path.DirectorySeparatorChar));
                    MATBINLookup[shortName] = kv.Value.MATBIN;
                    MATBINReferences[shortName] = new MaterialReference(MaterialSourceType.MATBIN, entry, kv.Key, kv.Value);
                }
            }
        });

        await Task.WhenAll(mtdTasks.Concat(matbinTasks));

        return true;
    }

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

    public async Task<bool> Save(MaterialEditorView view)
    {
        await Task.Yield();

        if(view.Selection.SourceType is MaterialSourceType.MTD)
        {
            return await view.Selection.MTDWrapper.Save(view);
        }

        if (view.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            return await view.Selection.MATBINWrapper.Save(view);
        }

        return true;
    }

    /// <summary>
    /// The binder paths and file names of the materials with unsaved changes
    /// </summary>
    public List<string> GetDirtyFiles()
    {
        var files = new List<string>();

        if (MTDs != null)
        {
            foreach (var wrapper in MTDs.Values)
            {
                files.AddRange(wrapper.Entries.Values.Where(e => e.IsDirty).Select(e => $"{wrapper.FilePath} : {e.Key}"));
            }
        }

        if (MATBINs != null)
        {
            foreach (var wrapper in MATBINs.Values)
            {
                files.AddRange(wrapper.Entries.Values.Where(e => e.IsDirty).Select(e => $"{wrapper.FilePath} : {e.Key}"));
            }
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

        if (MTDs != null)
        {
            foreach (var wrapper in MTDs.Values.Where(e => e.IsDirty()))
            {
                if (!wrapper.SaveDirtyFiles())
                    success = false;
            }
        }

        if (MATBINs != null)
        {
            foreach (var wrapper in MATBINs.Values.Where(e => e.IsDirty()))
            {
                if (!wrapper.SaveDirtyFiles())
                    success = false;
            }
        }

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

internal static class MaterialBinderSave
{
    /// <summary>
    /// Writes the dirty files into their binder and saves it to the project, then clears their dirty marks.
    /// Unlike the editor's save, this doesn't need a view, so the unsaved changes prompt can save every binder.
    /// </summary>
    public static bool SaveDirtyFiles(object owner, ProjectEntry project, VirtualFileSystem targetFS, string filePath,
        bool isBnd3, IEnumerable<MaterialFileWrapper> files, Func<MaterialFileWrapper, byte[]> write)
    {
        var dirty = files.Where(e => e.IsDirty).ToDictionary(e => e.Key);

        if (dirty.Count == 0)
            return true;

        var containerFileData = targetFS.ReadFile(filePath);

        if (containerFileData == null)
        {
            Smithbox.LogError(owner, LOC.Get("EDITOR_Data_Failed_Read_Container", filePath));
            return false;
        }

        try
        {
            IBinder binder = isBnd3 ? BND3.Read(containerFileData.Value) : BND4.Read(containerFileData.Value);

            foreach (var binderFile in binder.Files)
            {
                if (dirty.TryGetValue(binderFile.Name, out var wrapper))
                {
                    binderFile.Bytes = write(wrapper);
                }
            }

            var writeFile = binder is BND3 bnd3 ? bnd3.Write() : ((BND4)binder).Write();

            project.VFS.ProjectFS.WriteFile(filePath, writeFile);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(owner, LOC.Get("EDITOR_Data_Failed_Write_Container", filePath), ex);
            return false;
        }

        foreach (var wrapper in dirty.Values)
        {
            wrapper.IsDirty = false;
        }

        Smithbox.Log(owner, LOC.Get("EDITOR_Data_Save_File", filePath));

        return true;
    }
}

/// <summary>
/// The location of a material file within the bank
/// </summary>
public record MaterialReference(MaterialSourceType SourceType, FileDictionaryEntry BinderEntry, string FileKey, MaterialFileWrapper File);

public class MTDWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string FilePath { get; set; }
    public Dictionary<string, MTDFileWrapper> Entries { get; set; } = new();

    public MTDWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        FilePath = dictEntry.Path;
    }

    /// <summary>
    /// Whether any of the files within this binder have been modified
    /// </summary>
    public bool IsDirty()
    {
        return Entries.Values.Any(e => e.IsDirty);
    }

    public async Task<bool> Load()
    {
        await Task.Yield();

        try
        {
            if (TargetFS.FileExists(FilePath))
            {
                var binderData = TargetFS.ReadFile(FilePath);

                if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                {
                    var binder = BND3.Read(binderData.Value);

                    foreach (var entry in binder.Files)
                    {
                        if (entry.Name.Contains(".mtd"))
                        {
                            try
                            {
                                var mtd = MTD.Read(entry.Bytes);

                                Entries[entry.Name] = new MTDFileWrapper(entry.Name, mtd);
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_MTD", entry.Name), e);

                                return false;
                            }
                        }
                    }
                }
                else
                {
                    var binder = BND4.Read(binderData.Value);

                    foreach (var entry in binder.Files)
                    {
                        if (entry.Name.Contains(".mtd"))
                        {
                            try
                            {
                                var mtd = MTD.Read(entry.Bytes);

                                Entries[entry.Name] = new MTDFileWrapper(entry.Name, mtd);
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_MTD", entry.Name), e);

                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Find_File", FilePath));
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_File", FilePath), e);
            return false;
        }
    }

    public async Task<bool> Save(MaterialEditorView view)
    {
        await Task.Yield();

        var filename = Path.GetFileNameWithoutExtension(FilePath);

        if (!CFG.Current.MaterialEditor_Allow_Unmodified_Save)
        {
            if (!view.Selection.SelectedFileWrapper.IsDirty)
            {
                Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", filename));
                return false;
            }
        }

        if (!TargetFS.FileExists(FilePath))
        {
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", FilePath));
            return false;
        }

        if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            return HandleContainer_BND3(view);
        }
        else
        {
            return HandleContainer_BND4(view);
        }
    }

    /// <summary>
    /// Saves every modified MTD in this binder, without needing a view
    /// </summary>
    public bool SaveDirtyFiles()
    {
        var isBnd3 = Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;

        return MaterialBinderSave.SaveDirtyFiles(this, Project, TargetFS, FilePath, isBnd3,
            Entries.Values, e => ((MTDFileWrapper)e).MTD.Write());
    }

    private bool HandleContainer_BND3(MaterialEditorView view)
    {
        // Container
        var containerFileData = TargetFS.ReadFile(FilePath);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        // Binder
        var binder = BND3.Read(containerFileData.Value);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        // Binder Files
        foreach (var curFile in binder.Files)
        {
            HandleBinderFile(view, curFile);
        }

        // Save Binder
        try
        {
            var writeFile = binder.Write();

            Project.VFS.ProjectFS.WriteFile(view.Selection.SelectedBinderEntry.Path, writeFile);

            ClearDirtyState(view.Selection.SelectedFileKey);

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", FilePath));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", FilePath), ex);
            return false;
        }

        return true;
    }

    private bool HandleContainer_BND4(MaterialEditorView view)
    {
        // Container
        var containerFileData = TargetFS.ReadFile(FilePath);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        // Binder
        var binder = BND4.Read(containerFileData.Value);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        // Binder Files
        foreach (var curFile in binder.Files)
        {
            HandleBinderFile(view, curFile);
        }

        // Save Binder
        try
        {
            var writeFile = binder.Write();

            Project.VFS.ProjectFS.WriteFile(view.Selection.SelectedBinderEntry.Path, writeFile);

            ClearDirtyState(view.Selection.SelectedFileKey);

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", FilePath));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", FilePath), ex);
            return false;
        }

        return true;
    }

    private void HandleBinderFile(MaterialEditorView view, BinderFile file)
    {
        if (view.Selection.SelectedFileKey != file.Name)
            return;

        // Internal File
        try
        {
            file.Bytes = view.Selection.SelectedMTD.Write();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", file.Name), ex);
            return;
        }
    }

    private void ClearDirtyState(string key)
    {
        if (Entries.TryGetValue(key, out var fileWrapper))
        {
            fileWrapper.IsDirty = false;
        }
    }
}

public class MATBINWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string FilePath { get; set; }
    public Dictionary<string, MATBINFileWrapper> Entries { get; set; } = new();

    public MATBINWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        FilePath = dictEntry.Path;
    }

    /// <summary>
    /// Whether any of the files within this binder have been modified
    /// </summary>
    public bool IsDirty()
    {
        return Entries.Values.Any(e => e.IsDirty);
    }

    public async Task<bool> Load()
    {
        await Task.Yield();

        if (TargetFS.FileExists(FilePath))
        {
            try
            {
                var binderData = TargetFS.ReadFile(FilePath);
                var binder = BND4.Read(binderData.Value);

                foreach (var entry in binder.Files)
                {
                    if (entry.Name.Contains(".matbin"))
                    {
                        try
                        {
                            var matbin = MATBIN.Read(entry.Bytes);
                            Entries[entry.Name] = new MATBINFileWrapper(entry.Name, matbin);
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_MATBIN", entry.Name), e);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_File", FilePath), e);
                return false;
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Find_File", FilePath));
            return false;
        }
    }

    public async Task<bool> Save(MaterialEditorView view)
    {
        await Task.Yield();

        var filename = Path.GetFileNameWithoutExtension(FilePath);

        if (!CFG.Current.MaterialEditor_Allow_Unmodified_Save)
        {
            if (!view.Selection.SelectedFileWrapper.IsDirty)
            {
                Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", filename));
                return false;
            }
        }

        if (!TargetFS.FileExists(FilePath))
        {
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", FilePath));
            return false;
        }

        return HandleContainer_BND4(view);
    }

    /// <summary>
    /// Saves every modified MATBIN in this binder, without needing a view
    /// </summary>
    public bool SaveDirtyFiles()
    {
        return MaterialBinderSave.SaveDirtyFiles(this, Project, TargetFS, FilePath, false,
            Entries.Values, e => ((MATBINFileWrapper)e).MATBIN.Write());
    }

    private bool HandleContainer_BND4(MaterialEditorView view)
    {
        // Container
        var containerFileData = TargetFS.ReadFile(FilePath);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        // Binder
        var binder = BND4.Read(containerFileData.Value);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        // Binder Files
        foreach (var curFile in binder.Files)
        {
            HandleBinderFile(view, curFile);
        }

        // Save Binder
        try
        {
            var writeFile = binder.Write();

            Project.VFS.ProjectFS.WriteFile(view.Selection.SelectedBinderEntry.Path, writeFile);

            ClearDirtyState(view.Selection.SelectedFileKey);

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", FilePath));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", FilePath), ex);
            return false;
        }

        return true;
    }

    private void HandleBinderFile(MaterialEditorView view, BinderFile file)
    {
        if (view.Selection.SelectedFileKey != file.Name)
            return;

        // Internal File
        try
        {
            file.Bytes = view.Selection.SelectedMATBIN.Write();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", file.Name), ex);
            return;
        }
    }

    private void ClearDirtyState(string key)
    {
        if (Entries.TryGetValue(key, out var fileWrapper))
        {
            fileWrapper.IsDirty = false;
        }
    }
}