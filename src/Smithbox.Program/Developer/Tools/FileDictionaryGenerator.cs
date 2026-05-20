using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Text.Json;

namespace StudioCore.Application;

public class FileDictionaryGenerator
{
    public string _filePath = "";

    public FileDictionaryGenerator() { }

    public void Display()
    {
        UIHelper.SimpleHeader("File Path", "");
        UIHelper.SinglelineTextInput("FilePath", ref _filePath);

        UIHelper.MultiButtonInput("selectActions",
            "selectDir", "Select Directory", "", SelectDirectory,
            "generateDict", "Generate File Dictionary", "", GenerateFileDictionary);
    }

    public void SelectDirectory()
    {
        var newFilePath = "";
        var result = PlatformUtils.Instance.OpenFileDialog("Select File", [""], out newFilePath);

        if (result)
        {
            _filePath = newFilePath;
        }
    }

    public void GenerateFileDictionary()
    {
        if (File.Exists(_filePath))
        {
            GenerateFileDictionaryFromUXM(_filePath);
        }
        else
        {
            Smithbox.LogError<FileDictionaryGenerator>($"{_filePath} does not exist.");
        }
    }

    /// <summary>
    /// Used to a FileDictionary JSON object from a standard UXM file dictionary text file.
    /// </summary>
    /// <param name="filepath"></param>
    public void GenerateFileDictionaryFromUXM(string filepath)
    {
        var writePath = $"{AppContext.BaseDirectory}/{Path.GetFileName(filepath)}.json";

        var curDictionary = new FileDictionary();
        curDictionary.Entries = new();

        var file = File.ReadAllText(filepath);
        var contents = file.Split("\n");

        var currentArchive = "";

        foreach (var line in contents)
        {
            if (line == "" || line == " ")
                continue;

            if (line.StartsWith("#"))
            {
                currentArchive = line.Replace("#", "");
            }
            else
            {
                var newEntry = new FileDictionaryEntry();
                newEntry.Archive = currentArchive.Replace("\r", "");
                newEntry.Path = line.Replace("\r", "");

                if (newEntry.Path != "")
                {
                    newEntry.Folder = Path.GetDirectoryName(newEntry.Path).Replace('\\', '/'); ;
                    if (line.Contains(".dcx"))
                    {
                        var extension = Path.GetExtension(Path.GetFileNameWithoutExtension(newEntry.Path));
                        var fileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(line));
                        newEntry.Filename = Path.GetFileName(fileName);

                        if (extension != "")
                            newEntry.Extension = Path.GetFileName(extension).Substring(1, extension.Length - 1);
                        else
                            newEntry.Extension = "";
                    }
                    else
                    {
                        var extension = Path.GetExtension(newEntry.Path);
                        var fileName = Path.GetFileNameWithoutExtension(line);
                        newEntry.Filename = Path.GetFileName(fileName);

                        if (extension != "")
                            newEntry.Extension = Path.GetFileName(extension).Substring(1, extension.Length - 1);
                        else
                            newEntry.Extension = "";
                    }

                    curDictionary.Entries.Add(newEntry);
                }
            }
        }

        var json = JsonSerializer.Serialize(curDictionary, ProjectJsonSerializerContext.Default.FileDictionary);

        File.WriteAllText(writePath, json);
    }
}
