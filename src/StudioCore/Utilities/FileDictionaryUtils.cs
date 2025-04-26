using StudioCore.Core.ProjectNS;
using StudioCore.Resources.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public class FileDictionaryUtils
{
    public static List<FileDictionaryEntry> GetFinalDictionaryEntries(List<FileDictionaryEntry> root, List<FileDictionaryEntry> project)
    {
        var existingPaths = new HashSet<string>(root.Select(e => e.Path));

        var merged = new List<FileDictionaryEntry>(root);
        merged.AddRange(project.Where(e => !existingPaths.Contains(e.Path)));

        return merged;
    }

    public static List<FileDictionaryEntry> GetUniqueFileEntries(Project curProject, string filter)
    {
        string rootDirectory = $"{curProject.ProjectPath}";

        var entries = new List<FileDictionaryEntry>();
        var files = Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string relativePath = Path.GetRelativePath(rootDirectory, file);
            string archive = rootDirectory;
            string folder = Path.GetDirectoryName(relativePath)?.Replace("\\", "/") ?? "";
            string fileName = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file).TrimStart('.').ToLower();

            if (extension != filter)
                continue;

            // Handle .dcx case
            if (extension == "dcx")
            {
                var baseName = Path.GetFileNameWithoutExtension(file); // strip .dcx
                string basePath = Path.Combine(Path.GetDirectoryName(file), baseName);
                extension = Path.GetExtension(basePath).TrimStart('.').ToLower();
                fileName = Path.GetFileNameWithoutExtension(basePath);
            }

            entries.Add(new FileDictionaryEntry
            {
                Archive = archive,
                Path = relativePath.Replace("\\", "/"),
                Folder = folder,
                Filename = fileName,
                Extension = extension
            });
        }

        return entries;
    }
}
