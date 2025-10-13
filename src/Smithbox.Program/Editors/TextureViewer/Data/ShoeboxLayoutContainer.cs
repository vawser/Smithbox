﻿using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class ShoeboxLayoutContainer
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionaryEntry FileEntry;

    public Dictionary<string, ShoeboxLayout> Layouts = new Dictionary<string, ShoeboxLayout>();

    public Dictionary<string, List<SubTexture>> Textures = new Dictionary<string, List<SubTexture>>();

    public ShoeboxLayoutContainer(Smithbox baseEditor, ProjectEntry project, FileDictionaryEntry fileEntry)
    {
        BaseEditor = baseEditor;
        Project = project;

        FileEntry = fileEntry;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        try
        {
            var shoeboxData = Project.FS.ReadFile(FileEntry.Path);

            LoadLayouts(FileEntry, (Memory<byte>)shoeboxData);
            BuildTextureDictionary();
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to load Shoebox Layout: {FileEntry.Filename}", LogLevel.Error, Tasks.LogPriority.High, ex);
        }

        return true;
    }

    public void LoadLayouts(FileDictionaryEntry fileEntry, Memory<byte> data)
    {
        try
        {
            var binder = BND4.Read(data);

            foreach (var file in binder.Files)
            {
                if (file.Name.Contains(".layout"))
                {
                    ShoeboxLayout newLayout = new ShoeboxLayout(file);

                    if (!Layouts.ContainsKey(file.Name))
                    {
                        Layouts.Add(file.Name, newLayout);
                    }
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load Shoebox Layout: {FileEntry.Filename}", LogLevel.Error, Tasks.LogPriority.High, e);
        }
    }

    public async Task<bool> SetupLayoutsDirectly()
    {
        await Task.Yield();

        var srcFolder = Path.Combine(Common.FileLocations.Assets, "PARAM", ProjectUtils.GetGameDirectory(Project), "Icon Layouts");

        foreach (var path in Directory.EnumerateFiles(srcFolder))
        {
            var filename = Path.GetFileName(path);

            if (filename.Contains(".layout"))
            {
                ShoeboxLayout newLayout = new ShoeboxLayout(path);

                if (!Layouts.ContainsKey(filename))
                {
                    Layouts.Add(filename, newLayout);
                }
            }
        }

        BuildTextureDictionary();

        return true;
    }

    public void BuildTextureDictionary()
    {
        foreach (var entry in Layouts)
        {
            foreach (var tex in entry.Value.TextureAtlases)
            {
                var path = Path.GetFileNameWithoutExtension(tex.ImagePath);
                string Name = path;

                if (!Textures.ContainsKey(Name))
                {
                    Textures.Add(Name, tex.SubTextures);
                }
            }
        }
    }
}
