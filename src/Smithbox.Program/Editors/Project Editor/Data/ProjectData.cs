using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Logger;
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
            Smithbox.Log(this,
                LOC.Get("PRJ_DAT_Setup_Aliases_PASS", Project.Descriptor.ProjectName));
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("PRJ_DAT_Setup_Aliases_FAIL", Project.Descriptor.ProjectName));
        }

        return true;
    }

    public async void ReloadAliases()
    {
        // Aliases
        Task<bool> aliasesTask = SetupAliases();
        bool aliasesSetup = await aliasesTask;

        if (aliasesSetup)
        {
            Smithbox.Log(this,
                LOC.Get("PRJ_DAT_Reload_Aliases_PASS", Project.Descriptor.ProjectName));
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("PRJ_DAT_Reload_Aliases_FAIL", Project.Descriptor.ProjectName));
        }
    }

    /// <summary>
    /// Setup the alias store for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupAliases()
    {
        await Task.Yield();

        Aliases = new();

        HashSet<string> sourceDirectories = new();

        if(CFG.Current.Project_Alias_Editor_Use_Base_Source)
        {
            var baseDir = Path.Join(AppContext.BaseDirectory, "Assets", "Aliases", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));

            sourceDirectories.Add(baseDir);
        }

        if (CFG.Current.Project_Alias_Editor_Use_Project_Source)
        {
            var projectDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

            sourceDirectories.Add(projectDir);
        }

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
                    Smithbox.LogError(this,
                        LOC.Get("PRJ_DAT_Deseralize_Alias_FAIL", sourceFile), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("PRJ_DAT_Read_Alias_FAIL", sourceFile), e);
            }
        }

        foreach ((ProjectAliasType type, List<AliasEntry> entries) in Aliases)
        {
            Aliases[type] = entries.OrderBy(e => e.ID).ToList();
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Aliases = null;
    }
    #endregion
}