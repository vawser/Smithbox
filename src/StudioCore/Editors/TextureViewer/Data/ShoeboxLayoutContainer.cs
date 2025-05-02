using SoulsFormats;
using StudioCore.Core.Project;
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
    private TextureViewerScreen Screen;

    public string ContainerName = "";

    public Dictionary<string, ShoeboxLayout> Layouts = new Dictionary<string, ShoeboxLayout>();

    public Dictionary<string, List<SubTexture>> Textures = new Dictionary<string, List<SubTexture>>();

    public ShoeboxLayoutContainer(TextureViewerScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu\hi\01_common.sblytbnd.dcx";
            if (File.Exists($@"{Smithbox.ProjectRoot}\{sourcePath}"))
            {
                sourcePath = $@"{Smithbox.ProjectRoot}\{sourcePath}";
            }
            else
            {
                sourcePath = $@"{Smithbox.GameRoot}\{sourcePath}";
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
