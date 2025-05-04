using Octokit;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework;

public class MsbBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public MsbMeta Meta;

    public MsbBank(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        Meta = new MsbMeta(BaseEditor, Project);

        // META
        Task<bool> metaTask = LoadMeta();
        bool metaTaskResult = await metaTask;

        return true;
    }

    public async Task<bool> LoadMeta()
    {
        await Task.Delay(1);

        Meta._MsbMetas = new();

        var metaPath = $"{AppContext.BaseDirectory}\\Assets\\MSB\\{ProjectUtils.GetGameDirectory(Project)}\\Meta";

        //TaskLogs.AddLog($"metaPath: {metaPath}");

        if (Path.Exists(metaPath))
        {
            foreach (var folder in Directory.EnumerateDirectories(metaPath))
            {
                //TaskLogs.AddLog($"folder: {folder}");

                var rootType = new DirectoryInfo(folder).Name;

                //TaskLogs.AddLog($"rootType: {rootType}");

                var typeMetaPath = $"{metaPath}\\{rootType}";
                //TaskLogs.AddLog($"typeMetaPath: {typeMetaPath}");

                if (Path.Exists(typeMetaPath))
                {
                    foreach (var file in Directory.EnumerateFiles(typeMetaPath))
                    {
                        var currentPath = file;
                        var specificType = Path.GetFileNameWithoutExtension(file);

                        //TaskLogs.AddLog($"currentPath: {currentPath}");

                        var newMeta = new MapEntityPropertyMeta(currentPath);
                        Meta._MsbMetas.Add($"{rootType}_{specificType}", newMeta);
                    }
                }
            }
        }

        return true;
    }
}
