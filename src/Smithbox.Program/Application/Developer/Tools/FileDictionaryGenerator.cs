using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Text.Json;

namespace StudioCore.Application;

public static class FileDictionaryGenerator
{
    public static string _filePath = "";

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (ImGui.BeginTable($"generatorTable", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

            // File Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("File Path");
            UIHelper.Tooltip("The file path of the file.");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            ImGui.InputText("##generatorPath", ref _filePath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##generatorPathSelect", DPI.StandardButtonSize))
            {
                var newFilePath = "";
                var result = PlatformUtils.Instance.OpenFileDialog("Select File", [""], out newFilePath);

                if (result)
                {
                    _filePath = newFilePath;
                }
            }

            ImGui.EndTable();
        }

        if (File.Exists(_filePath))
        {
            if (ImGui.Button("Generate File Dictionary JSON", DPI.StandardButtonSize))
            {
                GenerateFileDictionaryFromUXM(_filePath);
            }
        }
    }

    /// <summary>
    /// Used to a FileDictionary JSON object from a standard UXM file dictionary text file.
    /// </summary>
    /// <param name="filepath"></param>
    public static void GenerateFileDictionaryFromUXM(string filepath)
    {
#if MACOS
        var writePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{Path.GetFileName(filepath)}.json";;
#else
        var writePath = $"{AppContext.BaseDirectory}/{Path.GetFileName(filepath)}.json";
#endif
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
