using StudioCore.Banks.AliasBank;
using StudioCore.Core;
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
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load: {EnumTitle} Bank: {e.Message}");
        }

        TaskLogs.AddLog($"Alias Bank: Loaded {EnumTitle} Bank");
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

        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\";

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
        var enumFileDir = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\";
        var enumFilePath = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";

        var baseResourcePath = AppContext.BaseDirectory + enumFilePath;
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;
        var projectResourceDir = Smithbox.SmithboxDataRoot + enumFileDir;

        if (!File.Exists(baseResourcePath))
        {
            PlatformUtils.Instance.MessageBox($"Failed to find base Enums.json.", "Error", MessageBoxButtons.OK);
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
        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";
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
        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";
        var baseResourcePath = AppContext.BaseDirectory + enumFilePath;
        var projectResourcePath = Smithbox.SmithboxDataRoot + enumFilePath;

        if (File.Exists(projectResourcePath))
        {
            var baseResource = LoadEnumResource(baseResourcePath);
            var projectResource = LoadEnumResource(projectResourcePath);
            var newResource = new ProjectEnumResource();

            foreach (var entry in projectResource.List)
            {
                var newEntry = new ProjectEnumEntry();

                foreach(var bEntry in baseResource.List)
                {
                    if(currentEntry.Name == bEntry.Name)
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
                            if(!bEntry.Options.Any(e => e.ID == opt.ID))
                            {
                                newOptions.Add(opt);
                            }
                        }

                        newEntry.Options = newOptions;
                    }
                }

                newResource.List.Add(newEntry);
            }

            WriteEnumResource(newResource);
        }

        LoadBank();
    }

    public void UpdateEnumEntryOption(ProjectEnumEntry newEntry, ProjectEnumOption newOption)
    {
        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";
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
        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";
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
        CreateProjectEnumFile();

        var enumFilePath = $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";
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

    private Dictionary<string, string> enumDict;

    public Dictionary<string, string> GetEnumDictionary(string enumType)
    {
        TaskLogs.AddLog(enumType);
        if (enumDict == null)
        {
            enumDict = new Dictionary<string, string>();
            foreach (var entry in Enums.List)
            {
                if(entry.Name == enumType)
                {
                    foreach(var opt in entry.Options)
                    {
                        enumDict[opt.ID] = opt.Name;
                    }
                }
            }
        }

        return enumDict;
    }
}
