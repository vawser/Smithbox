using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelData : IDisposable
{
    public ProjectEntry Project;

    public ModelBank PrimaryBank;

    public FileDictionary MapPieceFiles = new();
    public FileDictionary ChrFiles = new();
    public FileDictionary AssetFiles = new();
    public FileDictionary PartFiles = new();
    public FileDictionary CollisionFiles = new();

    public FileDictionary MapFiles = new();

    public FormatResource FlverInformation;
    public FormatEnum FlverEnums;

    public ModelData(ProjectEntry project)
    {
        Project = project;

        MapPieceFiles.Entries = new();
        ChrFiles.Entries = new();
        AssetFiles.Entries = new();
        PartFiles.Entries = new();
        CollisionFiles.Entries = new();
        MapFiles.Entries = new();
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        SetupFileDictionaries();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        // FLVER Information
        Task<bool> flverInfoTask = SetupFlverInfo();
        bool flverInfoResult = await flverInfoTask;

        if (flverInfoResult)
        {
            TaskLogs.AddLog($"[Model Editor] Setup FLVER information.");
        }
        else
        {
            TaskLogs.AddError($"[Model Editor] Failed to setup FLVER information.");
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddError($"[Model Editor] Failed to fully setup Primary Bank.");
        }

        return primaryBankTaskResult;
    }

    public void SetupFileDictionaries()
    {
        // Maps (for the map pieces)
        MapFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map") && !e.Folder.Contains("autoroute"))
            .Where(e => e.Extension == "msb")
            .Where(e => !e.Archive.Contains("sd"))
            .ToList();

        // Characters
        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ChrFiles.Entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith("/model/chr"))
                    .Where(e => e.Extension == "bnd")
                    .ToList();
        }
        else
        {
            ChrFiles.Entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Extension == "chrbnd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
        }

        // Assets / Objects
        if (Project.Descriptor.ProjectType is ProjectType.DS1)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/obj"))
                .Where(e => e.Extension == "objbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/obj"))
                .Where(e => e.Extension == "bnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/obj"))
                .Where(e => e.Extension == "objbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/asset"))
                .Where(e => e.Extension == "geombnd")
                .Where(e => !e.Archive.Contains("sd"))
                .ToList();
        }

        // Parts
        if (Project.Descriptor.ProjectType is ProjectType.DS1)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/parts"))
                .Where(e => e.Extension == "bnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .Where(e => !e.Archive.Contains("sd"))
                .ToList();
        }

        // Collisions
        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            CollisionFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/map"))
                .Where(e => e.Extension == "hkxbhd")
                .ToList();
        }

        foreach (var map in MapFiles.Entries)
        {
            var mapid = map.Filename;
            var entries = new List<FileDictionaryEntry>();

            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DES)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkx")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS1R)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
            }

            foreach (var entry in entries)
            {
                CollisionFiles.Entries.Add(entry);
            }
        }

        // Map Parts
        MapPieceFiles.Entries = new();

        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            MapPieceFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/map"))
                .Where(e => e.Extension == "mapbhd")
                .ToList();
        }

        foreach (var map in MapFiles.Entries)
        {
            var mapid = map.Filename;
            var entries = new List<FileDictionaryEntry>();

            if (Project.Descriptor.ProjectType is ProjectType.DS1)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS1R)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.BB or ProjectType.DES)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}/"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}"))
                    .Where(e => e.Extension == "mapbnd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
            }
            else
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "mapbnd")
                    .ToList();
            }

            foreach(var entry in entries)
            {
                MapPieceFiles.Entries.Add(entry);
            }
        }
    }

    /// <summary>
    /// Setup the FLVER information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupFlverInfo()
    {
        await Task.Yield();

        FlverInformation = new();
        FlverEnums = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "FLVER");
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "FLVER");
        var projectFile = Path.Combine(projectFolder, "Core.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    FlverInformation = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Model Editor] Failed to deserialize the FLVER information", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Model Editor] Failed to read the FLVER information: {targetFile}", e);
            }
        }

        // Enums
        sourceFile = Path.Combine(sourceFolder, "Enums.json");

        projectFile = Path.Combine(projectFolder, "Enums.json");

        targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    FlverEnums = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Model Editor] Failed to deserialize the FLVER enums: {targetFile}", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Model Editor] Failed to read the FLVER enums: {targetFile}", e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();

        PrimaryBank = null;

        MapPieceFiles = null;
        ChrFiles = null;
        AssetFiles = null;
        PartFiles = null;
        CollisionFiles = null;
        MapFiles = null;

        FlverInformation = null;
        FlverEnums = null;
    }
    #endregion
}
