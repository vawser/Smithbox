using Andre.Formats;
using DotNext.IO.Log;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
/// Handles icon textures, which crop the whole texture to the specific icon space.
/// </summary>
public class IconManager
{
    /// <summary>DS2's per-icon textures (LoadDirectIcon): each entry owns its handle.</summary>
    public Dictionary<string, CachedTexture> Cache = new();

    // ---- Sheet-based icons (LoadContainerIcon, see IconIndex) ----

    /// <summary>Icon lookups by group, prefixes and value. A null value is a cached miss. The handles belong to SheetHandles.</summary>
    private Dictionary<string, CachedTexture> IconCache = new();

    /// <summary>One GPU texture per sheet ("tpf path|sheet"), shared by every icon on it.</summary>
    private Dictionary<string, CachedTexture> SheetHandles = new();

    /// <summary>Menu TPFs read for the icons, by path. Kept across PurgeCache; dropped when the project changes.</summary>
    private Dictionary<string, TPF> Tpfs = new();
    private Dictionary<string, Task<TPF>> TpfLoads = new();

    private IconIndex Index;
    private ProjectEntry TpfProject;

    public IconManager() { }

    public void PurgeCache()
    {
        foreach (var entry in Cache)
        {
            // Null entries are cached misses
            entry.Value?.Handle?.Dispose();
        }

        Cache.Clear();

        foreach (var entry in SheetHandles)
        {
            entry.Value?.Handle?.Dispose();
        }

        SheetHandles.Clear();
        IconCache.Clear();

        // Cheap to rebuild, and picks up layouts the Texture Viewer has loaded again
        Index = null;
    }

    public CachedTexture HandleIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if(curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            return texture = LoadDirectIcon(context, iconConfig, fieldValue, fieldName, columnIndex);
        }
        else
        {
            return texture = LoadContainerIcon(context, iconConfig, fieldValue, fieldName, columnIndex);
        }
    }

    public CachedTexture LoadDirectIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        var key = $"icon_{context.ID}_{columnIndex}_{fieldName}_{fieldValue}";

        if (Cache.ContainsKey(key))
            return Cache[key];

        if (!long.TryParse($"{fieldValue}", out var id))
            return texture;

        // For DS2 the layout's type attribute is the IconConfig ref string; ids repeat across types
        // (ei_0010 Effect, ic_ca_00010 Item Category), so the lookup is by both
        if (!GetDirectLayouts(curProject).TryGetValue((iconConfig.TargetConfiguration, id), out var layout))
        {
            Cache.TryAdd(key, null);
            return texture;
        }

        var atlas = layout.TextureAtlases.FirstOrDefault();
        if (atlas == null)
            return texture;

        // ImagePath is set to the relative path needed for the VFS
        var relativePath = atlas.ImagePath;

        var fs = curProject.VFS.FS;

        if (fs.FileExists(relativePath))
        {
            try
            {
                var tpfData = curProject.VFS.FS.ReadFile(relativePath);

                TPF curTPF = TPF.Read(tpfData.Value);

                // DS2 TPFs only contain 1 texture, so we can skip proper matching
                var curTex = curTPF.Textures.FirstOrDefault();

                var subTexture = atlas.SubTextures.FirstOrDefault();

                // A layout generated from the icon folders (no layout file) shows the whole texture
                if (subTexture != null && curTex != null && (!int.TryParse(subTexture.Width, out var subWidth) || subWidth == 0))
                {
                    var (texWidth, texHeight) = IconIndex.TextureSize(curTex);
                    subTexture.Width = $"{texWidth}";
                    subTexture.Height = $"{texHeight}";
                }

                if (subTexture != null)
                {
                    var newCachedTexture = new CachedTexture(subTexture)
                    {
                        Source = new IconSource
                        {
                            Binder = "DS2 icon layouts",
                            Sheet = curTex?.Name ?? "",
                            Layout = DirectOrigins.GetValueOrDefault((iconConfig.TargetConfiguration, id), layout.FileName),
                            SubTexture = subTexture,
                            ShownFrom = relativePath,
                        }
                    };
                    newCachedTexture.Load(curTPF, 0);

                    if (!Cache.ContainsKey(key))
                    {
                        Cache.Add(key, newCachedTexture);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("REND_IconManager_TPF_Read_Failed", relativePath), e);

                // Cached as a miss, so the read and the log are not repeated every frame
                Cache.TryAdd(key, null);
            }
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("REND_IconManager_TPF_Find_Failed", relativePath));

            Cache.TryAdd(key, null);
        }

        return texture;
    }

    public CachedTexture LoadContainerIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (!IsValidIconConfiguration(iconConfig))
        {
            return null;
        }

        var group = curProject.Handler.ParamData.IconConfigurations.Groups.FirstOrDefault(
            e => e.Name == iconConfig.TargetConfiguration);

        if (group == null)
            return null;

        if (context[fieldName] == null)
            return null;

        if (!int.TryParse($"{fieldValue}", out var id))
            return null;

        var prefixes = IconIndex.GetPrefixes(group, context);
        var key = $"{group.Name}|{string.Join(",", prefixes)}|{id}";

        if (IconCache.TryGetValue(key, out var cached))
            return cached;

        var index = GetIndex(curProject);
        if (index == null)
            return null;

        // Bundled layouts (DS3, BB): check the group's TPF once for sheets no layout covers, and generate their grids
        if (index.NeedsSheetScan(group, out var scanTpf))
        {
            var tpfToScan = GetTpf(curProject, scanTpf);
            if (tpfToScan == null && TpfLoads.ContainsKey(scanTpf.Path))
                return null; // still loading: try again next frame

            if (index.AddGeneratedSheets(scanTpf, tpfToScan, curProject.Handler.ParamData.IconConfigurations.Groups) > 0)
            {
                // Earlier misses may now be found
                IconCache.Clear();
            }
        }

        var source = index.Find(group, prefixes, id);
        if (source == null)
        {
            IconCache[key] = null;
            return null;
        }

        // The sheet's own TPF, then the group's File TPF (some atlases' sheets live in another TPF)
        var candidates = new[] { source.Tpf, index.GroupTpf(group) }.Where(t => t != null).Distinct().ToList();

        foreach (var tpfEntry in candidates)
        {
            var tpf = GetTpf(curProject, tpfEntry);
            if (tpf == null)
            {
                if (TpfLoads.ContainsKey(tpfEntry.Path))
                    return null; // still loading: try again next frame

                continue; // failed to read
            }

            var texIndex = tpf.Textures.FindIndex(t => t.Name.Equals(source.Sheet, StringComparison.OrdinalIgnoreCase));
            if (texIndex < 0)
                continue;

            // A cell outside the real sheet (a bundled grid keeps 12 rows on a shorter vanilla sheet) is a miss,
            // rather than a crop of wrapped or clamped pixels. Checked before the sheet is uploaded.
            var (sheetWidth, sheetHeight) = IconIndex.TextureSize(tpf.Textures[texIndex]);
            if (sheetWidth > 0 && int.TryParse(source.SubTexture.X, out var sx) && int.TryParse(source.SubTexture.Y, out var sy)
                && int.TryParse(source.SubTexture.Width, out var sw) && int.TryParse(source.SubTexture.Height, out var sh)
                && (sx + sw > sheetWidth || sy + sh > sheetHeight))
            {
                IconCache[key] = null;
                return null;
            }

            var sheetKey = $"{tpfEntry.Path}|{source.Sheet}";
            if (!SheetHandles.TryGetValue(sheetKey, out var sheet))
            {
                sheet = new CachedTexture(null);
                sheet.Load(tpf, texIndex);
                SheetHandles[sheetKey] = sheet;
            }

            source.ShownFrom = tpfEntry.Path;
            var icon = new CachedTexture(source.SubTexture) { Handle = sheet.Handle, Source = source };
            IconCache[key] = icon;
            return icon;
        }

        // The sheet is in neither TPF
        IconCache[key] = null;
        return texture;
    }

    private Dictionary<(string Type, long Id), ShoeboxLayout> DirectLayouts;
    private Dictionary<(string Type, long Id), string> DirectOrigins = new();
    private ProjectEntry DirectProject;

    /// <summary>
    /// Set by ReloadIconSheets: the Texture Viewer's bank may hold a TPF read before the file changed on disk,
    /// so its copies are no longer reused for this project.
    /// </summary>
    private bool BankCopiesStale;

    /// <summary>
    /// Reload Icon Sheets (icon preview context menu): forget every menu TPF and layout read for the icons and read
    /// them again: the TPFs, the game's sblytbnd layout binders and the project's Icon Layouts folder. For files
    /// changed outside Smithbox while the project is open. A new file the project's file list does not have yet
    /// still needs a project reload.
    /// </summary>
    public void ReloadIconSheets(ProjectEntry project)
    {
        if (project == null)
            return;

        PurgeCache();
        Tpfs.Clear();
        TpfLoads.Clear();
        DirectLayouts = null;
        DirectOrigins.Clear();
        BankCopiesStale = true;

        var bank = project.Handler?.TextureData?.PrimaryBank;
        if (bank != null)
        {
            // Replaces bank.ShoeboxEntries, so GetIndex builds a new index once the containers are set up again
            _ = bank.SetupShoeboxLayouts();
        }
    }

    /// <summary>
    /// DS2's per-icon layouts by (type, id), id being the first number in the file name (ic_0050370000 → 50370000).
    /// The project's own Icon Layouts folder comes first: a project file replaces the bundled one with the same
    /// type and id, and a new one (for a mod's new icon TPF) is added. Read once per project.
    /// </summary>
    private Dictionary<(string Type, long Id), ShoeboxLayout> GetDirectLayouts(ProjectEntry project)
    {
        if (DirectLayouts != null && DirectProject == project)
            return DirectLayouts;

        DirectProject = project;
        DirectLayouts = new();
        DirectOrigins.Clear();

        var folders = new[]
        {
            ShoeboxLayoutContainer.GetProjectLayoutFolder(project),
            Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(project.Descriptor.ProjectType), "Icon Layouts"),
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
                continue;

            foreach (var file in Directory.EnumerateFiles(folder, "*.layout"))
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"\d+");
                if (!match.Success || !long.TryParse(match.Value, out var id))
                    continue;

                try
                {
                    var layout = new ShoeboxLayout(file);
                    var type = layout.TextureAtlases.FirstOrDefault()?.Type ?? "";
                    if (DirectLayouts.TryAdd((type, id), layout))
                    {
                        var origin = folder == folders[0] ? "project layout" : "bundled layout";
                        DirectOrigins[(type, id)] = $"{origin} {Path.GetFileName(file)}";
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("REND_Shoebox_Layout_Load_Failed", file), e);
                }
            }
        }

        // Icon TPFs no layout covers (a mod's new icon): the type comes from the folder, as the layouts use it
        // (/menu/tex/icon/effect → Effect), the id from the file name. The sub-texture is the whole texture, sized
        // when the TPF is read. Folders the layouts don't use (the localised map names) are left out.
        var folderTypes = DirectLayouts.Values
            .Select(l => l.TextureAtlases.FirstOrDefault())
            .Where(a => a != null && !string.IsNullOrEmpty(a.ImagePath))
            .GroupBy(a => a.ImagePath[..Math.Max(0, a.ImagePath.LastIndexOf('/'))], StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.GroupBy(a => a.Type).OrderByDescending(t => t.Count()).First().Key, StringComparer.OrdinalIgnoreCase);

        var covered = DirectLayouts.Values
            .Select(l => l.TextureAtlases.FirstOrDefault()?.ImagePath)
            .Where(p => p != null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in project.Locator.TextureFiles.Entries ?? Enumerable.Empty<FileDictionaryEntry>())
        {
            if (entry.Extension != "tpf" || covered.Contains(entry.Path) || !folderTypes.TryGetValue(entry.Folder, out var type))
                continue;

            var match = Regex.Match(entry.Filename, @"\d+");
            if (!match.Success || !long.TryParse(match.Value, out var id))
                continue;

            var atlas = new TextureAtlas(entry.Path, type, new List<SubTexture> { new SubTexture($"ICON_{id}.png", 0, 0, 0, 0) });
            if (DirectLayouts.TryAdd((type, id), new ShoeboxLayout(entry.Filename, atlas)))
                DirectOrigins[(type, id)] = "generated from the icon folder";
        }

        return DirectLayouts;
    }

    /// <summary>The project's icon index, rebuilt when the project or the bank's layouts change.</summary>
    private IconIndex GetIndex(ProjectEntry project)
    {
        if (TpfProject != project)
        {
            // New project: nothing read for the old one applies
            PurgeCache();
            Tpfs.Clear();
            TpfLoads.Clear();
            BankCopiesStale = false;
            TpfProject = project;
        }

        var containers = project.Handler?.TextureData?.PrimaryBank?.ShoeboxEntries;

        if (Index != null && Index.Project == project && ReferenceEquals(Index.Source, containers) && Index.SourceCount == containers?.Count)
            return Index;

        IconIndex index;
        try
        {
            index = new IconIndex(project);
        }
        catch (InvalidOperationException)
        {
            // The bank is still adding containers; try again next frame
            return null;
        }

        // The bank sets its layouts up in the background; don't keep an index built before it is done
        if (index.IsEmpty)
            return null;

        Index = index;
        return Index;
    }

    /// <summary>
    /// A menu TPF by its exact path (the Texture Viewer's bank keys TPFs by file name, which ER's hi and low
    /// 01_common share). Read in the background; null until it is ready, or if it can't be read.
    /// </summary>
    private TPF GetTpf(ProjectEntry project, FileDictionaryEntry entry)
    {
        if (Tpfs.TryGetValue(entry.Path, out var tpf))
            return tpf;

        // Reuse the bank's copy when it has this very file loaded
        var bank = project.Handler?.TextureData?.PrimaryBank;
        if (!BankCopiesStale && bank != null && bank.Entries.TryGetValue(entry, out var contents) && contents?.Files != null)
        {
            var loaded = contents.Files.Values.FirstOrDefault();
            if (loaded != null)
            {
                Tpfs[entry.Path] = loaded;
                return loaded;
            }
        }

        if (!TpfLoads.TryGetValue(entry.Path, out var load))
        {
            var fs = project.VFS.FS;
            var path = entry.Path;
            load = Task.Run(() =>
            {
                var data = fs.ReadFile(path);
                return data == null ? null : TPF.Read(data.Value);
            });
            TpfLoads[entry.Path] = load;
            return null;
        }

        if (!load.IsCompleted)
            return null;

        TpfLoads.Remove(entry.Path);

        if (load.IsFaulted || load.Result == null)
        {
            Smithbox.LogError(this, LOC.Get("REND_IconManager_TPF_Read_Failed", entry.Path), load.Exception);
            Tpfs[entry.Path] = null;
            return null;
        }

        Tpfs[entry.Path] = load.Result;
        return load.Result;
    }

    /// <summary>The icon preview's tooltip: where the icon came from, and the cells it hides.</summary>
    public string DescribeIcon(CachedTexture cachedTexture)
    {
        var source = cachedTexture?.Source;
        if (source == null)
            return "";

        var sub = source.SubTexture;
        var text = LOC.Get("PARAM_FieldDecorator_TexRef_Source_TT",
            source.Sheet, source.ShownFrom ?? "", source.Layout ?? source.Binder ?? "",
            sub?.Name ?? "", sub?.X ?? "", sub?.Y ?? "", sub?.Width ?? "", sub?.Height ?? "");

        if (source.Shadowed.Count > 0)
        {
            var others = string.Join("\n", source.Shadowed.Select(s => $"  {s.Sheet} ({s.Layout}) {s.SubTexture?.Name}"));
            text += "\n" + LOC.Get("PARAM_FieldDecorator_TexRef_Shadowed_TT", others);
        }

        return text;
    }

    public Vector2 DisplayIcon(CachedTexture cachedTexture)
    {
        if (cachedTexture.Handle == null)
            return new Vector2();

        // Get scaled image size vector
        var scale = CFG.Current.ParamEditor_Field_List_Icon_Preview_Scale;

        // Get crop bounds
        float Xmin = float.Parse(cachedTexture.SubTexture.X);
        float Xmax = Xmin + float.Parse(cachedTexture.SubTexture.Width);
        float Ymin = float.Parse(cachedTexture.SubTexture.Y);
        float Ymax = Ymin + float.Parse(cachedTexture.SubTexture.Height);

        // Image size should be based on cropped image
        Vector2 size = new Vector2(Xmax - Xmin, Ymax - Ymin) * scale;

        // Get UV coordinates based on full image
        float left = (Xmin) / cachedTexture.Handle.Width;
        float top = (Ymin) / cachedTexture.Handle.Height;
        float right = (Xmax) / cachedTexture.Handle.Width;
        float bottom = (Ymax) / cachedTexture.Handle.Height;

        // Build UV coordinates
        var UV0 = new Vector2(left, top);
        var UV1 = new Vector2(right, bottom);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            var vulkanHandle = (VulkanTextureHandle)cachedTexture.Handle;
            var textureId = new ImTextureID(vulkanHandle.Handle.TexHandle);
            ImGui.Image(textureId, size, UV0, UV1);
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
        {
            var openglHandle = (OpenGLTextureHandle)cachedTexture.Handle;
            var textureId = new ImTextureID(openglHandle.Handle);
            ImGui.Image(textureId, size, UV0, UV1);
        }

        return size;
    }

    public bool IsValidIconConfiguration(IconConfig iconConfig)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Check Icon Config, if not present then don't attempt to load or display anything
        if (iconConfig == null)
            return false;

        if (curProject.Handler.ParamData == null)
            return false;

        if (curProject.Handler.ParamData.IconConfigurations.Groups == null)
            return false;

        return true;
    }
}
