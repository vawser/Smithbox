using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using System;
using System.Diagnostics;
using System.IO;

namespace StudioCore.Editors.FileBrowser;

/// <summary>
/// Actions on a single file or folder, used by the File List's context menus
/// </summary>
public class FileActions
{
    private readonly FileBrowserScreen Editor;
    private readonly ProjectEntry Project;

    public FileActions(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Where a dictionary path sits in the project folder
    /// </summary>
    public string GetProjectPath(string relPath)
    {
        return Path.Combine(Project.Descriptor.ProjectPath, relPath.Replace('\\', '/').TrimStart('/'));
    }

    /// <summary>
    /// Copies the game's version of a file into the project at the same path: the usual first step of modding it.
    /// Never replaces a file the project already has.
    /// </summary>
    public void CopyToProject(FileDictionaryEntry entry)
    {
        var target = GetProjectPath(entry.Path);

        if (File.Exists(target))
        {
            Smithbox.Log(this, LOC.Get("FILE_Actions_Copy_To_Project_Exists", entry.Path));
            return;
        }

        try
        {
            var data = Project.VFS.VanillaFS.ReadFile(entry.Path);
            if (data == null)
            {
                Smithbox.LogError(this, LOC.Get("FILE_Actions_Copy_To_Project_Not_Found", entry.Path));
                return;
            }

            // Through the project VFS, so its write hooks (the DES uncompressed twin) apply as for any editor save
            Project.VFS.ProjectFS.WriteFile(entry.Path, data.Value.ToArray());

            Editor.Tree.MarkProjectFile(entry.Path);

            Smithbox.Log(this, LOC.Get("FILE_Actions_Copy_To_Project_Done", entry.Path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Actions_Copy_To_Project_Failed", entry.Path), LogPriority.High, e);
        }
    }

    /// <summary>
    /// Opens Explorer with the file selected (project copy first, then a loose game file), or its folder
    /// </summary>
    public void ShowInExplorer(string relPath, bool isFolder)
    {
        var candidates = new[]
        {
            GetProjectPath(relPath),
            Path.Combine(Project.Descriptor.DataPath, relPath.Replace('\\', '/').TrimStart('/'))
        };

        foreach (var candidate in candidates)
        {
            var full = Path.GetFullPath(candidate);

            if (isFolder && Directory.Exists(full))
            {
                Process.Start("explorer.exe", $"\"{full}\"");
                return;
            }

            if (!isFolder && File.Exists(full))
            {
                Process.Start("explorer.exe", $"/select,\"{full}\"");
                return;
            }
        }

        Smithbox.Log(this, LOC.Get("FILE_Actions_Show_In_Explorer_Missing", relPath));
    }

    /// <summary>
    /// Whether a file on disk exists for Show in Explorer (project copy or a loose game file)
    /// </summary>
    public bool ExistsOnDisk(string relPath, bool isFolder)
    {
        var rel = relPath.Replace('\\', '/').TrimStart('/');
        var project = Path.Combine(Project.Descriptor.ProjectPath, rel);
        var game = Path.Combine(Project.Descriptor.DataPath, rel);

        return isFolder
            ? Directory.Exists(project) || Directory.Exists(game)
            : File.Exists(project) || File.Exists(game);
    }

    /// <summary>
    /// The editor command that opens a file, and the editor's display name, or null when no editor takes this file.
    /// Uses the editors' existing load/view commands, which find the file by its name in their own lists.
    /// </summary>
    public string GetOpenCommand(FileDictionaryEntry entry, string textureName, out string editorName)
    {
        editorName = "";
        var ext = (entry.Extension ?? "").ToLowerInvariant();
        var name = entry.Filename;

        switch (ext)
        {
            case "msb":
                editorName = LOC.Get("FILE_Actions_Editor_Map");
                return $"map/load/{name}";

            case "flver":
            case "chrbnd":
            case "objbnd":
            case "partsbnd":
            case "geombnd":
            case "mapbnd":
                editorName = LOC.Get("FILE_Actions_Editor_Model");
                return $"model/load/{name}";

            case "tpf":
            case "tpfbhd":
            case "texbnd":
                editorName = LOC.Get("FILE_Actions_Editor_Texture");
                return $"texture/view/{name}/{textureName ?? ""}";

            case "gparam":
                editorName = LOC.Get("FILE_Actions_Editor_Gparam");
                return $"gparam/view/{name}";
        }

        return null;
    }

    public void OpenInEditor(FileDictionaryEntry entry, string textureName)
    {
        var command = GetOpenCommand(entry, textureName, out _);
        if (command == null)
            return;

        EditorCommandQueue.AddCommand(command);
    }
}
