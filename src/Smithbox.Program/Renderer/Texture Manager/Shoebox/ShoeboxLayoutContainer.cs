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
