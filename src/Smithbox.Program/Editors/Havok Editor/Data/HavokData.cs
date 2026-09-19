using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Editors.HavokEditor;

public class HavokData : IDisposable
{
    public ProjectEntry Project;

    public HavokObjectAliases HavokObjectAliases = new();

    public HavokData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> objectAliasesTask = SetupHavokObjectAliases();
        bool objectAliasesTaskResult = await objectAliasesTask;

        if (!objectAliasesTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Havok_Object_Aliases_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Havok_Object_Aliases_PASS"));
        }

        return true;
    }

    public async Task<bool> SetupHavokObjectAliases()
    {
        await Task.Yield();

        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "HAVOK", "Aliases", ProjectUtils.GetGameDirectory(Project));

        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "HAVOK", "Aliases");

        HavokObjectAliases = new();

        Dictionary<string, string> targetFilePaths = new();

        // Get target alias files from source, and thne if they exist in project, overwrite the target path for said file key
        if (Directory.Exists(srcDir))
        {
            foreach (var filepath in Directory.EnumerateFiles(srcDir))
            {
                var filename = Path.GetFileName(filepath);
                targetFilePaths.Add(filename, filepath);
            }
        }

        if (Directory.Exists(projDir))
        {
            foreach (var filepath in Directory.EnumerateFiles(projDir))
            {
                var filename = Path.GetFileName(filepath);

                if(targetFilePaths.ContainsKey(filename))
                {
                    targetFilePaths[filename] = filepath;
                }
                else
                {
                    targetFilePaths.Add(filename, filepath);
                }
            }
        }

        foreach (var entry in targetFilePaths)
        {
            var filename = entry.Key;
            var filepath = entry.Value;

            try
            {
                var filestring = await File.ReadAllTextAsync(filepath);

                var item = JsonSerializer.Deserialize(filestring, HavokJsonSerializerContext.Default.HavokAliasFileEntry);

                if (item != null)
                {
                    HavokObjectAliases.Files.Add(item);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_Data_Havok_Object_Aliases_Fail_Read", filepath), e);
            }
        }

        return true;
    }

    public string GetHavokObjectName(string filePath, string havokObjectID)
    {
        var name = "";
        var files = Project.Handler.HavokData.HavokObjectAliases.Files;

        var fileEntry = files.FirstOrDefault(e => e.Path == filePath);
        if (fileEntry != null)
        {
            foreach (var alias in fileEntry.Aliases)
            {
                if (alias.ID == havokObjectID)
                {
                    name = alias.Name;
                }
            }
        }

        return name;
    }

    public void UpdateHavokObjectName(HavokCategoryMode category, FileDictionaryEntry binderEntry, string filePath, string havokObjectID, string havokObjectName)
    {
        var srcDir = Path.Combine(ParamDebugTools.ProjectFolder,
            "src", "Smithbox.Data", "Assets", "HAVOK", "Aliases",
            ProjectUtils.GetGameDirectory(Project));

        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "HAVOK", "Aliases");

        var targetDir = projDir;

        if (CFG.Current.Developer_Enable_Tools)
        {
            targetDir = srcDir;
        }

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var files = Project.Handler.HavokData.HavokObjectAliases.Files;

        if (files.Any(e => e.Path == filePath))
        {
            var fileEntry = files.FirstOrDefault(e => e.Path == filePath);

            if (fileEntry.Aliases.Any(e => e.ID == havokObjectID))
            {
                for (int i = 0; i < fileEntry.Aliases.Count; i++)
                {
                    var curEntry = fileEntry.Aliases[i];

                    if (curEntry.ID == havokObjectID)
                    {
                        curEntry.Name = havokObjectName;
                    }
                }
            }
            else
            {
                var nameEntry = new HavokAliasEntry();
                nameEntry.ID = havokObjectID;
                nameEntry.Name = havokObjectName;

                fileEntry.Aliases.Add(nameEntry);
            }
        }
        else
        {
            var nameEntry = new HavokAliasEntry();
            nameEntry.ID = havokObjectID;
            nameEntry.Name = havokObjectName;

            var fileEntry = new HavokAliasFileEntry();
            fileEntry.Path = filePath;
            fileEntry.Aliases = new() { nameEntry };

            Project.Handler.HavokData.HavokObjectAliases.Files.Add(fileEntry);
        }

        if (files.Any(e => e.Path == filePath))
        {
            var fileEntry = files.FirstOrDefault(e => e.Path == filePath);

            if (Directory.Exists(targetDir))
            {
                var filename = GetNiceFileName(category, binderEntry, filePath);
                var targetFile = Path.Combine(targetDir, $"{filename}.json");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };

                var jsonString = JsonSerializer.Serialize(fileEntry, typeof(HavokAliasFileEntry), options);

                File.WriteAllText(targetFile, jsonString);
            }
        }
    }

    public string GetNiceFileName(HavokCategoryMode category, FileDictionaryEntry binderEntry, string filePath)
    {
        var filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filePath));

        return $"{category.ToString()}-{binderEntry.Filename}-{filename}";
    }

    #region Dispose
    public void Dispose()
    {
    }
    #endregion

}
