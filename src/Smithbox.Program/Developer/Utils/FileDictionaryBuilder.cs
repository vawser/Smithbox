using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Application;

public static class FileDictionaryBuilder
{
    public static void BuildFileDictionary()
    {
        var baseDir = @"G:\Game Data Repository\Duskbloods\0400d1d02399a000-BebTjl\base\romfs";

        var fileDictionary = new FileDictionary { Entries = new() };

        foreach (var filePath in Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories))
        {
            var dir = Path.GetDirectoryName(filePath);
            var normalizedDir = NormalizePath(dir);

            var relativePath = "/" + Path.GetRelativePath(baseDir, filePath).Replace('\\', '/');
            var relativeFolder = Path.GetDirectoryName(relativePath).Replace('\\', '/');

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

            fileDictionary.Entries.Add(new FileDictionaryEntry
            {
                Archive = "",
                Path = relativePath,
                Folder = Path.GetDirectoryName(relativePath)?.Replace('\\', '/'),
                Filename = fileName,
                Extension = ext
            });
        }

        // Write out file dictionary
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };
        var json = JsonSerializer.Serialize(fileDictionary, typeof(FileDictionary), options);

        var outputDir = @"C:\Users\benja\Programming\C#\Smithbox\src\Smithbox.Data\Assets\File Dictionaries\new_dict.json";

        File.WriteAllText(outputDir, json);
    }

    public static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? null
            : path.Trim().Replace('\\', '/'); // normalize separators and trim
    }
}
