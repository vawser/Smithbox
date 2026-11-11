using StudioCore.Application;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexCommandQueue
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

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
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        // Packed (.tpfbhd) containers are listed apart from the others
        var targetFile = Project.Locator.TextureFiles.Entries.FirstOrDefault(e => e.Filename == filename)
            ?? Project.Locator.TexturePackedFiles.Entries?.FirstOrDefault(e => e.Filename == filename);

        if (targetFile == null)
            return;

        Task<bool> loadTask = Project.Handler.TextureData.PrimaryBank.LoadTextureBinder(targetFile);

        Task.WaitAll(loadTask);

        var targetBinder = Project.Handler.TextureData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Path == targetFile.Path);

        if (targetBinder.Key == null || targetBinder.Value == null)
            return;

        activeView.Selection.SelectTextureFile(targetBinder.Key, targetBinder.Value);

        // TPF: the one named like the file, or the only one (a loose .tpf)
        var tpfSelected = false;
        foreach (var entry in targetBinder.Value.Files)
        {
            var binderFile = entry.Key;

            if (binderFile.Name == filename ||
                string.Equals(Path.GetFileNameWithoutExtension(binderFile.Name ?? ""), filename, StringComparison.OrdinalIgnoreCase))
            {
                activeView.Selection.SelectTpfFile(entry.Key, entry.Value);
                tpfSelected = true;
                break;
            }
        }

        if (!tpfSelected && targetBinder.Value.Files.Count == 1)
        {
            var only = targetBinder.Value.Files.First();
            activeView.Selection.SelectTpfFile(only.Key, only.Value);
        }

        if (activeView.Selection.SelectedTpf == null)
            return;

        // Texture
        int index = 0;
        int targetIndex = 0;

        foreach (var entry in activeView.Selection.SelectedTpf.Textures)
        {
            if (entry.Name == textureName)
            {
                targetIndex = index;
                activeView.Selection.SelectTextureEntry(entry.Name, entry);

                // TODO: fix this not properly working: the texture entry needs to be pressed again for the texture to appear after the editor switch
                activeView.TextureList.LoadTexture = true;
                break;
            }

            index++;
        }
    }
}
