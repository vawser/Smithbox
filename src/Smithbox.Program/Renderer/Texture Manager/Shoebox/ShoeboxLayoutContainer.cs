using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class ShoeboxLayoutContainer
{
    public ProjectEntry Project;

    public FileDictionaryEntry FileEntry;

    public Dictionary<string, ShoeboxLayout> Layouts = new Dictionary<string, ShoeboxLayout>();

    public Dictionary<string, List<SubTexture>> Textures = new Dictionary<string, List<SubTexture>>();

    /// <summary>The sblytbnd binder the layouts were read from (null for Smithbox's bundled layouts).</summary>
    public BND4 Binder;

    /// <summary>Smithbox's bundled Icon Layouts (DS3/BB) with the project's own over them, not a game binder.</summary>
    public bool IsBundled { get; private set; }

    /// <summary>A layout file was added or taken out (T5), so the binder is rewritten.</summary>
    public bool StructureModified;

    public bool IsModified => StructureModified || Layouts.Values.Any(l => l.IsModified);

    /// <summary>
    /// A new layout for a sheet with none, formatted like an existing layout of this container: the sibling's
    /// TextureAtlas element (attribute order and spacing, its imagePath folder and extension, its type) with no cells,
    /// and the sibling file's text around it. Not added yet (see AddLayout). Null when there is no layout to copy.
    /// </summary>
    public (string Key, ShoeboxLayout Layout) PrepareNewLayout(string sheetName, int sheetWidth, int sheetHeight)
    {
        var like = Layouts.Values.FirstOrDefault(l => !l.IsReadOnly && l.TextureAtlases.Count > 0);
        if (like == null)
            return (null, null);

        var doc = new LayoutDocument
        {
            HasUtf8Bom = like.Document.HasUtf8Bom,
            Nodes = like.Document.Nodes.Select(n => n.Clone()).ToList(),
        };

        var root = doc.Elements.First(e => e.Name == "TextureAtlas");

        // No cells: keep only the whitespace before the end tag
        var tail = root.Children.LastOrDefault() as LayoutText;
        root.Children.Clear();
        if (tail != null && tail.IsWhitespace)
            root.Children.Add(new LayoutText(tail.Raw));

        // imagePath: the sibling's folder and extension around the new sheet's name
        var oldPath = root.Get("imagePath") ?? "";
        var slash = Math.Max(oldPath.LastIndexOf('\\'), oldPath.LastIndexOf('/'));
        var fileName = oldPath[(slash + 1)..];
        var dot = fileName.IndexOf('.');
        root.Set("imagePath", oldPath[..(slash + 1)] + sheetName + (dot >= 0 ? fileName[dot..] : ".png"));

        if (root.Get("width") != null) root.Set("width", $"{sheetWidth}");
        if (root.Get("height") != null) root.Set("height", $"{sheetHeight}");

        var bytes = doc.ToBytes();

        if (!IsBundled)
        {
            // Binder file: the sibling's folder, flags and compression, the next ID
            var likeFile = like.BinderFile;
            var name = likeFile.Name;
            var nameSlash = Math.Max(name.LastIndexOf('\\'), name.LastIndexOf('/'));
            var newName = $"{name[..(nameSlash + 1)]}{sheetName}.layout";
            var id = Binder.Files.Count > 0 ? Binder.Files.Max(f => f.ID) + 1 : 0;

            var file = new BinderFile(likeFile.Flags, id, newName, bytes) { CompressionType = likeFile.CompressionType };
            var layout = new ShoeboxLayout(newName, bytes, file, null);
            layout.MarkModified();
            return (newName, layout);
        }
        else
        {
            var path = Path.Combine(GetProjectLayoutFolder(Project), $"{sheetName}.layout");
            var layout = new ShoeboxLayout(sheetName, bytes, null, path);
            layout.MarkModified();
            return ($"project:{sheetName}.layout", layout);
        }
    }

    public void AddLayout(string key, ShoeboxLayout layout)
    {
        if (layout.BinderFile != null && Binder != null && !Binder.Files.Contains(layout.BinderFile))
            Binder.Files.Add(layout.BinderFile);

        Layouts[key] = layout;

        foreach (var atlas in layout.TextureAtlases)
            Textures[Path.GetFileNameWithoutExtension(atlas.ImagePath.Replace('\\', '/'))] = atlas.SubTextures;

        StructureModified = true;
    }

    public void RemoveLayout(string key)
    {
        if (!Layouts.TryGetValue(key, out var layout))
            return;

        if (layout.BinderFile != null)
            Binder?.Files.Remove(layout.BinderFile);

        Layouts.Remove(key);

        foreach (var atlas in layout.TextureAtlases)
        {
            var sheet = Path.GetFileNameWithoutExtension(atlas.ImagePath.Replace('\\', '/'));
            if (Textures.TryGetValue(sheet, out var subs) && subs == atlas.SubTextures)
                Textures.Remove(sheet);
        }

        StructureModified = true;
    }

    /// <summary>
    /// The TPF the layouts describe: the one in the binder's own folder with its name
    /// (/menu/hi/01_common.sblytbnd ↔ /menu/hi/01_common.tpf), as IconIndex pairs them. Null for the bundled layouts.
    /// </summary>
    public FileDictionaryEntry FindPairedTpf()
    {
        if (IsBundled)
            return null;

        return Project.Locator.TextureFiles.Entries?.FirstOrDefault(t =>
            t.Extension == "tpf"
            && t.Folder.Equals(FileEntry.Folder, StringComparison.OrdinalIgnoreCase)
            && t.Filename.Equals(FileEntry.Filename, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The layout atlas that describes a sheet (a TPF texture name), or null.</summary>
    public (ShoeboxLayout Layout, TextureAtlas Atlas) FindAtlas(string sheetName)
    {
        foreach (var layout in Layouts.Values)
        {
            foreach (var atlas in layout.TextureAtlases)
            {
                if (Path.GetFileNameWithoutExtension(atlas.ImagePath.Replace('\\', '/')).Equals(sheetName, StringComparison.OrdinalIgnoreCase))
                    return (layout, atlas);
            }
        }

        return (null, null);
    }

    /// <summary>The files a save writes: the binder, or each edited bundled/project layout's project file.</summary>
    public List<string> GetUnsavedFiles()
    {
        if (!IsModified)
            return new();

        if (!IsBundled)
            return [FileEntry.Path];

        return Layouts.Values.Where(l => l.IsModified).Select(l => GetProjectPath(l)).ToList();
    }

    /// <summary>Where an edited bundled layout is saved: the project's Icon Layouts folder, same file name.</summary>
    private string GetProjectPath(ShoeboxLayout layout)
    {
        return Path.Combine(GetProjectLayoutFolder(Project), Path.GetFileName(layout.SourcePath ?? $"{layout.FileName}.layout"));
    }

    /// <summary>
    /// Writes the edited layouts. A game binder is rewritten with its read compression into the project; untouched
    /// layouts keep their bytes, so only the edited files change (T4). Bundled layouts are written to the project's
    /// Icon Layouts folder, which SetupLayoutsDirectly reads over Smithbox's own.
    /// </summary>
    public bool Save()
    {
        if (!IsModified)
            return true;

        try
        {
            if (!IsBundled)
            {
                if (Binder == null)
                    return false;

                foreach (var layout in Layouts.Values.Where(l => l.IsModified && l.BinderFile != null))
                {
                    layout.BinderFile.Bytes = layout.ToBytes();
                }

                Project.VFS.ProjectFS.WriteFile(FileEntry.Path, Binder.Write());
            }
            else
            {
                foreach (var layout in Layouts.Values.Where(l => l.IsModified))
                {
                    var path = GetProjectPath(layout);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, layout.ToBytes());
                }
            }

            foreach (var layout in Layouts.Values.Where(l => l.IsModified))
                layout.MarkSaved();

            StructureModified = false;

            Smithbox.Log(this, LOC.Get("TEXVIEW_Save_Log_Saved_Container", FileEntry.Path == "" ? GetProjectLayoutFolder(Project) : FileEntry.Path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Save_Log_Failed_Container", FileEntry.Path), e);
            return false;
        }
    }

    public ShoeboxLayoutContainer(ProjectEntry project, FileDictionaryEntry fileEntry)
    {
        Project = project;

        FileEntry = fileEntry;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        try
        {
            var shoeboxData = Project.VFS.FS.ReadFile(FileEntry.Path);

            LoadLayouts(FileEntry, (Memory<byte>)shoeboxData);
            BuildTextureDictionary();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, 
                LOC.Get("REND_Shoebox_Layout_Load_Failed", FileEntry.Filename), ex);
        }

        return true;
    }

    public void LoadLayouts(FileDictionaryEntry fileEntry, Memory<byte> data)
    {
        try
        {
            var binder = BND4.Read(data);
            Binder = binder;

            foreach (var file in binder.Files)
            {
                if (file.Name.Contains(".layout"))
                {
                    ShoeboxLayout newLayout = new ShoeboxLayout(file);

                    if (!Layouts.ContainsKey(file.Name))
                    {
                        Layouts.Add(file.Name, newLayout);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this,
                LOC.Get("REND_Shoebox_Layout_Load_Failed", FileEntry.Filename), e);
        }
    }

    /// <summary>
    /// The project-local Icon Layouts folder, beside the project-local Icon Configurations:
    /// &lt;project&gt;/.smithbox/Assets/PARAM/&lt;game&gt;/Icon Layouts
    /// </summary>
    public static string GetProjectLayoutFolder(ProjectEntry project)
    {
        return Path.Combine(project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(project), "Icon Layouts");
    }

    /// <summary>
    /// Smithbox's bundled layouts (DS3, BB), with the project's own layouts over them: a project layout replaces the
    /// bundled one for the same sheet (atlas imagePath), and a layout for a new sheet is added. A new sheet joins an
    /// Icon Configuration group through its atlas' type attribute (e.g. type="Item"), as the groups of these games
    /// only search their listed sheets.
    /// </summary>
    public async Task<bool> SetupLayoutsDirectly()
    {
        await Task.Yield();

        IsBundled = true;

        var srcFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Icon Layouts");
        var projectFolder = GetProjectLayoutFolder(Project);

        // Project-local first, so their sheets can be skipped in the bundled set
        var projectSheets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(projectFolder))
        {
            foreach (var path in Directory.EnumerateFiles(projectFolder, "*.layout"))
            {
                try
                {
                    var newLayout = new ShoeboxLayout(path);
                    Layouts.TryAdd($"project:{Path.GetFileName(path)}", newLayout);

                    foreach (var atlas in newLayout.TextureAtlases)
                    {
                        projectSheets.Add(Path.GetFileNameWithoutExtension(atlas.ImagePath));
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("REND_Shoebox_Layout_Load_Failed", path), e);
                }
            }
        }

        foreach (var path in Directory.EnumerateFiles(srcFolder))
        {
            var filename = Path.GetFileName(path);

            if (filename.Contains(".layout"))
            {
                ShoeboxLayout newLayout = new ShoeboxLayout(path);

                // Replaced by the project's layout for the same sheet
                if (newLayout.TextureAtlases.Any(a => projectSheets.Contains(Path.GetFileNameWithoutExtension(a.ImagePath))))
                    continue;

                if (!Layouts.ContainsKey(filename))
                {
                    Layouts.Add(filename, newLayout);
                }
            }
        }

        BuildTextureDictionary();

        return true;
    }

    public void BuildTextureDictionary()
    {
        foreach (var entry in Layouts)
        {
            foreach (var tex in entry.Value.TextureAtlases)
            {
                var path = Path.GetFileNameWithoutExtension(tex.ImagePath);
                string Name = path;

                if (!Textures.ContainsKey(Name))
                {
                    Textures.Add(Name, tex.SubTextures);
                }
            }
        }
    }
}
