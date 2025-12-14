using Andre.IO.VFS;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.FileBrowserNS;
using StudioCore.Formats.JSON;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.ModelEditor;

public class ModelBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Models = new();

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> MapPieces = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Characters = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Assets = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Parts = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Collisions = new();

    public ModelBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Map Pieces
        foreach (var entry in Project.ModelData.MapPieceFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);
            newEntry.DeriveMapID();

            MapPieces.Add(entry, newEntry);
            Models.Add(entry, newEntry);
        }

        // Characters
        foreach (var entry in Project.ModelData.ChrFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);
            Characters.Add(entry, newEntry);
            Models.Add(entry, newEntry);
        }

        // Assets
        foreach (var entry in Project.ModelData.AssetFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);
            Assets.Add(entry, newEntry);
            Models.Add(entry, newEntry);
        }

        // Parts
        foreach (var entry in Project.ModelData.PartFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);
            Parts.Add(entry, newEntry);
            Models.Add(entry, newEntry);
        }

        // Collisions
        foreach (var entry in Project.ModelData.CollisionFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);
            Collisions.Add(entry, newEntry);
            Models.Add(entry, newEntry);
        }

        return true;
    }
}

public class ModelContainerWrapper
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string MapID { get; set; }

    public string Name { get; set; }
    public string Path { get; set; }

    public List<ModelWrapper> Models { get; set; }

    public ModelContainerWrapper(Smithbox baseEditor, ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        BaseEditor = baseEditor;
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;

        Models = new();
    }

    /// <summary>
    /// Used to store the map ID for map pieces and collision wrappers
    /// </summary>
    public void DeriveMapID()
    {
        var parts = Path.Split("/");

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            // '/map/m30/m30_00_00_00/...'
            if(parts.Length >= 3)
            {
                MapID = parts[3];
            }
        }
        else
        {
            // '/map/m30_00_00_00/...'
            if (parts.Length >= 2)
            {
                MapID = parts[2];
            }
        }
    }

    public void PopulateModelList()
    {
        Models.Clear();

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, Path);

        if (binderType is ResourceContainerType.None)
        {
            var modelWrapper = new ModelWrapper(this, Name);
            Models.Add(modelWrapper);
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var fileData = TargetFS.ReadFile(Path);
                    if (fileData != null)
                    {
                        var binder = new BND3Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                var modelWrapper = new ModelWrapper(this, filename);
                                Models.Add(modelWrapper);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var fileData = TargetFS.ReadFile(Path);
                    if (fileData != null)
                    {
                        var binder = new BND4Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                var modelWrapper = new ModelWrapper(this, filename);
                                Models.Add(modelWrapper);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = Path;
            var targetBdtPath = Path.Replace("bhd", "bdt");

            try
            {
                bhd = (Memory<byte>)TargetFS.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBhdPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)TargetFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBdtPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (Project.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = new BXF3Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            var modelWrapper = new ModelWrapper(this, filename);
                            Models.Add(modelWrapper);
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

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            var modelWrapper = new ModelWrapper(this, filename);
                            Models.Add(modelWrapper);
                        }
                    }
                }
            }
        }
    }
}

public class ModelWrapper
{
    public ModelContainerWrapper Parent { get; set; }

    public string Name { get; set; }

    public FLVER2 FLVER { get; set; }

    public ModelContainer Container { get; set; }

    public ModelWrapper(ModelContainerWrapper parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    public void Load()
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Parent.Project, Parent.Path);

        if (binderType is ResourceContainerType.None)
        {
            var fileData = Parent.TargetFS.ReadFile(Parent.Path);
            if (fileData != null)
            {
                FLVER = FLVER2.Read(fileData.Value);
            }
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (Parent.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var fileData = Parent.TargetFS.ReadFile(Parent.Path);
                    if (fileData != null)
                    {
                        var binder = new BND3Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                if (filename == Name)
                                {
                                    var flverData = binder.ReadFile(file);
                                    FLVER = FLVER2.Read(flverData);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Parent.Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var fileData = Parent.TargetFS.ReadFile(Parent.Path);
                    if (fileData != null)
                    {
                        var binder = new BND4Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                if (filename == Name)
                                {
                                    var flverData = binder.ReadFile(file);
                                    FLVER = FLVER2.Read(flverData);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Parent.Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = Parent.Path;
            var targetBdtPath = Parent.Path.Replace("bhd", "bdt");

            try
            {
                bhd = (Memory<byte>)Parent.TargetFS.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBhdPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)Parent.TargetFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBdtPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (Parent.Project.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = new BXF3Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            if (filename == Name)
                            {
                                var flverData = binder.ReadFile(file);
                                FLVER = FLVER2.Read(flverData);
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

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            if (filename == Name)
                            {
                                var flverData = binder.ReadFile(file);
                                FLVER = FLVER2.Read(flverData);
                            }
                        }
                    }
                }
            }
        }

        if (FLVER != null)
        {
            Parent.Project.ModelEditor.Universe.LoadModel(this);
        }
    }


    public void Unload()
    {
        Parent.Project.ModelEditor.Universe.UnloadModel(this);
    }
}