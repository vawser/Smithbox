using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class ShoeboxLayoutContainer
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public string SourcePath;
    public string FallbackPath;

    public string ContainerName = "";

    public Dictionary<string, ShoeboxLayout> Layouts = new Dictionary<string, ShoeboxLayout>();

    public Dictionary<string, List<SubTexture>> Textures = new Dictionary<string, List<SubTexture>>();

    public ShoeboxLayoutContainer(Smithbox baseEditor, ProjectEntry project, string sourcePath, string fallbackPath)
    {
        BaseEditor = baseEditor;
        Project = project;
        SourcePath = sourcePath;
        FallbackPath = fallbackPath;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu\hi\01_common.sblytbnd.dcx";
            if (File.Exists($@"{SourcePath}\{sourcePath}"))
            {
                sourcePath = $@"{SourcePath}\{sourcePath}";
            }
            else
            {
                sourcePath = $@"{FallbackPath}\{sourcePath}";
            }

            if (File.Exists(sourcePath))
            {
                LoadLayouts(sourcePath);
                BuildTextureDictionary();
            }
            else
            {
                var filename = Path.GetFileNameWithoutExtension(sourcePath);
                TaskLogs.AddLog($"Failed to load Shoebox Layout: {filename} at {sourcePath}");
            }
        }

        return true;
    }

    public void LoadLayouts(string filepath)
    {
        try
        {
            var binder = BND4.Read(filepath);

            ContainerName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filepath));

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
            var filename = Path.GetFileNameWithoutExtension(filepath);

            TaskLogs.AddLog($"Failed to load Shoebox Layout Container: {filename} at {filepath}\n{e.Message}");
        }
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
