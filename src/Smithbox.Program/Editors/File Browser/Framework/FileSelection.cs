using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.FileBrowser;

public class FileSelection
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    public FolderNode SelectedVfsFolder = null;
    public FileDictionaryEntry SelectedVfsFile = null;

    public List<string> InternalFileList = new();
    public Dictionary<string, List<string>> InternalTextureList = new();

    public string SelectedInternalFile = "";
    public string SelectedInternalTexFile = "";

    public FileSelection(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void UpdateVfsFileSelection(FileDictionaryEntry targetFile)
    {
        SelectedVfsFile = targetFile;
        InternalFileList = new();
        InternalTextureList = new();

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, targetFile.Path);

        if (binderType is ResourceContainerType.None)
        {
            var fileData = Project.VFS.VanillaFS.ReadFile(targetFile.Path);
            if (fileData != null)
            {
                if(LocatorUtils.IsTPF(targetFile.Path))
                {
                    var tpfData = TPF.Read(fileData.Value);
                    foreach(var entry in tpfData.Textures)
                    {
                        if(!InternalTextureList.ContainsKey(targetFile.Path))
                        {
                            InternalTextureList.Add(targetFile.Path, new List<string>() { entry.Name });
                        }
                        else
                        {
                            InternalTextureList[targetFile.Path].Add(entry.Name);
                        }
                    }
                }
            }
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var fileData = Project.VFS.VanillaFS.ReadFile(targetFile.Path);
                    if (fileData != null)
                    {
                        var binder = new BND3Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            InternalFileList.Add(filepath);

                            if (LocatorUtils.IsTPF(filepath))
                            {
                                var internalFileData = binder.ReadFile(file);
                                var tpfData = TPF.Read(internalFileData);
                                foreach (var entry in tpfData.Textures)
                                {
                                    if (!InternalTextureList.ContainsKey(filepath))
                                    {
                                        InternalTextureList.Add(filepath, new List<string>() { entry.Name });
                                    }
                                    else
                                    {
                                        InternalTextureList[filepath].Add(entry.Name);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var fileData = Project.VFS.VanillaFS.ReadFile(targetFile.Path);
                    if (fileData != null)
                    {
                        var binder = new BND4Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            InternalFileList.Add(filepath);

                            if (LocatorUtils.IsTPF(filepath))
                            {
                                var internalFileData = binder.ReadFile(file);
                                var tpfData = TPF.Read(internalFileData);
                                foreach (var entry in tpfData.Textures)
                                {
                                    if (!InternalTextureList.ContainsKey(filepath))
                                    {
                                        InternalTextureList.Add(filepath, new List<string>() { entry.Name });
                                    }
                                    else
                                    {
                                        InternalTextureList[filepath].Add(entry.Name);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = targetFile.Path;
            var targetBdtPath = targetFile.Path.Replace("bhd", "bdt");

            try
            {
                bhd = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (Project.Descriptor.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = new BXF3Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        InternalFileList.Add(filepath);

                        if (LocatorUtils.IsTPF(filepath))
                        {
                            var internalFileData = binder.ReadFile(file);
                            var tpfData = TPF.Read(internalFileData);
                            foreach (var entry in tpfData.Textures)
                            {
                                if (!InternalTextureList.ContainsKey(filepath))
                                {
                                    InternalTextureList.Add(filepath, new List<string>() { entry.Name });
                                }
                                else
                                {
                                    InternalTextureList[filepath].Add(entry.Name);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var binder = new BXF4Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        InternalFileList.Add(filepath);

                        if (LocatorUtils.IsTPF(filepath))
                        {
                            var internalFileData = binder.ReadFile(file);
                            var tpfData = TPF.Read(internalFileData);
                            foreach (var entry in tpfData.Textures)
                            {
                                if (!InternalTextureList.ContainsKey(filepath))
                                {
                                    InternalTextureList.Add(filepath, new List<string>() { entry.Name });
                                }
                                else
                                {
                                    InternalTextureList[filepath].Add(entry.Name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
