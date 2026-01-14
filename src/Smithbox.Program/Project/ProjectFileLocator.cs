using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectFileLocator : IDisposable
{
    public ProjectEntry Project;

    public FileDictionary FileDictionary;

    public ProjectFileLocator(ProjectEntry project)
    {
        Project = project;
    }

    public async Task Initialize()
    {
        var projectLocalFolder = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox");

        if (!Directory.Exists(projectLocalFolder))
        {
            Directory.CreateDirectory(projectLocalFolder);
        }

        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "File Dictionaries");
        var file = "";

        // Build the file dictionary JSON objects here
        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                file = "DES-File-Dictionary.json"; break;
            case ProjectType.DS1:
                file = "DS1-File-Dictionary.json"; break;
            case ProjectType.DS1R:
                file = "DS1R-File-Dictionary.json"; break;
            case ProjectType.DS2:
                file = "DS2-File-Dictionary.json"; break;
            case ProjectType.DS2S:
                file = "DS2S-File-Dictionary.json"; break;
            case ProjectType.DS3:
                file = "DS3-File-Dictionary.json"; break;
            case ProjectType.BB:
                file = "BB-File-Dictionary.json"; break;
            case ProjectType.SDT:
                file = "SDT-File-Dictionary.json"; break;
            case ProjectType.ER:
                file = "ER-File-Dictionary.json"; break;
            case ProjectType.AC6:
                file = "AC6-File-Dictionary.json"; break;
            case ProjectType.NR:
                file = "NR-File-Dictionary.json"; break;
            default: break;
        }

        var filepath = Path.Join(folder, file);

        var jsonFileDictionary = new FileDictionary();

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(filepath);

                try
                {
                    jsonFileDictionary = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"Failed to deserialize the file dictionary: {filepath}", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"Failed to read the file dictionary: {filepath}", e);
            }
        }

        var projectFileDictionary = BuildFromSource(
            Project.Descriptor.ProjectPath, 
            jsonFileDictionary, Project.Descriptor.ProjectType);

        FileDictionary = MergeFileDictionaries(jsonFileDictionary, projectFileDictionary);

        return;
    }

    #region Utils
    private FileDictionary BuildFromSource(string sourcePath, FileDictionary existingDict, ProjectType type)
    {
        var fileDict = new FileDictionary { Entries = new() };

        if (!Directory.Exists(sourcePath))
            return fileDict;

        var existingPaths = new HashSet<string>(
            existingDict.Entries.Select(e => e.Path),
            StringComparer.OrdinalIgnoreCase);

        var excludedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        excludedDirs.Add(".git");
        excludedDirs.Add(".smithbox");

        foreach (var manifest in Directory.EnumerateFiles(sourcePath, "_witchy-*.xml", SearchOption.AllDirectories))
        {
            var dir = Path.GetDirectoryName(manifest);
            while (!string.IsNullOrEmpty(dir))
            {
                if (!excludedDirs.Add(dir))
                    break;

                dir = Path.GetDirectoryName(dir);
            }
        }

        var archiveName = Path.GetFileName(sourcePath);

        foreach (var filePath in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null && excludedDirs.Contains(dir))
                continue;

            var normalizedDir = NormalizePath(dir);

            var relativePath = "/" + Path.GetRelativePath(sourcePath, filePath).Replace('\\', '/');

            if (existingPaths.Contains(relativePath))
                continue;

            var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            if (ext == "dcx")
            {
                var prevExt = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(prevExt))
                {
                    ext = prevExt.TrimStart('.').ToLowerInvariant();
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                }
            }

            if (type == ProjectType.ER)
            {
                if (relativePath.StartsWith("/menu/deploy", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (ext == "matbinbnd")
                    continue;
            }

            fileDict.Entries.Add(new FileDictionaryEntry
            {
                Archive = archiveName,
                Path = relativePath,
                Folder = Path.GetDirectoryName(relativePath)?.Replace('\\', '/'),
                Filename = fileName,
                Extension = ext
            });
        }

        return fileDict;
    }

    private FileDictionary MergeFileDictionaries(FileDictionary first, FileDictionary second)
    {
        var combined = new FileDictionary();
        combined.Entries = new();

        // Normalize and track unique paths
        var seenPaths = new HashSet<string>(
            first.Entries
                 .Select(e => NormalizePath(e.Path))
                 .Where(p => p != null),
            StringComparer.OrdinalIgnoreCase);

        combined.Entries.AddRange(first.Entries);

        foreach (var entry in second.Entries)
        {
            var normalizedPath = NormalizePath(entry.Path);
            if (normalizedPath != null && !seenPaths.Contains(normalizedPath))
            {
                combined.Entries.Add(entry);
                seenPaths.Add(normalizedPath);
            }
        }

        combined.Entries = combined.Entries.OrderBy(e => e.Filename).ToList();
        return combined;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? null
            : path.Trim().Replace('\\', '/'); // normalize separators and trim
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        FileDictionary.Entries.Clear();
        FileDictionary = null;
    }
    #endregion
}