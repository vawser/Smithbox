using StudioCore.Core;
using StudioCore.Resource.Types;
using StudioCore.Resource;
using StudioCore.TextureViewer;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexCommandQueue
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexCommandQueue(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Parse the editor command queue for this editor.
    /// </summary>
    public void Parse(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 1)
        {
            // View Image:
            // e.g. "texture/view/01_common/SB_GarageTop_04"
            if (initcmd[0] == "view" && initcmd.Length >= 3)
            {
                var fileName = initcmd[1];
                var textureName = initcmd[2];

                HandleView(fileName, textureName);
            }
        }
    }

    public void HandleView(string filename, string textureName)
    {
        var targetFile = Project.TextureData.TextureFiles.Entries.FirstOrDefault(e => e.Filename == filename);

        if (targetFile == null)
            return;

        Task<bool> loadTask = Project.TextureData.PrimaryBank.LoadTextureBinder(targetFile);

        Task.WaitAll(loadTask);

        var targetBinder = Project.TextureData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == targetFile.Filename);

        if (targetBinder.Key != null)
        {
            Editor.Selection.SelectTextureFile(targetBinder.Key, targetBinder.Value);
        }

        // TPF
        foreach (var entry in targetBinder.Value.Files)
        {
            var binderFile = entry.Key;
            var tpfEntry = entry.Value;

            if (binderFile.Name == filename)
            {
                Editor.Selection.SelectTpfFile(entry.Key, entry.Value);
                break;
            }
        }

        // Texture
        int index = 0;
        int targetIndex = 0;

        foreach (var entry in Editor.Selection.SelectedTpf.Textures)
        {
            if (entry.Name == textureName)
            {
                targetIndex = index;
                Editor.Selection.SelectTextureEntry(entry.Name, entry);

                // TODO: fix this not properly working: the texture entry needs to be pressed again for the texture to appear after the editor switch
                Editor.TextureView.LoadTexture = true;
                break;
            }

            index++;
        }
    }
}
