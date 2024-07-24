using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Localization;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ProjectEnumBank;

/// <summary>
/// This is for user-modifiable enum lists for fields such as SubCategory and StateInfo, where the lists may want to include project-specific values that are not applicable to other projects.
/// </summary>
public class ProjectEnumBank
{
    public ProjectEnumResource Enums { get; set; }

    private string TemplateName = "Template.json";

    private string EnumTitle = "";

    public ProjectEnumBank(string title)
    {
        EnumTitle = title;


    }

    public void LoadBank()
    {
        try
        {
            Enums = BankUtils.LoadProjectEnumJSON();
            foreach(var entry in Enums.List)
            {
                entry.Options.Sort();
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog(
                $"{LOC.Get("PROJECT_ENUM_BANK__FAILED_TO_LOAD")}" +
                $" {EnumTitle} " +
                $"{LOC.Get("PROJECT_ENUM_BANK__BANK")}" +
                $"{e.Message}");
        }

        TaskLogs.AddLog($"{LOC.Get("PROJECT_ENUM_BANK__SUCCESSFUL_LOAD")}");
    }

    public Dictionary<string, ProjectEnumEntry> GetEntries()
    {
        Dictionary<string, ProjectEnumEntry> Entries = new Dictionary<string, ProjectEnumEntry>();

        if (Enums.List != null)
        {
            foreach (var entry in Enums.List)
            {
                if (!Entries.ContainsKey(entry.Name))
                {
                    Entries.Add(entry.Name, entry);
                }
            }
        }

        return Entries;
    }

    public ProjectEnumResource LoadEnumResource(string path)
    {
        var newResource = new ProjectEnumResource();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize(stream, ProjectEnumResourceSerializationContext.Default.ProjectEnumResource);
            }
        }

        return newResource;
    }

    public void WriteEnumResource(ProjectEnumResource targetBank)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\";

        var resourceFilePath = $"{resourcePath}\\Enums.json";

        if (File.Exists(resourceFilePath))
        {
            string jsonString = JsonSerializer.Serialize(targetBank, typeof(ProjectEnumResource), ProjectEnumResourceSerializationContext.Default);

            try
            {
                var fs = new FileStream(resourceFilePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    public void CreateProjectEnumFile()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var enumFileDir = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\";
        var enumFilePath = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";

        var baseResourcePath = AppContext.BaseDirectory + enumFilePath;
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;
        var projectResourceDir = Smithbox.SmithboxDataRoot + enumFileDir;

        if (!File.Exists(baseResourcePath))
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("PROJECT_ENUM_BANK__FAILED_TO_FIND_ENUM_JSON")}", 
                $"{LOC.Get("ERROR")}", 
                MessageBoxButtons.OK);
            return;
        }

        // Create directory/file if they don't exist
        if (!Directory.Exists(projectResourceDir))
        {
            Directory.CreateDirectory(projectResourceDir);
        }

        if (!File.Exists(projectResourcePath))
        {
            File.Copy(baseResourcePath, projectResourcePath);
        }
    }

    public void UpdateEnumEntry(ProjectEnumEntry newEntry)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;

        if (File.Exists(projectResourcePath))
        {
            var targetResource = LoadEnumResource(projectResourcePath);
            var newResource = new ProjectEnumResource();

            foreach (var entry in targetResource.List)
            {
                if (entry.Name == newEntry.Name)
                {
                    entry.DisplayName = newEntry.DisplayName;
                    entry.Description = newEntry.Description;
                    entry.Options = newEntry.Options;
                }

                newResource.List.Add(entry);
            }

            WriteEnumResource(newResource);
        }

        LoadBank();
    }

    public void RestoreBaseEnumEntry(ProjectEnumEntry currentEntry)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";
        var baseResourcePath = AppContext.BaseDirectory + enumFilePath;
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;

        if (File.Exists(projectResourcePath))
        {
            var baseResource = LoadEnumResource(baseResourcePath);
            var projectResource = LoadEnumResource(projectResourcePath);
            var newResource = new ProjectEnumResource();

            foreach (var entry in projectResource.List)
            {
                if (entry.Name == currentEntry.Name)
                {
                    var newEntry = new ProjectEnumEntry();

                    foreach (var bEntry in baseResource.List)
                    {
                        if (entry.Name == bEntry.Name)
                        {
                            newEntry.Name = bEntry.Name;
                            newEntry.DisplayName = bEntry.DisplayName;
                            newEntry.Description = bEntry.Description;

                            // Restore options from base, but retain project-unique ones
                            var newOptions = new List<ProjectEnumOption>();

                            foreach (var bOpt in bEntry.Options)
                            {
                                newOptions.Add(bOpt);
                            }

                            foreach (var opt in currentEntry.Options)
                            {
                                if (!bEntry.Options.Any(e => e.ID == opt.ID))
                                {
                                    newOptions.Add(opt);
                                }
                            }

                            newEntry.Options = newOptions;
                        }
                    }

                    newResource.List.Add(newEntry);
                }
                else
                {
                    newResource.List.Add(entry);
                }
            }

            WriteEnumResource(newResource);
        }

        LoadBank();
    }

    public void UpdateEnumEntryOption(ProjectEnumEntry newEntry, ProjectEnumOption newOption)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;

        if (File.Exists(projectResourcePath))
        {
            var targetResource = LoadEnumResource(projectResourcePath);
            var newResource = new ProjectEnumResource();

            foreach (var entry in targetResource.List)
            {
                if (entry.Name == newEntry.Name)
                {
                    if(entry.Options.Where(e => e.ID == newOption.ID).Any())
                    {
                        foreach (var opt in entry.Options)
                        {
                            if(opt.ID == newOption.ID)
                            {
                                opt.Name = newOption.Name;
                                opt.Description = newOption.Description;
                            }
                        }
                    }
                    else
                    {
                        entry.Options.Add(newOption);
                    }
                }

                newResource.List.Add(entry);
            }

            WriteEnumResource(newResource);
        }

        LoadBank();
    }

    public void RemoveEnumEntry(ProjectEnumEntry oldEntry)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;

        if (File.Exists(projectResourcePath))
        {
            // Load up the target local model alias bank. 
            var targetResource = LoadEnumResource(projectResourcePath);

            var newEntries = new List<ProjectEnumEntry>();

            foreach(var entry in targetResource.List)
            {
                if(entry.Name == oldEntry.Name)
                {
                    continue;
                }
                else
                {
                    newEntries.Add(entry);
                }
            }

            targetResource.List = newEntries;

            WriteEnumResource(targetResource);
        }

        LoadBank();
    }

    public void RemoveEnumEntryOption(ProjectEnumEntry oldEntry, ProjectEnumOption oldOption)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;

        if (File.Exists(projectResourcePath))
        {
            var targetResource = LoadEnumResource(projectResourcePath);

            var newEntries = new List<ProjectEnumEntry>();

            foreach (var entry in targetResource.List)
            {
                var newOptions = new List<ProjectEnumOption>();

                foreach(var opt in entry.Options)
                {
                    if(opt.ID == oldOption.ID)
                    {
                        continue;
                    }
                    else
                    {
                        newOptions.Add(opt);
                    }
                }

                entry.Options = newOptions;

                newEntries.Add(entry);
            }

            targetResource.List = newEntries;

            WriteEnumResource(targetResource);
        }

        LoadBank();
    }

    private Dictionary<string, Dictionary<string, string>> enumDict;

    public Dictionary<string, string> GetEnumDictionary(string enumType)
    {
        if(enumDict == null)
        {
            enumDict = new Dictionary<string, Dictionary<string, string>>();
        }

        if(!enumDict.ContainsKey(enumType))
        {
            var innerDict = new Dictionary<string, string>();
            foreach (var entry in Enums.List)
            {
                if (entry.Name == enumType)
                {
                    foreach (var opt in entry.Options)
                    {
                        innerDict[opt.ID] = opt.Name;
                    }
                }
            }

            enumDict.Add(enumType, innerDict);
        }

        if(enumDict.ContainsKey(enumType))
        {
            return enumDict[enumType];
        }

        return new Dictionary<string, string>();
    }
}
