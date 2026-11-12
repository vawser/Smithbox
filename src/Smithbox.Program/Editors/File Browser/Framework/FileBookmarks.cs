using StudioCore.Application;
using StudioCore.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.FileBrowser;

/// <summary>
/// Pinned files and folders, per project. Kept as one path per line in the project's .smithbox folder, so they
/// travel with the project and need no serializer setup. Folders end in '/'.
/// </summary>
public class FileBookmarks
{
    private readonly ProjectEntry Project;

    private readonly List<string> Paths = new();

    public IReadOnlyList<string> Entries => Paths;

    public FileBookmarks(ProjectEntry project)
    {
        Project = project;
        Load();
    }

    private string FilePath => Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "FileBrowserBookmarks.txt");

    public static string FolderKey(string folderPath) => folderPath.EndsWith('/') ? folderPath : folderPath + "/";

    public bool Contains(string key) => Paths.Contains(key, StringComparer.OrdinalIgnoreCase);

    public void Toggle(string key)
    {
        var existing = Paths.FindIndex(p => string.Equals(p, key, StringComparison.OrdinalIgnoreCase));

        if (existing >= 0)
            Paths.RemoveAt(existing);
        else
            Paths.Add(key);

        Save();
    }

    public void Remove(string key)
    {
        if (Paths.RemoveAll(p => string.Equals(p, key, StringComparison.OrdinalIgnoreCase)) > 0)
            Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return;

            foreach (var line in File.ReadAllLines(FilePath))
            {
                var trimmed = line.Trim();
                if (trimmed != "" && !Contains(trimmed))
                    Paths.Add(trimmed);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Bookmarks_Load_Failed", FilePath), e);
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllLines(FilePath, Paths);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Bookmarks_Save_Failed", FilePath), e);
        }
    }
}
