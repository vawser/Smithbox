using DotNext.Collections.Generic;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Octokit;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Data;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ModelBank PrimaryBank;

    public FileDictionary MapPieceFiles = new();
    public FileDictionary ChrFiles = new();
    public FileDictionary AssetFiles = new();
    public FileDictionary PartFiles = new();
    public FileDictionary CollisionFiles = new();

    public FileDictionary MapFiles = new();

    public ModelData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
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

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Model Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        return primaryBankTaskResult;
    }

    public void SetupFileDictionaries()
    {
        // Maps (for the map pieces)
        MapFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map") && !e.Folder.Contains("autoroute"))
            .Where(e => e.Extension == "msb")
            .Where(e => !e.Archive.Contains("sd"))
            .ToList();

        // Characters
        if (Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ChrFiles.Entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith("/model/chr"))
                    .Where(e => e.Extension == "bnd")
                    .ToList();
        }
        else
        {
            ChrFiles.Entries = Project.FileDictionary.Entries
                    .Where(e => e.Extension == "chrbnd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
        }

        // Assets / Objects
        if (Project.ProjectType is ProjectType.DS1)
        {
            AssetFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/obj"))
                .Where(e => e.Extension == "objbnd")
                .ToList();
        }
        else if (Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            AssetFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/obj"))
                .Where(e => e.Extension == "bnd")
                .ToList();
        }
        else if (Project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            AssetFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/obj"))
                .Where(e => e.Extension == "objbnd")
                .ToList();
        }
        else if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            AssetFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/asset"))
                .Where(e => e.Extension == "geombnd")
                .Where(e => !e.Archive.Contains("sd"))
                .ToList();
        }

        // Parts
        if (Project.ProjectType is ProjectType.DS1)
        {
            PartFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .ToList();
        }
        else if (Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            PartFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/parts"))
                .Where(e => e.Extension == "bnd")
                .ToList();
        }
        else if (Project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            PartFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .ToList();
        }
        else if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            PartFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .Where(e => !e.Archive.Contains("sd"))
                .ToList();
        }

        // Collisions
        if (Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            CollisionFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/map"))
                .Where(e => e.Extension == "hkxbhd")
                .ToList();
        }

        foreach (var map in MapFiles.Entries)
        {
            var mapid = map.Filename;
            var entries = new List<FileDictionaryEntry>();

            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DES)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkx")
                    .ToList();
            }
            else if (Project.ProjectType is ProjectType.DS1R)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .ToList();
            }
            else if (Project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .ToList();
            }
            else if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                entries = Project.FileDictionary.Entries
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

        if (Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            MapPieceFiles.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/map"))
                .Where(e => e.Extension == "mapbhd")
                .ToList();
        }

        foreach (var map in MapFiles.Entries)
        {
            var mapid = map.Filename;
            var entries = new List<FileDictionaryEntry>();

            if (Project.ProjectType is ProjectType.DS1)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.ProjectType is ProjectType.DS1R)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.ProjectType is ProjectType.BB or ProjectType.DES)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}/"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                entries = Project.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}"))
                    .Where(e => e.Extension == "mapbnd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
            }
            else
            {
                entries = Project.FileDictionary.Entries
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
}
