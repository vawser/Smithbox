using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

public class ProjectData : IDisposable
{
    public ProjectEntry Project;


    public AliasStore Aliases;
    public ProjectEnumResource ProjectEnums;

    public ProjectData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Aliases
        Task<bool> aliasesTask = SetupAliases();
        bool aliasesSetup = await aliasesTask;

        if (aliasesSetup)
        {
            TaskLogs.AddLog($"[{Project.Descriptor.ProjectName}] Setup aliases.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.Descriptor.ProjectName}] Failed to setup aliases.");
        }

        // Project Enums (per project)
        Task<bool> projectParamEnumTask = SetupProjectEnums();
        bool projectParamEnumResult = await projectParamEnumTask;

        if (projectParamEnumResult)
        {
            TaskLogs.AddLog($"[{Project.Descriptor.ProjectName}] Setup Project Param Enums.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.Descriptor.ProjectName}] Failed to setup Project Param Enums.");
        }

        return true;
    }

    /// <summary>
    /// Setup the alias store for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupAliases()
    {
        await Task.Yield();

        Aliases = new();

        HashSet<string> sourceDirectories =
        [
            Path.Join(AppContext.BaseDirectory, "Assets", "Aliases", 
            ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType)),
            Path.Join(Project.Descriptor.ProjectPath,".smithbox","Assets","Aliases")
        ];

        List<string> sourceFiles = sourceDirectories.Where(Directory.Exists).Select(dir => Directory.GetFiles(dir, "*.json")).SelectMany(f => f).ToList();

        foreach (string sourceFile in sourceFiles)
        {
            try
            {
                if (!Enum.TryParse(Path.GetFileNameWithoutExtension(sourceFile), out ProjectAliasType type)) continue;
                string text = await File.ReadAllTextAsync(sourceFile);
                try
                {
                    var entries = JsonSerializer.Deserialize(text, ProjectJsonSerializerContext.Default.ListAliasEntry);
                    if (!Aliases.ContainsKey(type))
                    {
                        Aliases.TryAdd(type, entries);
                        continue;
                    }
                    Aliases[type] = entries.UnionBy(Aliases[type], e => e.ID).ToList();
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the aliases: {sourceFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the aliases: {sourceFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        foreach ((ProjectAliasType type, List<AliasEntry> entries) in Aliases)
        {
            Aliases[type] = entries.OrderBy(e => e.ID).ToList();
        }

        return true;
    }

    /// <summary>
    /// Setup the project-specific PARAM enums for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupProjectEnums()
    {
        await Task.Yield();

        ProjectEnums = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Shared Param Enums.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Project");
        var projectFile = Path.Combine(projectFolder, "Shared Param Enums.json");

        var targetFile = sourceFile;

        if (CFG.Current.Project_Enable_Project_Metadata)
        {
            if (File.Exists(projectFile))
            {
                targetFile = projectFile;
            }
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    ProjectEnums = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.ProjectEnumResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Project Enums: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Project Enums: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Aliases = null;
        ProjectEnums = null;
    }
    #endregion
}